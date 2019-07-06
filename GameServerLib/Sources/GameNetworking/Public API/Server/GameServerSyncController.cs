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
            if (player.Transform == null) { return; }

            float pingValueInSeconds = this.Instance.pingController.GetPingValue(player);

            var syncMessage = new SyncMessage {
                playerId = player.PlayerId
            };
            this.FuturePosition(pingValueInSeconds, player).CopyToVec3(ref syncMessage.position);
            player.Transform.eulerAngles.CopyToVec3(ref syncMessage.rotation);
            this.Instance.SendBroadcast(syncMessage);
        }

        private Vector3 FuturePosition(float pingValueInSeconds, NetworkPlayer player) {
            return player.Transform.position + player.inputState.direction * pingValueInSeconds * Time.deltaTime;
        }
    }
}