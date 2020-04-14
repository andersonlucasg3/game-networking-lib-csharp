#if ENABLE

namespace MatchMaking.Connection {
    using Models;

    public interface IClientConnectionDelegate<TClient> where TClient: MatchMakingClient {
        void ClientConnectionDidConnect();

        void ClientConnectionDidTimeout();

        void ClientConnectionDidDisconnect();

        void ClientDidReadMessage(MessageContainer container);
    }
}

#endif