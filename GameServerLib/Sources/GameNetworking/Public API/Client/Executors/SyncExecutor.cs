using UnityEngine;

namespace GameNetworking.Executors.Client {
    using Messages.Server;
    using Models.Client;

    internal struct SyncExecutor : IExecutor {
        private readonly GameClient gameClient;
        private readonly SyncMessage message;

        public SyncExecutor(GameClient gameClient, SyncMessage message) {
            this.gameClient = gameClient;
            this.message = message;
        }

        public void Execute() {
            NetworkPlayer player;
            if (this.message.playerId == this.gameClient.player.playerId) {
                player = this.gameClient.player;
            } else {
                player = this.gameClient.FindPlayer(this.message.playerId);
            }

            if (player?.gameObject == null) { return; }

            Synchronize(player);
        }

        private void Synchronize(NetworkPlayer player) {
            if (!player.gameObject.TryGetComponent(out CharacterController charController)) {
                Position(player);
                return;
            }

            charController.enabled = false;

            Position(player);

            charController.enabled = true;
        }

        private void Position(NetworkPlayer player) {
            Vector3 pos = Vector3.zero;
            Vector3 euler = Vector3.zero;
            this.message.position.CopyToVector3(ref pos);
            this.message.rotation.CopyToVector3(ref euler);

            if (gameClient.instanceListener?.GameInstanceSyncPlayer(player, pos, euler) ?? false) { return; }

            player.transform.position = pos;
            player.transform.eulerAngles = euler;
        }
    }
}