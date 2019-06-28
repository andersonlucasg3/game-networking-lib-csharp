using System;
using UnityEngine;
using System.Collections.Generic;

namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;
    using Messages;

    internal class MovementController: BaseServerWorker {
        private const float syncTimeMs = 0.2F;
        private float lastSyncTime;

        private NetworkPlayersStorage players;

        internal MovementController(GameServer gameServer, NetworkPlayersStorage storage) :base(gameServer) { 
            this.players = storage;
        }

        public void Move(NetworkPlayer player, Vector3 direction) {
            var charController = player.GameObject.GetComponent<CharacterController>();
            if (charController != null) {
                charController.Move(direction);
            }
        }

        public void Update() {
            if (Time.time - this.lastSyncTime > syncTimeMs) {
                this.lastSyncTime = Time.time;

                this.players?.ForEach(player => this.SendSync(player));
            }
        }

        private void SendSync(NetworkPlayer player) {
            var syncMessage = new SyncMessage() {
                playerId = player.PlayerId
            };
            player.GameObject.transform.position.CopyToVec3(ref syncMessage.position);
            player.GameObject.transform.eulerAngles.CopyToVec3(ref syncMessage.rotation);
            this.Server.SendBroadcast(syncMessage);
        }
    }
}