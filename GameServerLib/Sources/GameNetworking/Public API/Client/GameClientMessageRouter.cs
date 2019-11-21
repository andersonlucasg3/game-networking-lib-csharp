using Messages.Models;

namespace GameNetworking {
    using Messages;
    using Messages.Server;
    using Executors.Client;
    using Executors;
    using Commons;

    internal class GameClientMessageRouter : BaseWorker<GameClient> {
        internal GameClientMessageRouter(GameClient client) : base(client) { }

        private void Execute(IExecutor executor) {
            UnityMainThreadDispatcher.Instance().Enqueue(executor.Execute);
        }

        internal void Route(MessageContainer container) {
            if (container == null) { return; }

            switch ((MessageType)container.Type) {
            case MessageType.CONNECTED_PLAYER:
                Execute(new ConnectedPlayerExecutor(this.Instance, container.Parse<ConnectedPlayerMessage>()));
                break;
            case MessageType.SPAWN_REQUEST:
                Execute(new PlayerSpawnExecutor(this.Instance, container.Parse<PlayerSpawnMessage>()));
                break;
            case MessageType.SYNC:
                Execute(new SyncExecutor(this.Instance, container.Parse<SyncMessage>()));
                break;
            case MessageType.MOVE_REQUEST:
                Execute(new ClientMoveRequestExecutor(this.Instance, container.Parse<MoveRequestMessage>()));
                break;
            case MessageType.PING:
                Execute(new PingRequestExecutor(this.Instance));
                break;
            case MessageType.PING_RESULT:
                Execute(new PingResultRequestExecutor(this.Instance, container.Parse<PingResultRequestMessage>()));
                break;

            default:
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    this.Instance?.Delegate?.GameClientDidReceiveMessage(container);
                });
                break;
            }
        }
    }
}