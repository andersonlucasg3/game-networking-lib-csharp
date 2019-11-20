using Messages.Models;

namespace GameNetworking {
    using Models;
    using Messages;
    using Messages.Client;
    using Executors.Server;
    using Executors;
    using Commons;

    internal class GameServerMessageRouter : BaseWorker<GameServer> {
        internal GameServerMessageRouter(GameServer server) : base(server) { }

        private void Execute(IExecutor executor) {
            UnityMainThreadDispatcher.Instance().Enqueue(executor.Execute);
        }

        public void Route(MessageContainer container, NetworkClient client) {
            if (container == null) { return; }

            var player = this.Instance.FindPlayer(client);

            switch ((MessageType)container.Type) {
            case MessageType.SPAWN_REQUEST:
                Execute(new SpawnRequestExecutor(this.Instance, container.Parse<SpawnRequestMessage>(), player));
                break;
            case MessageType.MOVE_REQUEST:
                Execute(new ServerMoveRequestExecutor(this.Instance, player, container.Parse<MoveRequestMessage>()));
                break;
            case MessageType.PONG:
                Execute(new PongRequestExecutor(this.Instance, player));
                break;

            default:
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    this.Instance.Delegate?.GameServerDidReceiveClientMessage(container, player);
                });
                break;
            }
        }
    }
}
