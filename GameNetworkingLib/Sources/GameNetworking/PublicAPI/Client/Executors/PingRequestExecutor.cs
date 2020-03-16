namespace GameNetworking.Executors.Client {
    using GameNetworking.Models.Client;
    using Messages.Client;

    internal struct PingRequestExecutor<PlayerType> : IExecutor where PlayerType : NetworkPlayer, new() {
        private GameClient<PlayerType> client;

        public PingRequestExecutor(GameClient<PlayerType> client) {
            this.client = client;
        }

        public void Execute() {
            this.client.Send(new PongRequestMessage());
        }
    }
}