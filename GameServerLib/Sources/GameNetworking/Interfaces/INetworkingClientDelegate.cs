namespace GameNetworking.Networking {
    public interface INetworkingClientDelegate {
        void NetworkingClientDidConnect();
        void NetworkingClientConnectDidTimeout();
        void NetworkingClientDidDisconnect();
    }
}