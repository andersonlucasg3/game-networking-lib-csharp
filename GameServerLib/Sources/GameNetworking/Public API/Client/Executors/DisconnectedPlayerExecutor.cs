namespace GameNetworking.Executors.Client {
    using Logging;
    using Messages.Server;

    internal struct DisconnectedPlayerExecutor : IExecutor {
        private readonly GameClient gameClient;
        private readonly DisconnectedPlayerMessage message;

        internal DisconnectedPlayerExecutor(GameClient client, DisconnectedPlayerMessage message) {
            this.gameClient = client;
            this.message = message;
        }

        public void Execute() {
            Logger.Log($"Executing for playerId {this.message.playerId}");

            var player = this.gameClient.RemovePlayer(this.message.playerId);
            if (player != null) { this.gameClient.listener?.GameClientNetworkPlayerDidDisconnect(player); }
        }
    }
}