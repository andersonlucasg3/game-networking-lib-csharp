using GameNetworking.Commons;
using GameNetworking.Executors.Server;
using GameNetworking.Messages;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Models;

namespace GameNetworking.Server {
    public class GameServerMessageRouter<TPlayer> : IPlayerMessageListener
        where TPlayer : Player, new() {
        protected GameServer<TPlayer> server { get; private set; }

        public readonly IMainThreadDispatcher dispatcher;

        public GameServerMessageRouter(IMainThreadDispatcher dispatcher) {
            this.dispatcher = dispatcher;
        }

        internal void Configure(GameServer<TPlayer> server) {
            this.server = server;
        }

        protected virtual void Route(MessageContainer container, TPlayer player) {
            var type = (MessageType)container.type;

            switch (type) {
            case MessageType.pong: EnqueuePong(player, container); break;
            default: server.listener?.GameServerDidReceiveClientMessage(container, player); break;
            }
        }

        void IPlayerMessageListener.PlayerDidReceiveMessage(MessageContainer container, IPlayer from) {
            Route(container, from as TPlayer);
        }

        protected void Execute<TExecutor, TModel, TMessage>(Executor<TExecutor, TModel, TMessage> executor)
            where TExecutor : struct, IExecutor<TModel, TMessage>
            where TMessage : struct, ITypedMessage {
            dispatcher.Enqueue(executor.Execute);
        }

        private void EnqueuePong(TPlayer player, MessageContainer message) {
            var executor = new Executor<
                PongRequestExecutor<TPlayer>,
                ServerModel<TPlayer>,
                PongRequestMessage
                >(new ServerModel<TPlayer>(server, player), message);
            Execute(executor);
        }

        public struct ServerModel<TModel> {
            public readonly GameServer<TPlayer> server;
            public readonly TModel model;

            public ServerModel(GameServer<TPlayer> server, TModel model) {
                this.server = server;
                this.model = model;
            }
        }
    }
}
