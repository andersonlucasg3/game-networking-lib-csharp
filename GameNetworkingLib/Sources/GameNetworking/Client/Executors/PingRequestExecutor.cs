using GameNetworking.Channels;
using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    struct PingRequestExecutor<TPlayer> : IExecutor<GameClient<TPlayer>, PingRequestMessage>
        where TPlayer : GameNetworking.Client.Player, new() {
        public void Execute(GameClient<TPlayer> model, PingRequestMessage message) {
            if (model.localPlayer != null) {
                model.localPlayer.lastReceivedPingRequest = TimeUtils.CurrentTime();
            }
            model.Send(new PongRequestMessage(message.pingRequestId), Channel.unreliable);
        }
    }
}