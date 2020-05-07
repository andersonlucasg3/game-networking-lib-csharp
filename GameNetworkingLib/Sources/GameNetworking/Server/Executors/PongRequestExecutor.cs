using GameNetworking.Channels;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Server;

namespace GameNetworking.Executors.Server {
    internal class PongRequestExecutor<TPlayer> : Commons.BaseExecutor<IGameServer<TPlayer>, PongRequestMessage>
        where TPlayer : GameNetworking.Server.Player {
        private readonly TPlayer player;

        public PongRequestExecutor(IGameServer<TPlayer> server, TPlayer player, PongRequestMessage message) : base(server, message) {
            this.player = player;
        }

        public override void Execute() {
            this.instance.pingController.PongReceived(this.player, this.message.pingRequestId);

            var players = this.instance.playerCollection.values;
            for (int index = 0; index < players.Count; index++) {
                TPlayer player = players[index];
                this.player.Send(new PingResultRequestMessage(player.playerId, player.mostRecentPingValue), Channel.unreliable);
            }
        }
    }
}