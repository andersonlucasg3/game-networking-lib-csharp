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
            Logging.Logger.Log(this.GetType(), string.Format("Executing for playerId {0} is me {1}", this.message.playerId, this.message.isMe));

            var player = new NetworkPlayer(this.message.playerId) {
                IsLocalPlayer = this.message.isMe
            };
            if (this.message.isMe) {
                this.gameClient.player = player;
            } else {
                this.gameClient.AddPlayer(player);
            }
        }
    }
}