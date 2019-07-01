using Messages.Models;
using System;

namespace GameNetworking {
    using Models;
    using Messages;
    using Messages.Client;
    using Executors.Server;

    internal class GameServerMessageRouter: BaseWorker<GameServer>, INetworkingServerMessagesDelegate {
        internal GameServerMessageRouter(GameServer server) : base(server) {
            server.networkingServer.MessagesDelegate = this;
        }

        #region INetworkingServerMessagesDelegate

        void INetworkingServerMessagesDelegate.NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client) {
            var player = this.Instance.FindPlayer(client);

            if (container.Is(typeof(SpawnRequestMessage))) {
                new SpawnRequestExecutor(this.Instance, container.Parse<SpawnRequestMessage>(), player).Execute();
            } else if (container.Is(typeof(MoveRequestMessage))) {
                new ServerMoveRequestExecutor(this.Instance, player, container.Parse<MoveRequestMessage>()).Execute();
            } else {
                this.Instance.Delegate?.GameServerDidReceiveClientMessage(container, player);
            }
        }

        #endregion
    }
}
