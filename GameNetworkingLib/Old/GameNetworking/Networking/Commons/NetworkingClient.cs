using GameNetworking.Commons.Models;
using GameNetworking.Messages.Models;
using Networking.Commons;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Networking.Commons {
    public interface INetworkingClientListener {
        void NetworkingClientDidReadMessage(MessageContainer container);
    }

    public interface INetworkingClient<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        TClient client { get; }

        INetworkingClientListener listener { get; set; }

        void Send(ITypedMessage message);
        void Update();

        void Close();
    }

    public abstract class NetworkingClient<TNetworking, TSocket, TClient, TNetClient> : INetworkingClient<TSocket, TClient, TNetClient>, INetClientListener<TSocket, TNetClient>
        where TNetworking : INetworking<TSocket, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        protected TNetworking networking { get; }

        public TClient client { get; protected set; }

        public INetworkingClientListener listener { get; set; }

        public NetworkingClient(TNetworking backend) {
            this.networking = backend;
        }

        public void Send(ITypedMessage message) {
            this.client?.Write(message);
        }

        public virtual void Update() {
            if (this.client == null || this.client.client == null) { return; }
            this.networking.Read(this.client.client);
            this.networking.Flush(this.client.client);
        }

        public void Close() {
            this.networking.Stop();
        }

        #region INetClientListener

        void INetClientListener<TSocket, TNetClient>.ClientDidReadBytes(TNetClient client, byte[] bytes, int count) {
            this.client.reader.Add(bytes, count);
            MessageContainer message;
            while ((message = this.client.reader.Decode()) != null) {
                this.listener?.NetworkingClientDidReadMessage(message);
            }
        }

        #endregion
    }
}