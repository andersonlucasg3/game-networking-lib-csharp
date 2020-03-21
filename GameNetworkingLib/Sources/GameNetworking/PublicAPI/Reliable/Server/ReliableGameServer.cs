using GameNetworking.Networking.Commons;
using Networking.Sockets;
using Networking.Models;
using GameNetworking.Networking;
using GameNetworking.Commons.Server;
using GameNetworking.Networking.Models;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Commons;

namespace GameNetworking {
    public class ReliableGameServer<TPlayer> :
        GameServer<ReliableNetworkingServer, TPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>,
        INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener
        where TPlayer : NetworkPlayer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>, new() {

        public new interface IListener : GameServer<ReliableNetworkingServer, TPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener {
            void GameServerPlayerDidConnect(TPlayer player);
            void GameServerPlayerDidDisconnect(TPlayer player);
        }

        private readonly GameServerClientAcceptor<TPlayer> clientAcceptor;

        public new IListener listener { get => base.listener as IListener; set => base.listener = value; }

        public ReliableGameServer(ReliableNetworkingServer server, IMainThreadDispatcher dispatcher) : base(server, dispatcher) {
            this.clientAcceptor = new GameServerClientAcceptor<TPlayer>(this);
            this.networkingServer.listener = this;
        }

        public void Disconnect(TPlayer player) {
            this.networkingServer.Disconnect(player.client);
        }

        #region INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener

        void INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.NetworkingServerDidAcceptClient(ReliableNetworkClient client) {
            this.clientAcceptor.AcceptClient(client);
        }

        void INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.NetworkingServerClientDidDisconnect(ReliableNetworkClient client) {
            var player = this.playersStorage.Find(client);
            this.clientAcceptor.Disconnect(player);
        }

        #endregion
    }
}
