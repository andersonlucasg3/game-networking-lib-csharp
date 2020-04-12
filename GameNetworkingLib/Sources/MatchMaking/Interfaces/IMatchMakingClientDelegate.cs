using System;
namespace MatchMaking {
    using Models;

    public interface IMatchMakingClientDelegate<TClient> where TClient: MatchMakingClient, new() {
        void MatchMakingClientDidConnect(MatchMakingClient<TClient> matchMaking);
        void MatchMakingClientConnectDidTimeout(MatchMakingClient<TClient> matchMaking);
        void MatchMakingClientDidDisconnect(MatchMakingClient<TClient> matchMaking);

        void MatchMakingClientDidRequestConnectToGameServer(MatchMakingClient<TClient> matchMaking, ConnectGameInstanceResponse message);
        void MatchMakingClientDidReceiveUnknownMessage(MatchMakingClient<TClient> matchMaking, MessageContainer message);
    }
}
