using GameNetworking.Commons;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Commons.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking {
    public class ReliableGameServer<TPlayer> : GameServer<ReliableNetworkingServer, TPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient, ReliableClientAcceptor<TPlayer>, ReliableGameServer<TPlayer>>
        where TPlayer : class, INetworkPlayer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>, new() {

        public ReliableGameServer(ReliableNetworkingServer server, IMainThreadDispatcher dispatcher) : base(server, new GameServerMessageRouter<ReliableGameServer<TPlayer>, ReliableNetworkingServer, TPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>(dispatcher)) {
            this.networkingServer.listener = this;
        }

        public void Disconnect(TPlayer player) {
            this.networkingServer.Disconnect(player.client);
        }
    }
}
