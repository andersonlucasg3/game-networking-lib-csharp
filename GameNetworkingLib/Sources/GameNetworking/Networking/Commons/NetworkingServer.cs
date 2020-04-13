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

        IReadOnlyList<TClient> clients { get; }

        internal IListener listener { get; set; }
        IMessagesListener messagesListener { get; set; }

        void Start(string host, int port);
        void Stop();

        void Disconnect(TClient client);

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

        protected List<TClient> clientsList { get; }
        protected Dictionary<TNetClient, TClient> clientsCollection { get; }
        protected Queue<TClient> disconnectedClientsToRemove { get; }

        protected TNetworking networking { get; private set; }

        INetworkingServer<TSocket, TClient, TNetClient>.IListener INetworkingServer<TSocket, TClient, TNetClient>.listener { get; set; }
        internal INetworkingServer<TSocket, TClient, TNetClient>.IListener listener { get => self.listener; set => self.listener = value; }

        public INetworkingServer<TSocket, TClient, TNetClient>.IMessagesListener messagesListener { get; set; }

        public IReadOnlyList<TClient> clients => this.clientsList;

        public NetworkingServer(TNetworking backend) {
            this.networking = backend;
            this.clientsList = new List<TClient>();
            this.clientsCollection = new Dictionary<TNetClient, TClient>();
            this.disconnectedClientsToRemove = new Queue<TClient>();
        }

        public virtual void Start(string host, int port) {
            this.networking.Start(host, port);
        }

        public virtual void Stop() {
            this.networking.Stop();
        }

        public virtual void Disconnect(TClient client) {
            this.disconnectedClientsToRemove.Enqueue(client);
        }

        public virtual void Update() {
            for (int i = 0; i < this.clientsList.Count; i++) {
                TClient client = this.clientsList[i];
                this.Read(client);
                this.Flush(client);
            }
            this.RemoveDisconnected();
        }

        public void Send(ITypedMessage encodable, TClient client) {
            client.Write(encodable);
        }

        public void SendBroadcast(ITypedMessage encodable, List<TClient> clients) {
            var writer = new MessageStreamWriter();
            var buffer = writer.Write(encodable);
            for (int i = 0; i < clients.Count; i++) {
                this.networking.Send(clients[i].client, buffer);
            }
        }

        #region Protected methods

        protected virtual void Read(TClient client) {
            this.networking.Read(client.client);
        }

        protected virtual void Flush(TClient client) {
            this.networking.Flush(client.client);
        }

        protected virtual void TryReadMessage(byte[] bytes, TClient client) {
            client.reader.Add(bytes);

            MessageContainer message;
            while ((message = client.reader.Decode()) != null) { 
                this.messagesListener?.NetworkingServerDidReadMessage(message, client);
            }
        }

        protected void RemoveDisconnected() {
            while (this.disconnectedClientsToRemove.Count > 0) {
                var removing = this.disconnectedClientsToRemove.Dequeue();
                this.clientsList.Remove(removing);
                this.clientsCollection.Remove(removing.client);
            }
        }

        #endregion

        #region Private methods

        #endregion

        #region INetClient<TSocket, TClient>.IListener

        void INetClient<TSocket, TNetClient>.IListener.ClientDidReadBytes(TNetClient client, byte[] bytes) {
            if (!clientsCollection.TryGetValue(client, out TClient n_client)) { return; }
            this.TryReadMessage(bytes, n_client);
        }

        #endregion
    }
}