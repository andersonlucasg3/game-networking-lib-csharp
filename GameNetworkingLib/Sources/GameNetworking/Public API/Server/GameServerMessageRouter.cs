using Messages.Models;

namespace GameNetworking {
    using Messages;
    using Executors.Server;
    using Executors;
    using Commons;
    using GameNetworking.Models.Server;

    internal class GameServerMessageRouter : BaseWorker<GameServer> {
        internal GameServerMessageRouter(GameServer server, IMainThreadDispatcher dispatcher) : base(server, dispatcher) { }

        private void Execute(IExecutor executor) {
            dispatcher.Enqueue(executor.Execute);
        }

        public void Route(MessageContainer container, NetworkPlayer player) {
            if (container == null) { return; }

            switch ((MessageType)container.Type) {
            case MessageType.PONG:
                Execute(new PongRequestExecutor(this.instance, player));
                break;

            default:
                this.instance.listener?.GameServerDidReceiveClientMessage(container, player);
                break;
            }
        }
    }
}
