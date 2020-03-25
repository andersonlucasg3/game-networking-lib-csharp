using GameNetworking.Commons;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Messages.Server;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Executors.Server {
    public class UnreliableDisconnectExecutor<TPlayer> : BaseExecutor<UnreliableGameServer<TPlayer>>
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {

        private readonly TPlayer player;

        public UnreliableDisconnectExecutor(UnreliableGameServer<TPlayer> instance, TPlayer player) : base(instance) {
            this.player = player;
        }

        public override void Execute() {
            this.instance.Send(new UnreliableDisconnectResponseMessage(), this.player);

            this.instance.DisconnectRequired(this.player);
        }
    }
}