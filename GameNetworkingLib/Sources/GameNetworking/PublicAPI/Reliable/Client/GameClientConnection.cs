using Messages.Models;
using GameNetworking.Models.Client;
using GameNetworking.Models.Contract.Client;
using GameNetworking.Commons;
using GameNetworking.Networking;

namespace GameNetworking {
    internal class GameClientConnection<PlayerType> : BaseExecutor<GameClient<PlayerType>>, INetworkingClientListener where PlayerType : INetworkPlayer, new() {
        internal GameClientConnection(GameClient<PlayerType> client, IMainThreadDispatcher dispatcher) : base(client, dispatcher) {
            client.networkingClient.listener = this;
        }

        internal void Connect(string host, int port) {
            this.instance?.networkingClient.Connect(host, port);
        }

        internal void Disconnect() {
            this.instance?.networkingClient.Disconnect();
        }

        #region INetworkingClientDelegate

        void INetworkingClientListener.NetworkingClientDidConnect() {
            this.instance?.listener?.GameClientDidConnect();
        }

        void INetworkingClientListener.NetworkingClientConnectDidTimeout() {
            this.instance?.listener?.GameClientConnectDidTimeout();
        }

        void INetworkingClientListener.NetworkingClientDidDisconnect() {
            this.instance?.listener?.GameClientDidDisconnect();
        }

        void INetworkingClientListener.NetworkingClientDidReadMessage(MessageContainer container) {
            this.instance?.GameClientConnectionDidReceiveMessage(container);
        }

        #endregion
    }
}