using System;
using UnityEngine;
using System.Collections.Generic;

namespace GameNetworking {
    using Models.Server;
    using Messages.Server;
    using Messages;

    internal class MovementController: BaseServerWorker {
        private const float syncTimeMs = 0.2F;
        private float lastSyncTime;

        public List<NetworkPlayer> Players {
            get; set;
        }

        internal MovementController(GameServer gameServer) :base(gameServer) { }

        public void Move(NetworkPlayer player, Vector3 direction) {
            var charController = player.GameObject.GetComponent<CharacterController>();
            if (charController != null) {
                charController.Move(direction);
            }
        }

        public void Update() {
            if (Time.deltaTime - this.lastSyncTime > syncTimeMs) {
                this.lastSyncTime = Time.deltaTime;

                this.Players?.ForEach(player => this.SendSync(player));
            }
        }

        private void SendSync(NetworkPlayer player) {
            var syncMessage = new SyncMessage() {
                playerId = player.PlayerId,
                position = player.GameObject.transform.position.ToVec3(),
                rotation = player.GameObject.transform.eulerAngles.ToVec3()
            };
            this.Server.SendBroadcast(syncMessage);
        }
    }
}