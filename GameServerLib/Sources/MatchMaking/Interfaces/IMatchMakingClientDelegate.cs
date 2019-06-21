using System;
namespace MatchMaking {
    using Models;

    public interface IMatchMakingClientDelegate<MMClient> where MMClient: Client, new() {
        void MatchMakingClientDidConnect(MatchMakingClient<MMClient> matchMaking);

        void MatchMakingClientDidRequestConnectToGameServer(MatchMakingClient<MMClient> matchMaking, ConnectGameInstanceResponse message);
    }
}
