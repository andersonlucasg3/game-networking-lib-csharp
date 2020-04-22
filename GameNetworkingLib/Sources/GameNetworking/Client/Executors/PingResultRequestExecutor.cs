using GameNetworking.Client;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    internal class PingResultRequestExecutor<TPlayer> : Commons.BaseExecutor<IGameClient<TPlayer>, PingResultRequestMessage>
        where TPlayer : GameNetworking.Client.Player {
        internal PingResultRequestExecutor(IGameClient<TPlayer> client, PingResultRequestMessage message) : base(client, message) { }

        public override void Execute() {
            var player = this.instance.playerCollection.FindPlayer(this.message.playerId);
            if (player == null) { return; }
            player.mostRecentPingValue = this.message.pingValue;
        }
    }
}
