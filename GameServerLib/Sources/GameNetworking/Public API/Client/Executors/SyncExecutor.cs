using UnityEngine;

namespace GameNetworking.Executors.Client {
    using Messages.Server;
    using Models.Client;

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

            Synchronize(player);
        }

        private void Synchronize(NetworkPlayer player) {
            CharacterController charController;
            if (!player.GameObject.TryGetComponent(out charController)) {
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

            if (gameClient.InstanceDelegate?.GameInstanceSyncPlayer(player, pos, euler) ?? false) { return; }

            player.Transform.position = pos;
            player.Transform.eulerAngles = euler;
        }
    }
}