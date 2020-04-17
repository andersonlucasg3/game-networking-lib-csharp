using GameNetworking.Commons;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Commons.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking {
    public class UnreliableGameServer<TPlayer> : GameServer<UnreliableNetworkingServer, TPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient, UnreliableClientAcceptor<TPlayer>, UnreliableGameServer<TPlayer>>
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {

        public UnreliableGameServer(UnreliableNetworkingServer server, IMainThreadDispatcher dispatcher) 
            : base(server, new GameServerMessageRouter<UnreliableGameServer<TPlayer>, UnreliableNetworkingServer, TPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>(dispatcher)) {
            this.networkingServer.listener = this;
        }

        public void Disconnect(TPlayer player) {
            if (player == null || player.client == null) { return; }

            this.networkingServer.Disconnect(player.client);
        }
    }
}