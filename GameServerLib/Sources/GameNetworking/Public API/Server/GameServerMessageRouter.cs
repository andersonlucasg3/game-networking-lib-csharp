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

            switch ((MessageType)container.Type) {
                case MessageType.SPAWN_REQUEST:
                    new SpawnRequestExecutor(this.Instance, container.Parse<SpawnRequestMessage>(), player).Execute();
                    break;
                case MessageType.MOVE_REQUEST:
                    new ServerMoveRequestExecutor(this.Instance, player, container.Parse<MoveRequestMessage>()).Execute();
                    break;

                default:
                    this.Instance.Delegate?.GameServerDidReceiveClientMessage(container, player);
                    break;
            }
        }

        #endregion
    }
}
