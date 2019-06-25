using System;
namespace MatchMaking {
    using Models;

    public interface IMatchMakingClientDelegate<MMClient> where MMClient: Client, new() {
        void MatchMakingClientDidConnect(MatchMakingClient<MMClient> matchMaking);
        void MatchMakingClientConnectDidTimeout(MatchMakingClient<MMClient> matchMaking);
        void MatchMakingClientDidDisconnect(MatchMakingClient<MMClient> matchMaking);

        void MatchMakingClientDidRequestConnectToGameServer(MatchMakingClient<MMClient> matchMaking, ConnectGameInstanceResponse message);
        void MatchMakingClientDidReceiveUnknownMessage(MatchMakingClient<MMClient> matchMaking, MessageContainer message);
    }
}
