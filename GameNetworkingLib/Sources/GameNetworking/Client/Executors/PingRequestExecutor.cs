using GameNetworking.Channels;
using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    internal class PingRequestExecutor<TPlayer> : BaseExecutor<IGameClient<TPlayer>, PingRequestMessage>
        where TPlayer : class, IPlayer {
        public PingRequestExecutor(IGameClient<TPlayer> client) : base(client, null) { }

        public override void Execute() {
            if (this.instance.localPlayer is Player player) {
                player.lastReceivedPingRequest = TimeUtils.CurrentTime();
            }

            this.instance.Send(new PongRequestMessage(), Channel.unreliable);
        }
    }
}