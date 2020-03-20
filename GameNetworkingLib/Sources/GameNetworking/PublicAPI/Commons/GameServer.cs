using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Server;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons {
    public abstract class GameServer<TPlayer, TSocket, TClient, TNetClient> 
        where TPlayer : NetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private readonly NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient> playersStorage;
        private readonly GameServerMessageRouter<TPlayer, TSocket, TClient, TNetClient> router;

        public GameServerPingController<TPlayer, TSocket, TClient, TNetClient> pingController { get; }

        protected GameServer(IMainThreadDispatcher dispatcher) {
            this.playersStorage = new NetworkPlayerCollection<TPlayer, TSocket, TClient, TNetClient>();

            this.router = new GameServerMessageRouter<TPlayer, TSocket, TClient, TNetClient>(this, dispatcher);

            this.pingController = new GameServerPingController<TPlayer, TSocket, TClient, TNetClient>(this, this.playersStorage, dispatcher);
        }
    }
}