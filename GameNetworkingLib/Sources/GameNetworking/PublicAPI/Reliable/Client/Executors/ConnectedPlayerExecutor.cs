namespace GameNetworking.Executors.Client {
    using GameNetworking.Models.Contract.Client;
    using Messages.Server;

    internal struct ConnectedPlayerExecutor<PlayerType> : IExecutor where PlayerType : INetworkPlayer, new() {
        private readonly GameClient<PlayerType> gameClient;
        private readonly ConnectedPlayerMessage message;

        internal ConnectedPlayerExecutor(GameClient<PlayerType> client, ConnectedPlayerMessage message) {
            this.gameClient = client;
            this.message = message;
        }

        public void Execute() {
            var player = new PlayerType() {
                playerId = this.message.playerId,
                isLocalPlayer = this.message.isMe
            };
            this.gameClient.AddPlayer(player);

            if (player.isLocalPlayer) {
                this.gameClient.listener?.GameClientDidIdentifyLocalPlayer(player);
            }
        }
    }
}