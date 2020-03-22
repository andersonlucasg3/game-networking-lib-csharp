using Messages.Models;
using Networking.Commons.Sockets;
using Networking.Commons.Models;
using GameNetworking.Commons.Models;
using GameNetworking.Executors.Server;
using GameNetworking.Networking.Commons;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Messages;
using GameNetworking.Executors;

namespace GameNetworking.Commons.Server {
    public class GameServerMessageRouter<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TNetworkingServer : INetworkingServer<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private readonly IGameServer<TPlayer, TSocket, TClient, TNetClient> server;
        private readonly IMainThreadDispatcher dispatcher;

        internal GameServerMessageRouter(IGameServer<TPlayer, TSocket, TClient, TNetClient> server, IMainThreadDispatcher dispatcher) {
            this.server = server;
            this.dispatcher = dispatcher;
        }

        private void Execute(IExecutor executor) {
            dispatcher.Enqueue(executor.Execute);
        }

        public void Route(MessageContainer container, TPlayer player) {
            if (container == null) { return; }

            switch ((MessageType)container.type) {
            case MessageType.pong:
                Execute(new PongRequestExecutor<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>(this.server, player));
                break;

            default:
                this.server.listener?.GameServerDidReceiveClientMessage(container, player);
                break;
            }
        }
    }
}
