namespace Networking {
    using Models;

    public interface INetworkingListener {
        void NetworkingDidConnect(INetClient client);

        void NetworkingConnectDidTimeout();

        void NetworkingDidDisconnect(INetClient client);
    }
}
