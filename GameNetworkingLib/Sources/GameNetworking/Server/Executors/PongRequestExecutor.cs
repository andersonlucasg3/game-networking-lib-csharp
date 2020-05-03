using GameNetworking.Channels;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Server;

namespace GameNetworking.Executors.Server {
    internal class PongRequestExecutor<TPlayer> : Commons.BaseExecutor<IGameServer<TPlayer>, PongRequestMessage>
        where TPlayer : class, GameNetworking.Server.IPlayer {
        private readonly TPlayer player;

        public PongRequestExecutor(IGameServer<TPlayer> server, TPlayer player) : base(server, null) {
            this.player = player;
        }

        public override void Execute() {
            this.instance.pingController.PongReceived(this.player, this.message.pingRequestId);

            var players = this.instance.playerCollection.values;
            for (int index = 0; index < players.Count; index++) {
                TPlayer player = players[index];
                PingResultRequestMessage message = new PingResultRequestMessage(player.playerId, player.mostRecentPingValue);
                this.player.Send(message, Channel.unreliable);
            }
        }
    }
}