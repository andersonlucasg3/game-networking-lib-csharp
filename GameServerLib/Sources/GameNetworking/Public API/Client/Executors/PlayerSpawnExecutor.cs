using UnityEngine;

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
            
            var charController = spawned.GetComponent<CharacterController>();
            charController.enabled = false;
            
            Vector3 pos = Vector3.zero;
            Vector3 euler = Vector3.zero;
            this.spawnMessage.position.CopyToVector3(ref pos);
            this.spawnMessage.rotation.CopyToVector3(ref euler);
            spawned.transform.position = pos;
            spawned.transform.eulerAngles = euler;

            charController.enabled = true;
        }
    }
}