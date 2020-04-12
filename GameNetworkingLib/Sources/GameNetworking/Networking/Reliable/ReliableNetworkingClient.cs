using Networking;
using Networking.Models;
using Messages.Streams;
using Networking.Sockets;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;

namespace GameNetworking.Networking {
    public class ReliableNetworkingClient : NetworkingClient<ReliableSocket, ITCPSocket, ReliableNetworkClient, ReliableNetClient>, IReliableSocket.IListener {

        public interface IListener : INetworkingClient<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener {
            void NetworkingClientDidConnect();
            void NetworkingClientConnectDidTimeout();
            void NetworkingClientDidDisconnect();
        }

        public new IListener listener { get => (IListener)base.listener; set => base.listener = value; }

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

        void IReliableSocket.IListener.NetworkingDidConnect(ReliableNetClient client) {
            client.listener = this;

            this.client = new ReliableNetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
            this.listener?.NetworkingClientDidConnect();
        }

        void IReliableSocket.IListener.NetworkingConnectDidTimeout() {
            this.client = null;
            this.listener?.NetworkingClientConnectDidTimeout();
        }

        void IReliableSocket.IListener.NetworkingDidDisconnect(ReliableNetClient client) {
            this.client = null;
            this.listener?.NetworkingClientDidDisconnect();
        }

        #endregion
    }
}
