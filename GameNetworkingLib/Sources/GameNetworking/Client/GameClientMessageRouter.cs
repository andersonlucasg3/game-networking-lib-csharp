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
        where TPlayer : Player, new() {
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
            case MessageType.connectedPlayer: EnqueueConnectedPlayer(container); break;
            case MessageType.ping: EnqueuePing(container); break;
            case MessageType.pingResult: EnqueuePingResult(container); break;
            case MessageType.disconnectedPlayer: EnqueueDisconnectedPlayer(container); break;
            default: game?.listener?.GameClientDidReceiveMessage(container); break;
            }
        }

        private void Execute<TExecutor, TModel, TMessage>(Executor<TExecutor, TModel, TMessage> executor)
            where TExecutor : struct, IExecutor<TModel, TMessage>
            where TMessage : struct, ITypedMessage {
            dispatcher.Enqueue(executor.Execute);
        }

        private void EnqueueConnectedPlayer(MessageContainer message) {
            var executor = new Executor<
                ConnectedPlayerExecutor,
                IRemoteClientListener,
                ConnectedPlayerMessage>(game, message);
            Execute(executor);
        }

        private void EnqueuePing(MessageContainer message) {
            var executor = new Executor<
                PingRequestExecutor<TPlayer>,
                GameClient<TPlayer>,
                PingRequestMessage
                >(game, message);
            Execute(executor);
        }

        private void EnqueuePingResult(MessageContainer message) {
            var executor = new Executor<
                PingResultRequestExecutor<TPlayer>,
                GameClient<TPlayer>,
                PingResultRequestMessage
                >(game, message);
            Execute(executor);
        }

        private void EnqueueDisconnectedPlayer(MessageContainer message) {
            var executor = new Executor<
                DisconnectedPlayerExecutor,
                IRemoteClientListener,
                DisconnectedPlayerMessage
                >(game, message);
            Execute(executor);
        }
    }
}