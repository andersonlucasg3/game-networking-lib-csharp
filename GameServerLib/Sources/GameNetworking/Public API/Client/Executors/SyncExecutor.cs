using UnityEngine;

namespace GameNetworking.Executors.Client {
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
            
            var charController = player.GameObject.GetComponent<CharacterController>();
            charController.enabled = false;
            
            Vector3 pos = Vector3.zero;
            Vector3 euler = Vector3.zero;
            this.message.position.CopyToVector3(ref pos);
            this.message.rotation.CopyToVector3(ref euler);
            charController.transform.position = pos;
            charController.transform.eulerAngles = euler;
            
            charController.enabled = true;
        }
    }
}