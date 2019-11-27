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

            var self = this;
            this.server.AllPlayers().ForEach((each) => {
                var result = new PingResultRequestMessage(each.playerId, each.mostRecentPingValue);
                self.server.Send(result, self.player.client);
            });
        }
    }
}