namespace Networking {
    using Models;

    public interface INetworkingDelegate {
        void NetworkingDidConnect(Client client);
        void NetworkingDidDisconnect(Client client);
    }
}
