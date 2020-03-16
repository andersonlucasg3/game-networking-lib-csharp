using Messages.Models;

namespace GameNetworking {
    using Messages;
    using Messages.Server;
    using Executors.Client;
    using Executors;
    using Commons;
    using GameNetworking.Models.Client;
    using Logging;

    internal class GameClientMessageRouter<PlayerType> : BaseWorker<GameClient<PlayerType>> where PlayerType : NetworkPlayer, new() {
        internal GameClientMessageRouter(GameClient<PlayerType> client, IMainThreadDispatcher dispatcher) : base(client, dispatcher) { }

        private void Execute(IExecutor executor) {
            Logger.Log($"Executing {executor}");
            dispatcher.Enqueue(executor.Execute);
        }

        internal void Route(MessageContainer container) {
            if (container == null) { return; }

            var type = (MessageType)container.Type;
            Logger.Log($"Routing message type {type.ToString()}");
            switch (type) {
                case MessageType.CONNECTED_PLAYER: this.Execute(new ConnectedPlayerExecutor<PlayerType>(this.instance, container.Parse<ConnectedPlayerMessage>())); break;
                case MessageType.PING: this.Execute(new PingRequestExecutor<PlayerType>(this.instance)); break;
                case MessageType.PING_RESULT: this.Execute(new PingResultRequestExecutor<PlayerType>(this.instance, container.Parse<PingResultRequestMessage>())); break;
                case MessageType.DISCONNECTED_PLAYER: this.Execute(new DisconnectedPlayerExecutor<PlayerType>(this.instance, container.Parse<DisconnectedPlayerMessage>())); break;
                default: this.instance?.listener?.GameClientDidReceiveMessage(container); break;
            }
        }
    }
}