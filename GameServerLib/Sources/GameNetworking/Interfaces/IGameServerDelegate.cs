using UnityEngine;

namespace GameNetworking {
    using Models;

    public interface IGameServerDelegate {
        GameObject GameServerSpawnCharacter(int spawnId);
    }
}