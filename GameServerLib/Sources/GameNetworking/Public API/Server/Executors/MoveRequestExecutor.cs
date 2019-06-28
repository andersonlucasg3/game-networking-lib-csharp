namespace GameNetworking.Executors.Server {
    using Models.Server;
    using Messages.Client;

    internal class MoveRequestExecutor: IExecutor {
        private readonly GameServer gameServer;
        private readonly NetworkPlayer player;
        private readonly MoveRequestMessage message;

        public MoveRequestExecutor(GameServer server, NetworkPlayer player, MoveRequestMessage message) {
            this.gameServer = server;
            this.player = player;
            this.message = message;
        }

        public void Execute() {
            this.gameServer.movementController.Move(this.player, this.message.direction.ToVector3());
        }
    }
}