namespace GameNetworking.Executors.Client {
    using Messages.Server;
    using Models.Client;

    internal struct ConnectedPlayerExecutor: IExecutor {
        private readonly GameClient gameClient;
        private readonly ConnectedPlayerMessage spawnMessage;

        internal ConnectedPlayerExecutor(GameClient client, ConnectedPlayerMessage message) {
            this.gameClient = client;
            this.spawnMessage = message;
        }

        public void Execute() {
            Logging.Logger.Log(this.GetType(), "Executing...");

            var player = new NetworkPlayer(this.spawnMessage.playerId) {
                IsLocalPlayer = this.spawnMessage.isMe
            };
            this.gameClient.AddPlayer(player);
        }
    }
}