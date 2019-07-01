using Messages.Models;

namespace GameNetworking {
    using Messages;
    using Messages.Server;
    using Executors.Client;

    internal class GameClientMessageRouter: BaseWorker<GameClient> {
        internal GameClientMessageRouter(GameClient client) : base(client) { }

        internal void Route(MessageContainer container) {
            if (container == null) { return; }

            if (container.Is(typeof(ConnectedPlayerMessage))) {
                new ConnectedPlayerExecutor(this.Instance, container.Parse<ConnectedPlayerMessage>()).Execute();
            } else if (container.Is(typeof(PlayerSpawnMessage))) {
                new PlayerSpawnExecutor(this.Instance, container.Parse<PlayerSpawnMessage>()).Execute();
            } else if (container.Is(typeof(SyncMessage))) {
                new SyncExecutor(this.Instance, container.Parse<SyncMessage>()).Execute();
            } else if (container.Is(typeof(MoveRequestMessage))) {
                new ClientMoveRequestExecutor(this.Instance, container.Parse<MoveRequestMessage>()).Execute();
            } else {
                this.Instance?.Delegate?.GameClientDidReceiveMessage(container);
            }
        }
    }
}