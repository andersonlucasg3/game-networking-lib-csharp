using Messages.Models;

namespace GameNetworking {
    using Models;

    internal interface INetworkingServerListener {
        void NetworkingServerDidAcceptClient(NetworkClient client);
        void NetworkingServerClientDidDisconnect(NetworkClient client);
    }

    internal interface INetworkingServerMessagesListener {
        void NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client);
    }
}