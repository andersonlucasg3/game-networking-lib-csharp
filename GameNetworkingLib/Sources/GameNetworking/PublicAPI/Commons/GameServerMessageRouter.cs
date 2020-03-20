using Messages.Models;
    using GameNetworking.Commons.Models.Server;
    using Networking.Commons.Sockets;
    using Networking.Commons.Models;
    using GameNetworking.Commons.Models;
    using GameNetworking.Executors.Server;

namespace GameNetworking {
    using Messages;
    using Executors;
    using Commons;

    internal class GameServerMessageRouter<TPlayer, TSocket, TClient, TNetClient>
        where TPlayer : NetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private readonly GameServer<TPlayer, TSocket, TClient, TNetClient> server;
        private readonly IMainThreadDispatcher dispatcher;

        internal GameServerMessageRouter(GameServer<TPlayer, TSocket, TClient, TNetClient> server, IMainThreadDispatcher dispatcher) {
            this.server = server;
            this.dispatcher = dispatcher;
        }

        private void Execute(IExecutor executor) {
            dispatcher.Enqueue(executor.Execute);
        }

        public void Route(MessageContainer container, TPlayer player) {
            if (container == null) { return; }

            switch ((MessageType)container.Type) {
                case MessageType.pong:
                    Execute(new PongRequestExecutor<TPlayer, TSocket, TClient, TNetClient>(this.server, player));
                    break;

                default:
                    this.server.listener?.GameServerDidReceiveClientMessage(container, player);
                    break;
            }
        }
    }
}
