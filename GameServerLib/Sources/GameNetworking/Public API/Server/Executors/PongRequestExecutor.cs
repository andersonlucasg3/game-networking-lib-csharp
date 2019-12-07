namespace GameNetworking.Executors.Server {
    using Models.Server;
    using Messages.Server;

    internal struct PongRequestExecutor : IExecutor {
        private GameServer server;
        private NetworkPlayer player;

        public PongRequestExecutor(GameServer server, NetworkPlayer player) {
            this.server = server;
            this.player = player;
        }

        public void Execute() {
            this.server.pingController.PongReceived(this.player);

            var players = this.server.AllPlayers();
            PingResultRequestMessage message;
            NetworkPlayer player;
            for (int i = 0; i < players.Count; i++) {
                player = players[i];
                message = new PingResultRequestMessage(player.playerId, player.mostRecentPingValue);
                this.server.Send(message, this.player.client);
            }
        }
    }
}