namespace GameNetworking.Executors.Server {
    using Models.Server;
    using Messages.Server;

    internal struct PongRequestExecutor<PlayerType> : IExecutor where PlayerType : NetworkPlayer, new() {
        private GameServer<PlayerType> server;
        private PlayerType player;

        public PongRequestExecutor(GameServer<PlayerType> server, PlayerType player) {
            this.server = server;
            this.player = player;
        }

        public void Execute() {
            this.server.pingController.PongReceived(this.player);

            var players = this.server.AllPlayers();
            PingResultRequestMessage message;
            PlayerType player;
            for (int i = 0; i < players.Count; i++) {
                player = players[i];
                message = new PingResultRequestMessage(player.playerId, player.mostRecentPingValue);
                this.server.Send(message, this.player);
            }
        }
    }
}