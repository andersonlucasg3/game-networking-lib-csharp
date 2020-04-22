using GameNetworking.Client;
using GameNetworking.Executors;
using GameNetworking.Executors.Client;
using GameNetworking.Messages;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Server;

namespace GameNetworking.Commons.Client {
    public class GameClientMessageRouter<TPlayer>
        where TPlayer : class, IPlayer {

        private readonly IMainThreadDispatcher dispatcher;
        protected IGameClient<TPlayer> game { get; private set; }

        public GameClientMessageRouter(IMainThreadDispatcher dispatcher) {
            this.dispatcher = dispatcher;
        }

        internal void Configure(IGameClient<TPlayer> game) {
            this.game = game;
        }

        protected void Execute(IExecutor executor) {
            dispatcher.Enqueue(executor.Execute);
        }

        internal virtual void Route(MessageContainer container) {
            if (container == null) { return; }

            switch ((MessageType)container.type) {
                case MessageType.connectedPlayer: this.Execute(new ConnectedPlayerExecutor(this.game as IRemoteClientListener, container.Parse<ConnectedPlayerMessage>())); break;
                case MessageType.ping: this.Execute(new PingRequestExecutor<TPlayer>(this.game)); break;
                case MessageType.pingResult: this.Execute(new PingResultRequestExecutor<TPlayer>(this.game, container.Parse<PingResultRequestMessage>())); break;
                case MessageType.disconnectedPlayer: this.Execute(new DisconnectedPlayerExecutor(this.game as IRemoteClientListener, container.Parse<DisconnectedPlayerMessage>())); break;
                default: this.game?.listener?.GameClientDidReceiveMessage(container); break;
            }
        }
    }
}