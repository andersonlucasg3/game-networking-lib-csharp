using Messages.Models;

namespace GameNetworking.Networking {
    public interface INetworkingClientListener {
        void NetworkingClientDidConnect();
        void NetworkingClientConnectDidTimeout();
        void NetworkingClientDidDisconnect();
        void NetworkingClientDidReadMessage(MessageContainer container);
    }
}