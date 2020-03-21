using System.Collections.Generic;
using GameNetworking.Commons.Models;
using Messages.Models;
using Messages.Streams;
using Networking.Commons;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Networking.Commons {
    public interface INetworkingServer<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        public interface IListener {
            void NetworkingServerDidAcceptClient(TClient client);
            void NetworkingServerClientDidDisconnect(TClient client);
        }

        public interface IMessagesListener {
            void NetworkingServerDidReadMessage(MessageContainer container, TClient client);
        }

        internal IListener listener { get; set; }
        IMessagesListener messagesListener { get; set; }

        void Start(int port);
        void Stop();

        void Update();

        void Send(ITypedMessage message, TClient client);
        void SendBroadcast(ITypedMessage message, List<TClient> clients);
    }

    public abstract class NetworkingServer<TNetworking, TSocket, TClient, TNetClient> : INetworkingServer<TSocket, TClient, TNetClient>, INetClient<TSocket, TNetClient>.IListener
        where TNetworking : INetworking<TSocket, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        private INetworkingServer<TSocket, TClient, TNetClient> self => this;

        protected List<TClient> clientsStorage { get; }
        protected Queue<TClient> disconnectedClientsToRemove { get; }

        protected TNetworking networking { get; private set; }

        INetworkingServer<TSocket, TClient, TNetClient>.IListener INetworkingServer<TSocket, TClient, TNetClient>.listener { get; set; }
        internal INetworkingServer<TSocket, TClient, TNetClient>.IListener listener { get => self.listener; set => self.listener = value; }

        public INetworkingServer<TSocket, TClient, TNetClient>.IMessagesListener messagesListener { get; set; }

        public NetworkingServer(TNetworking backend) {
            this.networking = backend;
            this.clientsStorage = new List<TClient>();
            this.disconnectedClientsToRemove = new Queue<TClient>();
        }

        public virtual void Start(int port) {
            this.networking.Start(port);
        }

        public virtual void Stop() {
            this.networking.Stop();
        }

        public virtual void Update() {
            TClient client;
            for (int i = 0; i < this.clientsStorage.Count; i++) {
                client = this.clientsStorage[i];
                this.Read(client);
                this.Flush(client);
            }
            this.RemoveDisconnected();
        }

        public void Send(ITypedMessage encodable, TClient client) {
            client.write(encodable);
        }

        public void SendBroadcast(ITypedMessage encodable, List<TClient> clients) {
            var writer = new MessageStreamWriter();
            var buffer = writer.Write(encodable);
            for (int i = 0; i < clients.Count; i++) {
                this.networking.Send(clients[i].client, buffer);
            }
        }

        #region Private methods

        private void Read(TClient client) {
            this.networking.Read(client.client);
        }

        private void Flush(TClient client) {
            this.networking.Flush(client.client);
        }

        private void RemoveDisconnected() {
            while (this.disconnectedClientsToRemove.Count > 0) {
                this.clientsStorage.Remove(this.disconnectedClientsToRemove.Dequeue());
            }
        }

        #endregion

        #region INetClient<TSocket, TClient>.IListener

        void INetClient<TSocket, TNetClient>.IListener.ClientDidReadBytes(TNetClient client, byte[] bytes) {
            var n_client = clientsStorage.Find((c) => c.Equals(client));
            n_client.reader.Add(bytes);

            MessageContainer message = null;
            do {
                message = n_client.reader.Decode();
                if (message != null) {
                    this.messagesListener?.NetworkingServerDidReadMessage(message, n_client);
                }
            } while (message != null);
        }

        #endregion
    }
}