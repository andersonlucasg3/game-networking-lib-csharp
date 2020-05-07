using GameNetworking.Client;
using GameNetworking.Executors;
using GameNetworking.Executors.Client;
using GameNetworking.Messages;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Server;

namespace GameNetworking.Commons.Client {
    public interface IClientMessageRouter {
        void Route(MessageContainer container);
    }

    public class GameClientMessageRouter<TPlayer> : IClientMessageRouter
        where TPlayer : GameNetworking.Client.Player {
        protected IGameClient<TPlayer> game { get; private set; }

        public readonly IMainThreadDispatcher dispatcher;

        public GameClientMessageRouter(IMainThreadDispatcher dispatcher) {
            this.dispatcher = dispatcher;
        }

        public void Configure(IGameClient<TPlayer> game) {
            this.game = game;
        }

        public virtual void Route(MessageContainer container) {
            if (container == null) { return; }

            switch ((MessageType)container.type) {
            case MessageType.connectedPlayer: this.Execute(); break;
            case MessageType.ping: this.Execute(new PingRequestExecutor<TPlayer>(this.game, container.Parse<PingRequestMessage>())); break;
            case MessageType.pingResult: this.Execute(new PingResultRequestExecutor<TPlayer>(this.game, container.Parse<PingResultRequestMessage>())); break;
            case MessageType.disconnectedPlayer: this.Execute(new DisconnectedPlayerExecutor(this.game as IRemoteClientListener, container.Parse<DisconnectedPlayerMessage>())); break;
            default: this.game?.listener?.GameClientDidReceiveMessage(container); break;
            }
        }

        private void Execute<TExecutor, TModel, TMessage>(Executor<TExecutor, TMessage, TModel> executor)
            where TExecutor : IExecutor<TModel, TMessage>
            where TMessage : struct, ITypedMessage {
            dispatcher.Enqueue(executor.Execute);
        }

        private Executor<TExecutor, TModel, TMessage> Create<TExecutor, TModel, TMessage>(
            TExecutor executor, TModel model, TMessage message)
            where TExecutor : IExecutor<TModel, TMessage>
            where TMessage : struct, ITypedMessage {
            return new Executor<TExecutor, TMessage, TModel>(executor, model, message);
        }
    }
}