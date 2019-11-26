using UnityEngine;
using Messages.Models;
using System.Collections.Generic;

namespace GameNetworking {
    using Models.Client;

    public interface IGameClientListener {
        void GameClientDidConnect();
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();

        void GameClientDidReceiveMessage(MessageContainer container);

        GameObject GameClientSpawnCharacter(NetworkPlayer player);

        void GameClientNetworkPlayerDidDisconnect(NetworkPlayer player);
    }
}