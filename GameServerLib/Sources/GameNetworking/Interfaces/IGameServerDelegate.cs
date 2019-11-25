using UnityEngine;
using Messages.Models;

namespace GameNetworking {
    using Models.Server;

    public interface IGameServerListener {
        void GameServerPlayerDidDisconnect(NetworkPlayer player);
        GameObject GameServerSpawnCharacter(NetworkPlayer player);
        void GameServerDidReceiveClientMessage(MessageContainer container, NetworkPlayer player);
    }
}