using Messages.Models;

namespace GameNetworking {
    using Messages;
    using Messages.Server;
    using Executors.Client;
    using Executors;
    using Commons;
    using System.Threading;

    internal class GameClientMessageRouter : BaseWorker<GameClient> {
        internal GameClientMessageRouter(GameClient client) : base(client) { }

        private void Execute(IExecutor executor) {
            UnityMainThreadDispatcher.instance.Enqueue(executor.Execute);
        }

        internal void Route(MessageContainer container) {
            if (container == null) { return; }

            switch ((MessageType)container.Type) {
            case MessageType.CONNECTED_PLAYER: this.Execute(new ConnectedPlayerExecutor(this.instance, container.Parse<ConnectedPlayerMessage>())); break;
            case MessageType.SPAWN_REQUEST: this.Execute(new PlayerSpawnExecutor(this.instance, container.Parse<PlayerSpawnMessage>())); break;
            case MessageType.SYNC: this.Execute(new SyncExecutor(this.instance, container.Parse<SyncMessage>())); break;
            case MessageType.PING: this.Execute(new PingRequestExecutor(this.instance)); break;
            case MessageType.PING_RESULT: this.Execute(new PingResultRequestExecutor(this.instance, container.Parse<PingResultRequestMessage>())); break;
            case MessageType.DISCONNECTED_PLAYER: this.Execute(new DisconnectedPlayerExecutor(this.instance, container.Parse<DisconnectedPlayerMessage>())); break;
            default: this.instance?.listener?.GameClientDidReceiveMessage(container); break;
            }
        }
    }
}