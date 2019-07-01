using UnityEngine;

namespace GameNetworking {
    using Models.Server;

    public interface IMovementController {
        void Move(NetworkPlayer player, Vector3 direction, float velocity);
    }
}