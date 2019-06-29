using UnityEngine;
using Messages.Models;

namespace GameNetworking {
    using Models.Server;

    public interface IGameServerDelegate {
        void GameServerPlayerDidDisconnect(NetworkPlayer player);
        GameObject GameServerSpawnCharacter(NetworkPlayer player);
        void GameServerDidReceiveClientMessage(MessageContainer container, NetworkPlayer player);
        void GameServerDidReceiveMoveRequest(Vector3 direction, NetworkPlayer player, IMovementController movementController);
    }
}