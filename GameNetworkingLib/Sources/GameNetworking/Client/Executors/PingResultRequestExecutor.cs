using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    struct PingResultRequestExecutor<TPlayer> : IExecutor<GameClient<TPlayer>, PingResultRequestMessage>
        where TPlayer : Player, new() {
        public void Execute(GameClient<TPlayer> model, PingResultRequestMessage message) {
            var player = model.playerCollection.FindPlayer(message.playerId);
            if (player == null) { return; }
            player.mostRecentPingValue = message.pingValue;
        }
    }
}
