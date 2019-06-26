namespace GameNetworking.Executors.Client {
    using Messages.Server;

    internal struct PlayerSpawnExecutor: IExecutor {
        private readonly GameClient gameClient;
        private readonly PlayerSpawnMessage spawnMessage;

        internal PlayerSpawnExecutor(GameClient client, PlayerSpawnMessage message) {
            this.gameClient = client;
            this.spawnMessage = message;
        }

        void IExecutor.Execute() {

        }
    }
}