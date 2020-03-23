using GameNetworking.Commons;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Executors.Server {
    public class UnreliableConnectExecutor<TPlayer> : BaseExecutor<UnreliableGameServer<TPlayer>>
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {

        public UnreliableConnectExecutor(UnreliableGameServer<TPlayer> instance) : base(instance) { }

        public override void Execute() { }
    }
}