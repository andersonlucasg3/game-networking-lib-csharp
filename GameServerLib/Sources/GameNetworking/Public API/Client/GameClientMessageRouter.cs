using Messages.Models;

namespace GameNetworking {
    using Messages;
    using Messages.Server;
    using Executors.Client;

    internal class GameClientMessageRouter: BaseWorker<GameClient> {
        internal GameClientMessageRouter(GameClient client) : base(client) { }

        internal void Route(MessageContainer container) {
            if (container == null) { return; }

            switch ((MessageType)container.Type) {
                case MessageType.CONNECTED_PLAYER:
                    new ConnectedPlayerExecutor(this.Instance, container.Parse<ConnectedPlayerMessage>()).Execute();
                    break;
                case MessageType.SPAWN_REQUEST:
                    new PlayerSpawnExecutor(this.Instance, container.Parse<PlayerSpawnMessage>()).Execute();
                    break;
                case MessageType.SYNC:
                    new SyncExecutor(this.Instance, container.Parse<SyncMessage>()).Execute();
                    break;
                case MessageType.MOVE_REQUEST:
                    new ClientMoveRequestExecutor(this.Instance, container.Parse<MoveRequestMessage>()).Execute();
                    break;
                default:
                    this.Instance?.Delegate?.GameClientDidReceiveMessage(container);
                    break;
            }
        }
    }
}