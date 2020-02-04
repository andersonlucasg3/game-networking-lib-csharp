using Messages.Models;

namespace GameNetworking {
    using Messages;
    using Messages.Server;
    using Executors.Client;
    using Executors;
    using Commons;

    internal class GameClientMessageRouter : BaseWorker<GameClient> {
        internal GameClientMessageRouter(GameClient client, IMainThreadDispatcher dispatcher) : base(client, dispatcher) { }

        private void Execute(IExecutor executor) {
            dispatcher.Enqueue(executor.Execute);
        }

        internal void Route(MessageContainer container) {
            if (container == null) { return; }

            switch ((MessageType)container.Type) {
            case MessageType.CONNECTED_PLAYER: this.Execute(new ConnectedPlayerExecutor(this.instance, container.Parse<ConnectedPlayerMessage>())); break;
            case MessageType.PING: this.Execute(new PingRequestExecutor(this.instance)); break;
            case MessageType.PING_RESULT: this.Execute(new PingResultRequestExecutor(this.instance, container.Parse<PingResultRequestMessage>())); break;
            case MessageType.DISCONNECTED_PLAYER: this.Execute(new DisconnectedPlayerExecutor(this.instance, container.Parse<DisconnectedPlayerMessage>())); break;
            default: this.instance?.listener?.GameClientDidReceiveMessage(container); break;
            }
        }
    }
}