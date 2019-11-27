using GameNetworking.Executors;

namespace GameNetworking.Executors.Client {
    using Messages.Server;

    internal struct PingResultRequestExecutor : IExecutor {
        private GameClient client;
        private readonly PingResultRequestMessage message;

        public PingResultRequestExecutor(GameClient client, PingResultRequestMessage message) {
            this.client = client;
            this.message = message;
        }

        void IExecutor.Execute() {
            if (this.message.playerId == this.client.player.playerId) {
                this.client.player.mostRecentPingValue = this.message.pingValue;
            } else {
                var player = client.FindPlayer(this.message.playerId);
                player.mostRecentPingValue = this.message.pingValue;
            }
        }
    }
}
