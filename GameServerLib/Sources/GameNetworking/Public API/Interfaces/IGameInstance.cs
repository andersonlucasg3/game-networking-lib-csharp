using UnityEngine;

namespace GameNetworking {
    public interface IGameInstanceDelegate {
        void GameInstanceMovePlayer(Models.Server.NetworkPlayer player, Vector3 direction, Vector3 position);
    }

    public interface IGameClientInstanceDelegate: IGameInstanceDelegate {
        bool GameInstanceSyncPlayer(Models.Client.NetworkPlayer player, Vector3 position, Vector3 eulerAngles);
    }

    public interface IGameInstance {

    }

    public interface IGameServerInstance: IGameInstance {
        IGameInstanceDelegate InstanceDelegate { get; set; }
    }

    public interface IGameClientInstance: IGameInstance {
        IGameClientInstanceDelegate InstanceDelegate { get; set; }
    }
}