namespace GameNetworking {
    public interface IGameClientDelegate {
        void GameClientDidConnect();
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();
    }
}