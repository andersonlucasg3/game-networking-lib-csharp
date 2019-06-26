using Messages.Models;

namespace GameNetworking {
    using Messages.Server;
    using Messages.Client;
    using Executors;
    using Executors.Client;

    internal class GameClientMessageRouter: BaseClientWorker {
        internal GameClientMessageRouter(GameClient client) : base(client) { }

        private void Execute(IExecutor executor) {
            executor.Execute();
        }

        internal void Route(MessageContainer container) {
            if (container == null) { return; }

            if (container.Is(typeof(ConnectedPlayerMessage))) {
                this.Execute(new ConnectedPlayerExecutor(this.Client, container.Parse<ConnectedPlayerMessage>()));
            } else if (container.Is(typeof(PlayerSpawnMessage))) {

            } else {
                this.Client?.Delegate?.GameClientDidReceiveMessage(container);
            }
        }
    }
}