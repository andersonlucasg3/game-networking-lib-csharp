namespace GameNetworking.Executors.Client {
    using Models.Client;
    using Messages;

    internal struct ClientMoveRequestExecutor: IExecutor {
        private readonly GameClient client;
        private readonly MoveRequestMessage message;

        public ClientMoveRequestExecutor(GameClient client, MoveRequestMessage message) {
            this.client = client;
            this.message = message;
        }

        public void Execute() {
            Logging.Logger.Log(this.GetType(), string.Format("Executing move request for client {0}", this.message.playerId));
            var player = this.client.FindPlayer(this.message.playerId);
            if (player != null) { this.message.direction.CopyToVector3(ref player.inputState.direction); }
            else { Logging.Logger.Log(this.GetType(), "Client not found, returning..."); }
        }
    }
}