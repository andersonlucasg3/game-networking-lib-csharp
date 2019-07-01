using UnityEngine;

namespace GameNetworking {
    using Models;
    using Models.Server;

    public abstract class MovementController<GameType>: BaseWorker<GameType>, IMovementController where GameType: class, IGameInstance {
        protected NetworkPlayersStorage players;

        internal MovementController(GameType instance, NetworkPlayersStorage storage) : base(instance) { 
            this.players = storage;
        }

        public virtual void Update() {
            this.players.ForEach(player => { this.Instance.InstanceDelegate?.GameInstanceMovePlayer(player, this.Instance.MovementController); });
        }

        public void Move(NetworkPlayer player, Vector3 direction, float velocity) {
            var charController = player.GameObject.GetComponent<CharacterController>();
            var movement = direction.normalized;
            if (charController != null) {
                charController.Move(movement * velocity);
                this.Rotate(charController.transform, movement);
            } else {
                player.GameObject.transform.position = movement * velocity;
                this.Rotate(player.GameObject.transform, movement);
            }
        }

        private void Rotate(Transform transform, Vector3 direction) {
            direction.y = 0;
            transform.forward = direction;
        }
    }
}