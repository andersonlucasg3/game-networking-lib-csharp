using Messages.Models;

namespace GameNetworking.Networking {
    public interface INetworkingClientDelegate {
        void NetworkingClientDidConnect();
        void NetworkingClientConnectDidTimeout();
        void NetworkingClientDidDisconnect();
        void NetworkingClientDidReadMessage(MessageContainer container);
    }
}