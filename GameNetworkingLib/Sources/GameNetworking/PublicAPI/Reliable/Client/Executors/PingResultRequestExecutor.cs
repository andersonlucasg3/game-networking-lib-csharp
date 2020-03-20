namespace GameNetworking.Executors.Client {
    using GameNetworking.Models.Client;
    using Messages.Server;

    internal struct PingResultRequestExecutor<PlayerType> : IExecutor where PlayerType : NetworkPlayer, new() {
        private GameClient<PlayerType> client;
        private readonly PingResultRequestMessage message;

        public PingResultRequestExecutor(GameClient<PlayerType> client, PingResultRequestMessage message) {
            this.client = client;
            this.message = message;
        }

        void IExecutor.Execute() {
            var player = this.client.FindPlayer(this.message.playerId);
            player.mostRecentPingValue = this.message.pingValue;
        }
    }
}
