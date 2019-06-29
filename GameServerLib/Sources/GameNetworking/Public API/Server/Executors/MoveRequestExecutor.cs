using UnityEngine;

namespace GameNetworking.Executors.Server {
    using Models.Server;
    using Messages.Client;

    internal struct MoveRequestExecutor: IExecutor {
        private readonly GameServer gameServer;
        private readonly NetworkPlayer player;
        private readonly MoveRequestMessage message;

        public MoveRequestExecutor(GameServer server, NetworkPlayer player, MoveRequestMessage message) {
            this.gameServer = server;
            this.player = player;
            this.message = message;
        }

        public void Execute() {
            Vector3 vec3 = Vector3.zero;
            this.message.direction.CopyToVector3(ref vec3);
            this.gameServer.Delegate?.GameServerDidReceiveMoveRequest(vec3, this.player, this.gameServer.movementController);
        }
    }
}