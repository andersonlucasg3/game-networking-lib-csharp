using Networking;
using Networking.Models;
using Messages.Streams;
using Messages.Models;
using System;
using System.Collections.Generic;
using Commons;
using System.Threading;

namespace GameNetworking.Networking {
    using Models;

    internal class NetworkingServer: WeakDelegate<INetworkingServerDelegate> {
        private readonly INetworking networking;
        private WeakReference weakMessagesDelegate;

        private readonly List<NetworkClient> clientsStorage;

        private Thread proletariatThread;

        public INetworkingServerMessagesDelegate MessagesDelegate {
            get { return this.weakMessagesDelegate?.Target as INetworkingServerMessagesDelegate; }
            set { this.weakMessagesDelegate = new WeakReference(value); }
        }

        public NetworkingServer() {
            this.networking = new NetSocket();
            clientsStorage = new List<NetworkClient>();
        }

        public void Listen(int port) {
            this.networking.Start(port);
            CreateAndStartThread();
        }

        private void AcceptClient() {
            NetClient client = this.networking.Accept();
            if (client != null) {
                NetworkClient networkClient = new NetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
                clientsStorage.Add(networkClient);
                this.Delegate?.NetworkingServerDidAcceptClient(networkClient);
            }
        }

        public void Send(ITypedMessage encodable, NetworkClient client) {
            client.Write(encodable);
        }

        public void SendBroadcast(ITypedMessage encodable, List<NetworkClient> clients) {
            var writer = new MessageStreamWriter();
            var buffer = writer.Write(encodable);
            clients.ForEach(c => this.networking.Send(c.Client, buffer));
        }

        #region Private Methods

        private void CreateAndStartThread() {
            proletariatThread = new Thread(ThreadWork);
            proletariatThread.Start();
        }

        private bool ShouldKeepWorking() {
            // TODO:
            // there is no way to stop a server yet
            // when there is, add the check here
            var isAborted = this.proletariatThread?.ThreadState == ThreadState.AbortRequested || this.proletariatThread?.ThreadState == ThreadState.Aborted;
            return !isAborted;
        }

        private void ThreadWork() {
            do {
                this.AcceptClient();
                this.clientsStorage.ForEach((each) => {
                    this.Read(each);
                    this.Flush(each);
                });
            } while (ShouldKeepWorking());
        }

        private void Read(NetworkClient client) {
            byte[] bytes = this.networking.Read(client.Client);
            client.Reader.Add(bytes);

            MessageContainer message = null;
            do {
                message = client.Reader.Decode();
                if (message != null) {
                    this.MessagesDelegate?.NetworkingServerDidReadMessage(message, client);
                }
            } while (message != null);
        }

        private void Flush(NetworkClient client) {
            if (client.Client.IsConnected) {
                this.networking.Flush(client.Client);
            } else {
                this.Delegate?.NetworkingServerClientDidDisconnect(client);
                this.clientsStorage.Remove(client);
            }
        }

        #endregion
    }
}
