using System.Collections.Generic;
using GameNetworking.Commons.Models;
using GameNetworking.Messages.Models;
using Networking.Commons;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Networking.Commons {
    public interface INetworkingServerListener<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        void NetworkingServerDidAcceptClient(TClient client);
        void NetworkingServerClientDidDisconnect(TClient client);
    }

    public interface INetworkingServerMessagesListener<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        void NetworkingServerDidReadMessage(MessageContainer container, TClient client);
    }

    public interface INetworkingServer<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        IReadOnlyList<TClient> clients { get; }

        INetworkingServerListener<TSocket, TClient, TNetClient> listener { get; set; }
        INetworkingServerMessagesListener<TSocket, TClient, TNetClient> messagesListener { get; set; }

        void Start(string host, int port);
        void Stop();

        void Disconnect(TClient client);

        void Update();

        void Send(ITypedMessage message, TClient client);
        void SendBroadcast(ITypedMessage message, List<TClient> clients);
    }

    public abstract class NetworkingServer<TNetworking, TSocket, TClient, TNetClient> : INetworkingServer<TSocket, TClient, TNetClient>, INetClientListener<TSocket, TNetClient>
        where TNetworking : INetworking<TSocket, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        protected List<TClient> clientsList { get; }
        protected Dictionary<TNetClient, TClient> clientsCollection { get; }
        protected Queue<TClient> disconnectedClientsToRemove { get; }

        protected TNetworking networking { get; private set; }

        public INetworkingServerListener<TSocket, TClient, TNetClient> listener { get; set; }
        public INetworkingServerMessagesListener<TSocket, TClient, TNetClient> messagesListener { get; set; }

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
            if (client == null || client.client == null) { return; }
            this.networking.Flush(client.client);
        }

        protected virtual void TryReadMessage(byte[] bytes, int count, TClient client) {
            client.reader.Add(bytes, count);

            MessageContainer message;
            while ((message = client.reader.Decode()) != null) {
                this.ProcessMessage(message, client);
            }
        }

        protected virtual void ProcessMessage(MessageContainer message, TClient client) {
            this.messagesListener?.NetworkingServerDidReadMessage(message, client);
        }

        protected void RemoveDisconnected() {
            while (this.disconnectedClientsToRemove.Count > 0) {
                var removing = this.disconnectedClientsToRemove.Dequeue();
                this.clientsList.Remove(removing);
                this.clientsCollection.Remove(removing.client);
                removing.Close();
            }
        }

        #endregion

        #region INetClient<TSocket, TClient>.IListener

        void INetClientListener<TSocket, TNetClient>.ClientDidReadBytes(TNetClient client, byte[] bytes, int count) {
            if (!clientsCollection.TryGetValue(client, out TClient n_client)) { return; }
            this.TryReadMessage(bytes, count, n_client);
        }

        #endregion
    }
}