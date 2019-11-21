using UnityEngine;

namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;
    using Messages;
    using Commons;

    public class GameServerSyncController: BaseWorker<GameServer> {
        private NetworkPlayersStorage storage;
        private float lastSyncTime;

        public float SyncInterval {
            get; set;
        }

        internal GameServerSyncController(GameServer instance, NetworkPlayersStorage storage) : base(instance) {
            this.storage = storage;
            this.SyncInterval = .2f;
            this.lastSyncTime = 0F;
        }

        public void Update() {
            if (Time.time - this.lastSyncTime > this.SyncInterval) {
                this.lastSyncTime = Time.time;

                this.storage?.ForEach(player => this.SendSync(player));
            }
        }

        private void SendSync(NetworkPlayer player) {
            if (player.Transform == null) { return; }

            var syncMessage = new SyncMessage {
                playerId = player.PlayerId
            };
            player.Transform.position.CopyToVec3(ref syncMessage.position);
            player.Transform.eulerAngles.CopyToVec3(ref syncMessage.rotation);
            this.Instance.SendBroadcast(syncMessage);
        }
    }
}