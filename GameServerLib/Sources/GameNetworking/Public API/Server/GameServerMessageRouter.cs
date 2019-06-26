using Messages.Models;
using System;

namespace GameNetworking {
    using Models;
    using Messages.Client;
    using Executors;
    using Executors.Server;

    internal class GameServerMessageRouter: BaseServerWorker, INetworkingServerMessagesDelegate {
        internal GameServerMessageRouter(GameServer server) : base(server) {
            server.networkingServer.MessagesDelegate = this;
        }

        private void Execute(IExecutor executor) {
            executor.Execute();
        }

        #region INetworkingServerMessagesDelegate

        void INetworkingServerMessagesDelegate.NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client) {
            var pair = this.Server.FindPair(client);
            if (container.Is(typeof(SpawnRequestMessage))) {
                this.Execute(new SpawnRequestExecutor(this.Server, container.Parse<SpawnRequestMessage>(), pair));
            } else {
                this.Server.Delegate?.GameServerDidReceiveClientMessage(container, pair.Player);
            }
        }

        #endregion
    }
}
