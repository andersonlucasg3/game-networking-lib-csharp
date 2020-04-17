using GameNetworking.Commons;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Executors.Client {
    public class UnreliableConnectResponseExecutor<TPlayer> : BaseExecutor<UnreliableGameClient<TPlayer>>
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {
        public UnreliableConnectResponseExecutor(UnreliableGameClient<TPlayer> instance) : base(instance) { }

        public override void Execute() {
            this.instance.DidConnect();
        }
    }
}