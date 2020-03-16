namespace GameNetworking.Executors.Client {
    using GameNetworking.Models.Client;
    using Logging;
    using Messages.Server;

    internal struct ConnectedPlayerExecutor<PlayerType> : IExecutor where PlayerType : NetworkPlayer, new() {
        private readonly GameClient<PlayerType> gameClient;
        private readonly ConnectedPlayerMessage message;

        internal ConnectedPlayerExecutor(GameClient<PlayerType> client, ConnectedPlayerMessage message) {
            this.gameClient = client;
            this.message = message;
        }

        public void Execute() {
            Logger.Log($"Executing for playerId {this.message.playerId} is me {this.message.isMe}");

            var player = new PlayerType() {
                playerId = this.message.playerId,
                isLocalPlayer = this.message.isMe
            };
            this.gameClient.AddPlayer(player);
        }
    }
}