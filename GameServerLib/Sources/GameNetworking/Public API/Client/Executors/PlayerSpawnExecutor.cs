namespace GameNetworking.Executors.Client {
    using Messages.Server;

    internal struct PlayerSpawnExecutor: IExecutor {
        private readonly GameClient gameClient;
        private readonly PlayerSpawnMessage spawnMessage;

        internal PlayerSpawnExecutor(GameClient client, PlayerSpawnMessage message) {
            this.gameClient = client;
            this.spawnMessage = message;
        }

        public void Execute() {
            Logging.Logger.Log(this.GetType(), "Executing...");

            var player = this.gameClient.FindPlayer(this.spawnMessage.playerId);
            var spawned = this.gameClient.Delegate?.GameClientSpawnCharacter(this.spawnMessage.spawnId, player);
            player.GameObject = spawned;
            spawned.transform.position = this.spawnMessage.position.ToVector3();
            spawned.transform.eulerAngles = this.spawnMessage.rotation.ToVector3();
        }
    }
}