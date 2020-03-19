using Messages.Models;

namespace GameNetworking {
    using Messages;
    using Executors.Server;
    using Executors;
    using Commons;
    using GameNetworking.Models.Server;

    internal class GameServerMessageRouter<PlayerType> : BaseWorker<GameServer<PlayerType>> where PlayerType : NetworkPlayer, new() {
        internal GameServerMessageRouter(GameServer<PlayerType> server, IMainThreadDispatcher dispatcher) : base(server, dispatcher) { }

        private void Execute(IExecutor executor) {
            dispatcher.Enqueue(executor.Execute);
        }

        public void Route(MessageContainer container, PlayerType player) {
            if (container == null) { return; }

            switch ((MessageType)container.Type) {
            case MessageType.pong:
                Execute(new PongRequestExecutor<PlayerType>(this.instance, player));
                break;

            default:
                this.instance.listener?.GameServerDidReceiveClientMessage(container, player);
                break;
            }
        }
    }
}
