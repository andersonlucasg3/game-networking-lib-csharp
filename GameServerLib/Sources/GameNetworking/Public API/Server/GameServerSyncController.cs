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

        public float SyncIntervalMs {
            get; set;
        }

        internal GameServerSyncController(GameServer instance, NetworkPlayersStorage storage) : base(instance) {
            this.storage = storage;
            this.SyncIntervalMs = 0.2f;
        }

        public void Update() {
            this.storage.ForEach(player => {
                if (player.inputState.HasMovement) {
                    this.Instance.InstanceDelegate?.GameInstanceMovePlayer(player, player.inputState.direction);
                }
            });

            if (Time.time - this.lastSyncTime > this.SyncIntervalMs) {
                this.lastSyncTime = Time.time;

                this.storage?.ForEach(player => this.SendSync(player));
            }
        }

        private void SendSync(NetworkPlayer player) {
            if (player.GameObject?.transform == null) { return; }

            var syncMessage = new SyncMessage {
                playerId = player.PlayerId
            };
            player.GameObject.transform.position.CopyToVec3(ref syncMessage.position);
            player.GameObject.transform.eulerAngles.CopyToVec3(ref syncMessage.rotation);
            this.Instance.SendBroadcast(syncMessage);
        }
    }
}