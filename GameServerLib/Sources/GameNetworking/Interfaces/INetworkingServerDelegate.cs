using Messages.Models;

namespace GameNetworking {
    using Models;

    internal interface INetworkingServerDelegate {
        void NetworkingServerDidAcceptClient(NetworkClient client);
        void NetworkingServerClientDidDisconnect(NetworkClient client);
    }

    internal interface INetworkingServerMessagesDelegate {
        void NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client);
    }
}