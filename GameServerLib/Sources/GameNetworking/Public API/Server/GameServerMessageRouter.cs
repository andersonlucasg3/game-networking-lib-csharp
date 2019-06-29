using Messages.Models;
using System;

namespace GameNetworking {
    using Models;
    using Messages.Client;
    using Executors.Server;

    internal class GameServerMessageRouter: BaseServerWorker, INetworkingServerMessagesDelegate {
        internal GameServerMessageRouter(GameServer server) : base(server) {
            server.networkingServer.MessagesDelegate = this;
        }

        #region INetworkingServerMessagesDelegate

        void INetworkingServerMessagesDelegate.NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client) {
            var player = this.Server.FindPlayer(client);

            if (container.Is(typeof(SpawnRequestMessage))) {
                new SpawnRequestExecutor(this.Server, container.Parse<SpawnRequestMessage>(), player).Execute();
            } else if (container.Is(typeof(MoveRequestMessage))) {
                new MoveRequestExecutor(this.Server, player, container.Parse<MoveRequestMessage>()).Execute();
            } else {
                this.Server.Delegate?.GameServerDidReceiveClientMessage(container, player);
            }
        }

        #endregion
    }
}
