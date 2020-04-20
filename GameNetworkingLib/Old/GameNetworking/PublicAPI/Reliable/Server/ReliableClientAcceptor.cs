using GameNetworking.Commons.Models.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Commons.Server {
    public class ReliableClientAcceptor<TPlayer> : GameServerClientAcceptor<ReliableGameServer<TPlayer>, ReliableNetworkingServer, TPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>
        where TPlayer : class, INetworkPlayer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>, new() {

        public ReliableClientAcceptor() : base() { }
    }
}