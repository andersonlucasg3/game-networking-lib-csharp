using GameNetworking.Commons;
using GameNetworking.Executors;
using GameNetworking.Executors.Server;
using GameNetworking.Messages;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Models;

namespace GameNetworking.Server {
    public class GameServerMessageRouter<TPlayer> : IPlayerMessageListener
        where TPlayer : class, IPlayer {

        private readonly IMainThreadDispatcher dispatcher;

        protected IGameServer<TPlayer> server { get; private set; }

        public GameServerMessageRouter(IMainThreadDispatcher dispatcher) {
            this.dispatcher = dispatcher;
        }

        internal void Configure(IGameServer<TPlayer> server) {
            this.server = server;
        }

        protected void Execute(IExecutor executor) {
            dispatcher.Enqueue(executor.Execute);
        }

        protected virtual void Route(MessageContainer container, TPlayer player) {
            if (container == null) { return; }

            var type = (MessageType)container.type;

            switch (type) {
                case MessageType.pong: Execute(new PongRequestExecutor<TPlayer>(this.server, player)); break;
                default: this.server.listener?.GameServerDidReceiveClientMessage(container, player); break;
            }
        }

        void IPlayerMessageListener.PlayerDidReceiveMessage(MessageContainer container, IPlayer from) {
            this.Route(container, from as TPlayer);
        }
    }
}
