namespace Networking {
    using Models;

    public interface INetworkingDelegate {
        void NetworkingDidConnect(NetClient client);

        void NetworkingConnectDidTimeout();

        void NetworkingDidDisconnect(NetClient client);
    }
}
