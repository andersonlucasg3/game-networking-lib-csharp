namespace GameNetworking.Executors.Client {
    using Messages.Client;

    internal struct PingRequestExecutor: IExecutor {
        private GameClient client;

        public PingRequestExecutor(GameClient client) {
            this.client = client;
        }

        public void Execute() {
            this.client.Send(new PongRequestMessage());
        }
    }
}