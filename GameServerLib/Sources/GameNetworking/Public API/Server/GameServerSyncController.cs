using UnityEngine;

namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;
    using Messages;
    using Commons;

    public class GameServerSyncController : BaseWorker<GameServer> {
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

                var players = this.storage?.players;
                if (players == null) { return; }

                for (int i = 0; i < players.Count; i++) {
                    this.SendSync(players[i]);
                }
            }
        }

        private void SendSync(NetworkPlayer player) {
            if (player.transform == null) { return; }

            var syncMessage = new SyncMessage {
                playerId = player.playerId
            };
            syncMessage.position = player.transform.position;
            syncMessage.rotation = player.transform.eulerAngles;
            this.instance.SendBroadcast(syncMessage);
        }
    }
}