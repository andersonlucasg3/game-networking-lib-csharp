using System;
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
        where TPlayer : GameNetworking.Client.Player, new() {
        protected GameClient<TPlayer> game { get; private set; }

        public readonly IMainThreadDispatcher dispatcher;

        public GameClientMessageRouter(IMainThreadDispatcher dispatcher) {
            this.dispatcher = dispatcher;
        }

        public void Configure(GameClient<TPlayer> game) {
            this.game = game;
        }

        public virtual void Route(MessageContainer container) {
            switch ((MessageType)container.type) {
            case MessageType.connectedPlayer: this.EnqueueConnectedPlayer(container.Parse<ConnectedPlayerMessage>()); break;
            case MessageType.ping: this.EnqueuePing(container.Parse<PingRequestMessage>()); break;
            case MessageType.pingResult: this.EnqueuePingResult(container.Parse<PingResultRequestMessage>()); break;
            case MessageType.disconnectedPlayer: this.EnqueueDisconnectedPlayer(container.Parse<DisconnectedPlayerMessage>()); break;
            default: this.game?.listener?.GameClientDidReceiveMessage(container); break;
            }
        }

        private void Execute<TExecutor, TModel, TMessage>(Executor<TExecutor, TModel, TMessage> executor)
            where TExecutor : struct, IExecutor<TModel, TMessage>
            where TMessage : struct, ITypedMessage {
            dispatcher.Enqueue(executor.Execute);
        }

        private void EnqueueConnectedPlayer(ConnectedPlayerMessage message) {
            var executor = new Executor<
                ConnectedPlayerExecutor,
                IRemoteClientListener,
                ConnectedPlayerMessage>(this.game, message);
            this.Execute(executor);
        }

        private void EnqueuePing(PingRequestMessage message) {
            var executor = new Executor<
                PingRequestExecutor<TPlayer>,
                GameClient<TPlayer>,
                PingRequestMessage
                >(this.game, message);
            this.Execute(executor);
        }

        private void EnqueuePingResult(PingResultRequestMessage message) {
            var executor = new Executor<
                PingResultRequestExecutor<TPlayer>,
                GameClient<TPlayer>,
                PingResultRequestMessage
                >(this.game, message);
            this.Execute(executor);
        }

        private void EnqueueDisconnectedPlayer(DisconnectedPlayerMessage message) {
            var executor = new Executor<
                DisconnectedPlayerExecutor,
                IRemoteClientListener,
                DisconnectedPlayerMessage
                >(this.game, message);
            this.Execute(executor);
        }
    }
}