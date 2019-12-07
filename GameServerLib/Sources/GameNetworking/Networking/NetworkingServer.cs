using Networking;
using Networking.Models;
using Messages.Streams;
using Messages.Models;
using System;
using System.Collections.Generic;
using Networking.IO;

namespace GameNetworking.Networking {
    using Models;

    internal class NetworkingServer : INetClientReadListener {
        private readonly INetworking networking;
        
        private readonly List<NetworkClient> clientsStorage;
        private Queue<NetworkClient> disconnectedClientsToRemove;

        internal INetworkingServerListener listener { get; set; }

        public INetworkingServerMessagesListener messagesListener { get; set; }

        public NetworkingServer(INetworking backend) {
            this.networking = backend;
            this.clientsStorage = new List<NetworkClient>();
            this.disconnectedClientsToRemove = new Queue<NetworkClient>();
        }

        public void Listen(int port) {
            this.networking.StartServer(port);
        }

        public void Stop() {
            for (int i = 0; i < this.clientsStorage.Count; i++) {
                this.Disconnect(this.clientsStorage[i]);
            }
            this.networking.Stop();
        }

        public void Disconnect(NetworkClient client) {
            this.networking.Disconnect(client.client);
        }

        private void AcceptClient() {
            INetClient client = this.networking?.Accept();
            if (client != null) {
                client.listener = this;
                NetworkClient networkClient = new NetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
                clientsStorage.Add(networkClient);
                this.listener?.NetworkingServerDidAcceptClient(networkClient);
            }
        }

        public void Send(ITypedMessage encodable, NetworkClient client) {
            client.Write(encodable);
        }

        public void SendBroadcast(ITypedMessage encodable, List<NetworkClient> clients) {
            var writer = new MessageStreamWriter();
            var buffer = writer.Write(encodable);
            for (int i = 0; i < clients.Count; i++) {
                this.networking.Send(clients[i].client, buffer);
            }
        }

        public void Update() {
            this.AcceptClient();
            NetworkClient client;
            for (int i = 0; i < this.clientsStorage.Count; i++) {
                client = this.clientsStorage[i];
                this.Read(client);
                this.Flush(client);
            }
            this.RemoveDisconnected();
        }

        #region Private Methods

        private void Read(NetworkClient client) {
            this.networking.Read(client.client);
        }

        private void Flush(NetworkClient client) {
            if (client.client.isConnected) {
                this.networking.Flush(client.client);
            } else {
                this.listener?.NetworkingServerClientDidDisconnect(client);
                this.disconnectedClientsToRemove.Enqueue(client);
            }
        }

        private void RemoveDisconnected() {
            while (this.disconnectedClientsToRemove.Count > 0) {
                this.clientsStorage.Remove(this.disconnectedClientsToRemove.Dequeue());
            }
        }

        #endregion

        #region INetClientReadDelegate

        void INetClientReadListener.ClientDidReadBytes(NetClient client, byte[] bytes) {
            var n_client = clientsStorage.Find((c) => c.Equals(client));
            n_client.Reader.Add(bytes);

            MessageContainer message = null;
            do {
                message = n_client.Reader.Decode();
                if (message != null) {
                    this.messagesListener?.NetworkingServerDidReadMessage(message, n_client);
                }
            } while (message != null);
        }

        #endregion
    }
}
