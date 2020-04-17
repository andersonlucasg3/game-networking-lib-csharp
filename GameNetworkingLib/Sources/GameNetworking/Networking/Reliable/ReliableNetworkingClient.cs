using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Messages.Streams;
using Networking;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public interface IReliableNetworkingClientListener : INetworkingClientListener {
        void NetworkingClientDidConnect();
        void NetworkingClientConnectDidTimeout();
        void NetworkingClientDidDisconnect();
    }

    public class ReliableNetworkingClient : NetworkingClient<ReliableSocket, ITCPSocket, ReliableNetworkClient, ReliableNetClient>, IReliableSocketListener {

        public new IReliableNetworkingClientListener listener { get => (IReliableNetworkingClientListener)base.listener; set => base.listener = value; }

        public ReliableNetworkingClient(ReliableSocket backend) : base(backend) {
            this.networking.listener = this;
        }

        public void Connect(string host, int port) {
            this.networking.Connect(host, port);
        }

        public void Disconnect() {
            if (this.client?.client != null) {
                this.networking.Disconnect(this.client.client);
            }
        }

        #region INetworkingDelegate

        void IReliableSocketListener.NetworkingDidConnect(ReliableNetClient client) {
            client.listener = this;

            this.client = new ReliableNetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
            this.listener?.NetworkingClientDidConnect();
        }

        void IReliableSocketListener.NetworkingConnectDidTimeout() {
            this.client = null;
            this.listener?.NetworkingClientConnectDidTimeout();
        }

        void IReliableSocketListener.NetworkingDidDisconnect(ReliableNetClient client) {
            this.client = null;
            this.listener?.NetworkingClientDidDisconnect();
        }

        #endregion
    }
}
