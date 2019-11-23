using UnityEngine;

namespace GameNetworking {
    public interface IGameClientInstanceDelegate {
        bool GameInstanceSyncPlayer(Models.Client.NetworkPlayer player, Vector3 position, Vector3 eulerAngles);
    }

    public interface IGameInstance {

    }

    public interface IGameClientInstance: IGameInstance {
        IGameClientInstanceDelegate InstanceDelegate { get; set; }
    }
}