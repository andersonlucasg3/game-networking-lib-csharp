using UnityEngine;

namespace GameNetworking {
    public interface IGameInstance {

    }

    public interface IGameClientInstanceListener {
        bool GameInstanceSyncPlayer(Models.Client.NetworkPlayer player, Vector3 position, Vector3 eulerAngles);
    }

    public interface IGameClientInstance : IGameInstance {
        IGameClientInstanceListener instanceListener { get; set; }
    }
}