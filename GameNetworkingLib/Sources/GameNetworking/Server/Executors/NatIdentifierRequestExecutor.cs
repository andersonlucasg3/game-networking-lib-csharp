using GameNetworking.Messages.Client;
using GameNetworking.Server;
using GameNetworking.Sockets;

namespace GameNetworking.Executors.Server {
    class NatIdentifierRequestExecutor<TPlayer> : Commons.BaseExecutor<IGameServer<TPlayer>, NatIdentifierRequestMessage>
        where TPlayer : class, GameNetworking.Server.IPlayer {
        private readonly TPlayer player;

        public NatIdentifierRequestExecutor(IGameServer<TPlayer> instance, TPlayer player, NatIdentifierRequestMessage message) : base(instance, message) {
            this.player = player;
        }

        public override void Execute() {
            this.player.NatIdentify(new NetEndPoint(this.message.remote, this.message.port));
        }
    }
}