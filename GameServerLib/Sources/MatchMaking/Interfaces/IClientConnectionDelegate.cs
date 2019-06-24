namespace MatchMaking.Connection {
    using Models;

    public interface IClientConnectionDelegate<MMClient> where MMClient: Client {
        void ClientConnectionDidConnect();
        void ClientConnectionDidDisconnect();
    }
}