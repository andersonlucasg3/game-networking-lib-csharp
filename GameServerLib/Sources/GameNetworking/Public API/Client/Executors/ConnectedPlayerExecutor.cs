namespace GameNetworking.Executors.Client {
    using Logging;
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
            Logger.Log($"Executing for playerId {this.message.playerId} is me {this.message.isMe}");

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