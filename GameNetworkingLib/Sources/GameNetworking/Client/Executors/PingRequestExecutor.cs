using GameNetworking.Channels;
using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    internal class PingRequestExecutor<TPlayer> : Commons.BaseExecutor<IGameClient<TPlayer>, PingRequestMessage>
        where TPlayer : GameNetworking.Client.Player {
        public PingRequestExecutor(IGameClient<TPlayer> client) : base(client, null) { }

        public override void Execute() {
            this.instance.localPlayer.lastReceivedPingRequest = TimeUtils.CurrentTime();

            this.instance.Send(new PongRequestMessage(), Channel.unreliable);
        }
    }
}