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

    internal class NetworkingServer: WeakDelegate<INetworkingServerDelegate>, INetClientReadDelegate {
        private readonly INetworking networking;
        private WeakReference weakMessagesDelegate;

        private readonly List<NetworkClient> clientsStorage;
        private List<NetworkClient> disconnectedClientsToRemove;

        public INetworkingServerMessagesDelegate MessagesDelegate {
            get { return this.weakMessagesDelegate?.Target as INetworkingServerMessagesDelegate; }
            set { this.weakMessagesDelegate = new WeakReference(value); }
        }

        public NetworkingServer() {
            this.networking = new NetSocket();
            this.clientsStorage = new List<NetworkClient>();
            this.disconnectedClientsToRemove = new List<NetworkClient>();
        }

        public void Listen(int port) {
            this.networking.Start(port);
        }

        private void AcceptClient() {
            NetClient client = this.networking.Accept();
            if (client != null) {
                client.Delegate = this;
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

        public void Update() {
            this.AcceptClient();
            this.clientsStorage.ForEach((each) => {
                this.Read(each);
                this.Flush(each);
            });
            this.RemoveDisconnected();
        }

        #region Private Methods

        private void Read(NetworkClient client) {
            this.networking.Read(client.Client);
        }

        private void Flush(NetworkClient client) {
            if (client.Client.IsConnected) {
                this.networking.Flush(client.Client);
            } else {
                this.Delegate?.NetworkingServerClientDidDisconnect(client);
                this.disconnectedClientsToRemove.Add(client);
            }
        }

        private void RemoveDisconnected() {
            if (this.disconnectedClientsToRemove.Count == 0) { return; }
            this.disconnectedClientsToRemove.ForEach((each) => this.clientsStorage.Remove(each));
            this.disconnectedClientsToRemove.Clear();
        }

        #endregion

        #region INetClientReadDelegate

        void INetClientReadDelegate.ClientDidReadBytes(NetClient client, byte[] bytes) {
            var n_client = clientsStorage.Find((c) => c.Equals(client));
            n_client.Reader.Add(bytes);

            MessageContainer message = null;
            do {
                message = n_client.Reader.Decode();
                if (message != null) {
                    this.MessagesDelegate?.NetworkingServerDidReadMessage(message, n_client);
                }
            } while (message != null);
        }

        #endregion
    }
}
