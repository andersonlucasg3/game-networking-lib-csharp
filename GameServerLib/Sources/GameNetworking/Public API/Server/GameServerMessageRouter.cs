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

        #region INetworkingServerMessagesDelegate

        void INetworkingServerMessagesDelegate.NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client) {
            var pair = this.Server.FindPair(client);

            Logging.Logger.Log(this.GetType(), string.Format("NetworkingServerDidReadMessage | container: {0}, client: {1}", container, pair));

            if (container.Is(typeof(SpawnRequestMessage))) {
                new SpawnRequestExecutor(this.Server, container.Parse<SpawnRequestMessage>(), pair).Execute();
            } else {
                this.Server.Delegate?.GameServerDidReceiveClientMessage(container, pair.Player);
            }
        }

        #endregion
    }
}
