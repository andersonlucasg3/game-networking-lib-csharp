using Messages.Models;

namespace GameNetworking {
    using Models;

    internal interface INetworkingServerDelegate {
        void NetworkingServerDidAcceptClient(NetworkClient client);
        void NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client);
    }
}