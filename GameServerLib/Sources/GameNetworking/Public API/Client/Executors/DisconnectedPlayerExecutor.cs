namespace GameNetworking.Executors.Client {
    using Logging;
    using Messages.Server;
    using Models.Client;

    internal struct DisconnectedPlayerExecutor : IExecutor {
        private readonly GameClient gameClient;
        private readonly DisconnectedPlayerMessage message;

        internal DisconnectedPlayerExecutor(GameClient client, DisconnectedPlayerMessage message) {
            this.gameClient = client;
            this.message = message;
        }

        public void Execute() {
            Logger.Log(this.GetType(), string.Format("Executing for playerId {0}", this.message.playerId));

            var player = this.gameClient.RemovePlayer(this.message.playerId);
            if (player != null) { this.gameClient.Delegate?.GameClientNetworkPlayerDidDisconnect(player); }
        }
    }
}