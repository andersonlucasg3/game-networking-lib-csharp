using UnityEngine;

namespace GameNetworking.Executors.Server {
    using Models.Server;
    using Messages;

    internal struct ServerMoveRequestExecutor: IExecutor {
        private readonly GameServer server;
        private readonly NetworkPlayer player;
        private readonly MoveRequestMessage message;

        public ServerMoveRequestExecutor(GameServer server, NetworkPlayer player, MoveRequestMessage message) {
            this.server = server;
            this.player = player;
            this.message = message;
        }

        public void Execute() {
            this.message.direction.CopyToVector3(ref this.player.inputState.direction);
            var position = Vector3.zero;
            this.message.position.CopyToVector3(ref position);
            
            this.message.playerId = this.player.PlayerId;

            var self = this;
            this.server.AllPlayers().ForEach(player => {
                if (player.PlayerId == self.player.PlayerId) { return; }
                self.server.Send(self.message, player.Client);
            });
        }
    }
}