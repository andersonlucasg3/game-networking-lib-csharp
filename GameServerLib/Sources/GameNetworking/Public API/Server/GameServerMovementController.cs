using System;
using UnityEngine;
using System.Collections.Generic;

namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;
    using Messages;

    public class GameServerMovementController: MovementController<GameServer> {
        private float lastSyncTime;

        public float SyncIntervalMs {
            get; set;
        }

        internal GameServerMovementController(GameServer instance, NetworkPlayersStorage storage) : base(instance, storage) {
            this.SyncIntervalMs = 0.2f;
        }

        public override void Update() {
            base.Update();
            
            if (Time.time - this.lastSyncTime > this.SyncIntervalMs) {
                this.lastSyncTime = Time.time;

                this.players?.ForEach(player => this.SendSync(player));
            }
        }

        private void SendSync(NetworkPlayer player) {
            if (player.GameObject?.transform == null) { return; }

            var syncMessage = new SyncMessage() {
                playerId = player.PlayerId
            };
            player.GameObject.transform.position.CopyToVec3(ref syncMessage.position);
            player.GameObject.transform.eulerAngles.CopyToVec3(ref syncMessage.rotation);
            this.Instance.SendBroadcast(syncMessage);
        }
    }
}