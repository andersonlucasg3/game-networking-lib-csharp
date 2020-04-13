using GameNetworking.Commons;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Commons.Server;
using GameNetworking.Messages.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking {
    public class UnreliableGameServer<TPlayer> : GameServer<UnreliableNetworkingServer, TPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient, UnreliableClientAcceptor<TPlayer>, UnreliableGameServer<TPlayer>>
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {

        public UnreliableGameServer(UnreliableNetworkingServer server, IMainThreadDispatcher dispatcher) : base(server, new UnreliableServerMessageRouter<TPlayer>(dispatcher)) {
            this.networkingServer.listener = this;
        }

        public void Disconnect(TPlayer player) {
            this.Send(new UnreliableDisconnectResponseMessage(), player);
        }

        internal void DisconnectRequired(TPlayer player) {
            if (player == null || player.client == null) { return; }

            this.Disconnect(player);

            this.networkingServer.listener?.NetworkingServerClientDidDisconnect(player.client);
        }
    }
}