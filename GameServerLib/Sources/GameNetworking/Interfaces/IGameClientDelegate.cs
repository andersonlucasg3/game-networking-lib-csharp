using UnityEngine;
using Messages.Models;

namespace GameNetworking {
    using Models;

    public interface IGameClientDelegate {
        void GameClientDidConnect();
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();

        void GameClientDidReceiveMessage(MessageContainer container);

        GameObject GameClientSpawnCharacter(int spawnId, NetworkPlayer player);
    }
}