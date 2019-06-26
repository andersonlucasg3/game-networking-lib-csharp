namespace GameNetworking {
    using Models;
    using Messages.Server;

    internal class GameServerClientAcceptor: BaseServerWorker, INetworkingServerDelegate {
        public GameServerClientAcceptor(GameServer server) : base(server) {
            this.Server.networkingServer.Delegate = this;
        }

        #region INetworkingServerDelegate

        void INetworkingServerDelegate.NetworkingServerDidAcceptClient(NetworkClient client) {
            NetworkPlayer player = new NetworkPlayer();
            var pair = new ClientPlayerPair(client, player);
            this.Server.AddPair(pair);
            this.Server.BroadcastMessage(new ConnectedPlayerMessage { playerId = player.PlayerId, isMe = false }, client);
            this.Server.Send(new ConnectedPlayerMessage { playerId = player.PlayerId, isMe = true }, client);
        }

        #endregion
    }
}