namespace GameNetworking.Executors.Client {
    using Messages;
    using Messages.Server;
    using Models;

    internal struct ConnectedPlayerExecutor: IExecutor {
        private readonly GameClient gameClient;
        private readonly ConnectedPlayerMessage spawnMessage;

        internal ConnectedPlayerExecutor(GameClient client, ConnectedPlayerMessage message) {
            this.gameClient = client;
            this.spawnMessage = message;
        }

        public void Execute() {
            Logging.Logger.Log(this.GetType(), "Executing...");

            var player = new NetworkPlayer(this.spawnMessage.playerId);
            this.gameClient.AddPlayer(player);
        }
    }
}