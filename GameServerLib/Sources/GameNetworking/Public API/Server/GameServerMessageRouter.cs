using Messages.Models;

namespace GameNetworking {
    using Models;
    using Messages;
    using Messages.Client;
    using Executors.Server;
    using Executors;
    using Commons;
    using GameNetworking.Models.Server;

    internal class GameServerMessageRouter : BaseWorker<GameServer> {
        internal GameServerMessageRouter(GameServer server) : base(server) { }

        private void Execute(IExecutor executor) {
            UnityMainThreadDispatcher.instance.Enqueue(executor.Execute);
        }

        public void Route(MessageContainer container, NetworkPlayer player) {
            if (container == null) { return; }

            switch ((MessageType)container.Type) {
            case MessageType.SPAWN_REQUEST:
                Execute(new SpawnRequestExecutor(this.instance, container.Parse<SpawnRequestMessage>(), player));
                break;
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
