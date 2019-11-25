using UnityEngine;

namespace GameNetworking.Executors.Client {
    using GameNetworking.Models.Client;
    using Messages.Server;

    internal struct PlayerSpawnExecutor: IExecutor {
        private readonly GameClient gameClient;
        private readonly PlayerSpawnMessage spawnMessage;

        internal PlayerSpawnExecutor(GameClient client, PlayerSpawnMessage message) {
            this.gameClient = client;
            this.spawnMessage = message;
        }

        public void Execute() {
            Logging.Logger.Log(this.GetType(), string.Format("Executing for playerId {0}", this.spawnMessage.playerId));

            NetworkPlayer player;
            if (this.spawnMessage.playerId == this.gameClient.player.playerId) {
                player = this.gameClient.player;
            } else {
                player = this.gameClient.FindPlayer(this.spawnMessage.playerId);
            }

            player.spawnId = this.spawnMessage.spawnId;

            var spawned = this.gameClient.listener?.GameClientSpawnCharacter(player);
            player.gameObject = spawned;

            SetupCharacterControllerIfNeeded(spawned);
        }

        private void SetupCharacterControllerIfNeeded(GameObject spawned) {
            if (!spawned.TryGetComponent(out CharacterController charController)) {
                Position(spawned.transform);
                return;
            }

            charController.enabled = false;

            Position(spawned.transform);

            charController.enabled = true;
        }

        private void Position(Transform transform) {
            transform.position = this.spawnMessage.position;
            transform.eulerAngles = this.spawnMessage.rotation;
        }
    }
}