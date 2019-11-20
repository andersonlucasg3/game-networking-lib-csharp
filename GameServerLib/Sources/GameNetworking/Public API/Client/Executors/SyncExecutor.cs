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

            if (player?.GameObject == null) { return; }

            Synchronize(player.GameObject);
        }

        private void Synchronize(GameObject player) {
            var charController = player.GetComponent<CharacterController>();

            if (charController == null) {
                Position(player.transform);
                return;
            }

            charController.enabled = false;

            Position(player.transform);

            charController.enabled = true;
        }

        private void Position(Transform transform) {
            Vector3 pos = Vector3.zero;
            Vector3 euler = Vector3.zero;
            this.message.position.CopyToVector3(ref pos);
            this.message.rotation.CopyToVector3(ref euler);
            transform.position = pos;
            transform.eulerAngles = euler;
        }
    }
}