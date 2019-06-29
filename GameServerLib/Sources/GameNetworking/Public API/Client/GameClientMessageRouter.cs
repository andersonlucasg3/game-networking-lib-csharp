using Messages.Models;

namespace GameNetworking {
    using Messages.Server;
    using Executors.Client;

    internal class GameClientMessageRouter: BaseClientWorker {
        internal GameClientMessageRouter(GameClient client) : base(client) { }

        internal void Route(MessageContainer container) {
            if (container == null) { return; }

            if (container.Is(typeof(ConnectedPlayerMessage))) {
                new ConnectedPlayerExecutor(this.Client, container.Parse<ConnectedPlayerMessage>()).Execute();
            } else if (container.Is(typeof(PlayerSpawnMessage))) {
                new PlayerSpawnExecutor(this.Client, container.Parse<PlayerSpawnMessage>()).Execute();
            } else if (container.Is(typeof(SyncMessage))) {
                new SyncExecutor(this.Client, container.Parse<SyncMessage>()).Execute();
            } else {
                this.Client?.Delegate?.GameClientDidReceiveMessage(container);
            }
        }
    }
}