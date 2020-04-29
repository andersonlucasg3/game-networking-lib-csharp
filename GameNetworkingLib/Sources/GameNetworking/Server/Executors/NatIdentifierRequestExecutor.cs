using GameNetworking.Channels;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Server;

namespace GameNetworking.Executors.Server {
    class NatIdentifierRequestExecutor<TPlayer> : Commons.BaseExecutor<IGameServer<TPlayer>, NatIdentifierRequestMessage>
        where TPlayer : class, GameNetworking.Server.IPlayer {
        private readonly TPlayer player;

        public NatIdentifierRequestExecutor(IGameServer<TPlayer> instance, TPlayer player, NatIdentifierRequestMessage message) : base(instance, message) {
            this.player = player;
        }

        public override void Execute() {
            this.player.NatIdentify(this.message.port);
            this.instance.listener.GameServerPlayerDidConnect(this.player, Channel.unreliable);

            this.player.Send(new NatIdentifierResponseMessage(), Channel.unreliable);
        }
    }
}