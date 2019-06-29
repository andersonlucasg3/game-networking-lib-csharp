using UnityEngine;
using Messages.Models;

namespace GameNetworking {
    using Models.Client;

    public interface IGameClientDelegate {
        void GameClientDidConnect();
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();

        void GameClientDidReceiveMessage(MessageContainer container);

        GameObject GameClientSpawnCharacter(NetworkPlayer player);
    }
}