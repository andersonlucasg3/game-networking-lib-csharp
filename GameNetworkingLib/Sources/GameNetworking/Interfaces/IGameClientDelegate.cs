using Messages.Models;

namespace GameNetworking {
    using Models.Client;

    public interface IGameClientListener {
        void GameClientDidConnect();
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();

        void GameClientDidReceiveMessage(MessageContainer container);

        void GameClientNetworkPlayerDidDisconnect(NetworkPlayer player);
    }
}