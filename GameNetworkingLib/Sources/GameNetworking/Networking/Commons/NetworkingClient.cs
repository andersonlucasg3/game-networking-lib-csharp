using GameNetworking.Commons.Models;
using Messages.Models;
using Networking.Commons;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Networking.Commons {
    public interface INetworkingClient<TSocket, TClient, TNetClient, TListener>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient>
        where TListener : INetworkingClient<TSocket, TClient, TNetClient, TListener>.IListener {

        public interface IListener {
            void NetworkingClientDidReadMessage(MessageContainer container);
        }

        TClient client { get; }

        IListener listener { get; set; }
    }

    public abstract class NetworkingClient<TNetworking, TSocket, TClient, TNetClient, TListener> : INetworkingClient<TSocket, TClient, TNetClient, TListener>, INetClient<TSocket, TNetClient>.IListener
        where TNetworking : INetworking<TSocket, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient>
        where TListener : INetworkingClient<TSocket, TClient, TNetClient, TListener>.IListener {

        protected TNetworking networking { get; }

        public TClient client { get; protected set; }

        public INetworkingClient<TSocket, TClient, TNetClient, TListener>.IListener listener { get; set; }

        public NetworkingClient(TNetworking backend) {
            this.networking = backend;
        }

        public void Send(ITypedMessage message) {
            this.client?.write(message);
        }

        public void Update() {
            if (this.client == null || this.client.client == null) { return; }
            this.networking.Read(this.client.client);
            this.networking.Flush(this.client.client);
        }

        #region INetClientReadDelegate

        void INetClient<TSocket, TNetClient>.IListener.ClientDidReadBytes(TNetClient client, byte[] bytes) {
            this.client.reader.Add(bytes);
            MessageContainer container;
            do {
                container = this.client.reader.Decode();
                if (container == null) { continue; }
                this.listener?.NetworkingClientDidReadMessage(container);
            } while (container != null);
        }

        #endregion
    }
}