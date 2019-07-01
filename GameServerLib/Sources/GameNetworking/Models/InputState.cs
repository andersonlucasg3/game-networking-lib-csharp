using UnityEngine;

namespace GameNetworking.Models {
    internal class InputState {
        public Vector3 direction;

        public bool HasMovement {
            get { return this.direction != Vector3.zero; }
        }

        public InputState() {
            this.direction = Vector3.zero;
        }
    }
}