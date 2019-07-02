using System;
using UnityEngine;
using System.Collections.Generic;

namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;
    using Messages;

    public class GameServerSyncController {
        private WeakReference weakGameServer;
        private NetworkPlayersStorage storage;
        private float lastSyncTime;

        private GameServer Instance {
            get { return this.weakGameServer?.Target as GameServer; }
        }

        public float SyncIntervalMs {
            get; set;
        }

        internal GameServerSyncController(GameServer server, NetworkPlayersStorage storage) {
            this.weakGameServer = new WeakReference(server);
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

            var syncMessage = new SyncMessage() {
                playerId = player.PlayerId
            };
            player.GameObject.transform.position.CopyToVec3(ref syncMessage.position);
            player.GameObject.transform.eulerAngles.CopyToVec3(ref syncMessage.rotation);
            this.Instance.SendBroadcast(syncMessage);
        }
    }
}