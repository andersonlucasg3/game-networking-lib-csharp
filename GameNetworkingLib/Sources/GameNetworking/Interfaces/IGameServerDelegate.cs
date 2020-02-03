using Messages.Models;

namespace GameNetworking {
    using Models.Server;

    public interface IGameServerListener {
        void GameServerPlayerDidConnect(NetworkPlayer player);
        void GameServerPlayerDidDisconnect(NetworkPlayer player);
        void GameServerDidReceiveClientMessage(MessageContainer container, NetworkPlayer player);
    }
}