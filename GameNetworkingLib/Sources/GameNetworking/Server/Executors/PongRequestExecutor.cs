using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Server;

namespace GameNetworking.Executors.Server {
    struct PongRequestExecutor<TPlayer> : IExecutor<GameServerMessageRouter<TPlayer>.ServerModel<TPlayer>, PongRequestMessage>
        where TPlayer : Player, new() {
        public void Execute(GameServerMessageRouter<TPlayer>.ServerModel<TPlayer> model, PongRequestMessage message) {
            model.server.pingController.PongReceived(model.model, message.pingRequestId);

            var players = model.server.playerCollection.values;
            for (int index = 0; index < players.Count; index++) {
                TPlayer player = players[index];
                model.model.Send(new PingResultRequestMessage(player.playerId, player.mostRecentPingValue), Channel.unreliable);
            }
        }
    }
}