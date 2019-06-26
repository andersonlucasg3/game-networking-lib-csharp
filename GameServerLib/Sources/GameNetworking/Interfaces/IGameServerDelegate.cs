using UnityEngine;
using Messages.Models;

namespace GameNetworking {
    using Models;

    public interface IGameServerDelegate {
        GameObject GameServerSpawnCharacter(int spawnId, NetworkPlayer player);
        void GameServerDidReceiveClientMessage(MessageContainer container, NetworkPlayer player);
    }
}