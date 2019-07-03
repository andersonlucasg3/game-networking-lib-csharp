namespace GameNetworking.Executors.Server {
    using Models.Server;

    internal struct PongRequestExecutor : IExecutor {
        private GameServer server;
        private NetworkPlayer player;

        public PongRequestExecutor(GameServer server, NetworkPlayer player) {
            this.server = server;
            this.player = player;
        }

        public void Execute() {
            this.server.pingController.PongReceived(this.player);
        }
    }
}