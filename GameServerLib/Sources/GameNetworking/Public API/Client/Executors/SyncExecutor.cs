namespace GameNetworking.Executors.Client {
    using Models.Client;
    using Messages.Server;

    internal struct SyncExecutor: IExecutor {
        private readonly GameClient gameClient;
        private readonly SyncMessage message;

        public SyncExecutor(GameClient gameClient, SyncMessage message) {
            this.gameClient = gameClient;
            this.message = message;
        }

        public void Execute() {
            var player = this.gameClient.FindPlayer(this.message.playerId);
            var transform = player.GameObject.transform;
            transform.position = this.message.position.ToVector3();
            transform.eulerAngles = this.message.rotation.ToVector3();
        }
    }
}