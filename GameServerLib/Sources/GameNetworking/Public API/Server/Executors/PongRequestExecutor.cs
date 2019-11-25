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
            var pingValue = this.server.pingController.PongReceived(this.player);

            this.server.Send(new PingResultRequestMessage(pingValue), this.player.client);
        }
    }
}