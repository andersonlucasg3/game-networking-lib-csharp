using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Executors;
using GameNetworking.Executors.Client;
using GameNetworking.Messages;
using GameNetworking.Messages.Server;
using GameNetworking.Networking.Commons;
using Logging;
using Messages.Models;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons.Client {
    public class GameClientMessageRouter<TGame, TPlayer, TSocket, TClient, TNetClient>
        where TGame : IGameClient<TPlayer, TSocket, TClient, TNetClient>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        private readonly IMainThreadDispatcher dispatcher;
        protected TGame game { get; private set; }

        internal GameClientMessageRouter(IMainThreadDispatcher dispatcher) {
            this.dispatcher = dispatcher;
        }

        internal void Configure(TGame game) {
            this.game = game;
        }

        protected void Execute(IExecutor executor) {
            Logger.Log($"Enqueuing executor {executor}");
            dispatcher.Enqueue(executor.Execute);
        }

        internal virtual void Route(MessageContainer container) {
            if (container == null) { return; }

            switch ((MessageType)container.type) {
                case MessageType.connectedPlayer: this.Execute(new ConnectedPlayerExecutor<TPlayer, TSocket, TClient, TNetClient>(this.game, container.Parse<ConnectedPlayerMessage>())); break;
                case MessageType.ping: this.Execute(new PingRequestExecutor<TPlayer, TSocket, TClient, TNetClient>(this.game)); break;
                case MessageType.pingResult: this.Execute(new PingResultRequestExecutor<TPlayer, TSocket, TClient, TNetClient>(this.game, container.Parse<PingResultRequestMessage>())); break;
                case MessageType.disconnectedPlayer: this.Execute(new DisconnectedPlayerExecutor<TPlayer, TSocket, TClient, TNetClient>(this.game, container.Parse<DisconnectedPlayerMessage>())); break;
                default: this.game?.listener?.GameClientDidReceiveMessage(container); break;
            }
        }
    }

    public interface IConfigurableExecutor<TGame, TMessage> where TMessage : ITypedMessage {
        void Configure(TGame game, IMainThreadDispatcher dispatcher, TMessage message);
    }
}