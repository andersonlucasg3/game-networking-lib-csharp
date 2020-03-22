using GameNetworking.Commons.Models.Server;
using GameNetworking.Commons.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking {
    public class UnreliableClientAcceptor<TPlayer> : GameServerClientAcceptor<UnreliableGameServer<TPlayer>, UnreliableNetworkingServer, TPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {
        public UnreliableClientAcceptor() : base() { }
    }
}