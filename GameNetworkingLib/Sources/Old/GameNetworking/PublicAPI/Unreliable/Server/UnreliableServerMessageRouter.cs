using GameNetworking.Commons;
using GameNetworking.Executors;
using GameNetworking.Executors.Server;
using GameNetworking.Messages;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Messages.Models;

namespace GameNetworking {
    public class UnreliableServerMessageRouter {
        private readonly UnreliableNetworkingServer server;
        private readonly IMainThreadDispatcher dispatcher;

        internal UnreliableServerMessageRouter(UnreliableNetworkingServer server, IMainThreadDispatcher dispatcher) {
            this.server = server;
            this.dispatcher = dispatcher;
        }

        public bool Route(MessageContainer container, UnreliableNetworkClient client) {
            var type = (MessageType)container.type;

            switch (type) {
                case MessageType.connect: this.Execute(new UnreliableConnectExecutor(this.server, client)); break;
                case MessageType.disconnect: this.Execute(new UnreliableDisconnectExecutor(this.server, client)); break;
                default: return false;
            }

            return true;
        }

        private void Execute(IExecutor executor) {
            this.dispatcher.Enqueue(executor.Execute);
        }
    }
}