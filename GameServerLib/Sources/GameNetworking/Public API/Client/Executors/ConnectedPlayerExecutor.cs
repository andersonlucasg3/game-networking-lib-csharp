namespace GameNetworking.Executors.Client {
    using Messages.Server;
    using Models.Client;

    internal struct ConnectedPlayerExecutor: IExecutor {
        private readonly GameClient gameClient;
        private readonly ConnectedPlayerMessage message;

        internal ConnectedPlayerExecutor(GameClient client, ConnectedPlayerMessage message) {
            this.gameClient = client;
            this.message = message;
        }

        public void Execute() {
            Logging.Logger.Log(this.GetType(), "Executing...");

            var player = new NetworkPlayer(this.message.playerId) {
                IsLocalPlayer = this.message.isMe
            };
            this.gameClient.AddPlayer(player);
        }
    }
}