using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    internal class PingResultRequestExecutor<TPlayer> : BaseExecutor<IGameClient<TPlayer>, PingResultRequestMessage>
        where TPlayer : class, IPlayer {
        internal PingResultRequestExecutor(IGameClient<TPlayer> client, PingResultRequestMessage message) : base(client, message) { }

        public override void Execute() {
            if (!(this.instance.playerCollection.FindPlayer(this.message.playerId) is Player player)) { return; }
            player.mostRecentPingValue = this.message.pingValue;
        }
    }
}
