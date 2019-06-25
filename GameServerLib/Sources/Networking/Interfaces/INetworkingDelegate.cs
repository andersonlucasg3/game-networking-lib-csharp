namespace Networking {
    using Models;

    public interface INetworkingDelegate {
        void NetworkingDidConnect(Client client);

        void NetworkingConnectDidTimeout();

        void NetworkingDidDisconnect(Client client);
    }
}
