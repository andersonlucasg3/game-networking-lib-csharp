using System;
using UnityEngine;
using System.Collections.Generic;

namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;
    using Messages;

    public interface IMovementController {
        void Move(NetworkPlayer player, Vector3 direction, float velocity);
    }

    internal class MovementController: BaseServerWorker, IMovementController {
        private float lastSyncTime;
        private NetworkPlayersStorage players;

        public float SyncIntervalMs {
            get; set;
        }

        internal MovementController(GameServer gameServer, NetworkPlayersStorage storage) :base(gameServer) { 
            this.players = storage;
            this.SyncIntervalMs = 0.2f;
        }

        public void Move(NetworkPlayer player, Vector3 direction, float velocity) {
            var charController = player.GameObject.GetComponent<CharacterController>();
            if (charController != null) {
                charController.Move(direction * velocity);
                this.Rotate(charController.transform, direction.normalized);
            } else {
                var movement = direction.normalized;
                player.GameObject.transform.position = movement * velocity * Time.deltaTime;
                this.Rotate(player.GameObject.transform, movement);
            }
        }

        private void Rotate(Transform transform, Vector3 direction) {
            direction.y = 0;
            transform.forward = direction;
        }

        internal void Update() {
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
            this.Server.SendBroadcast(syncMessage);
        }
    }
}