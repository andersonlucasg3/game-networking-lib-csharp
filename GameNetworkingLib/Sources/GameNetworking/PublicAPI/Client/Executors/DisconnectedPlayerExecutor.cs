namespace GameNetworking.Executors.Client {
    using Logging;
    using Messages.Server;
    using Models.Client;

    internal struct DisconnectedPlayerExecutor<PlayerType> : IExecutor where PlayerType : NetworkPlayer, new() {
        private readonly GameClient<PlayerType> gameClient;
        private readonly DisconnectedPlayerMessage message;

        internal DisconnectedPlayerExecutor(GameClient<PlayerType> client, DisconnectedPlayerMessage message) {
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