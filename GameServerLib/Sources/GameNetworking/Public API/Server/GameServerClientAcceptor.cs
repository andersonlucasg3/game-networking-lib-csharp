namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;

    internal class GameServerClientAcceptor: BaseServerWorker, INetworkingServerDelegate {
        public GameServerClientAcceptor(GameServer server) : base(server) {
            this.Server.networkingServer.Delegate = this;
        }

        #region INetworkingServerDelegate

        void INetworkingServerDelegate.NetworkingServerDidAcceptClient(NetworkClient client) {
            NetworkPlayer player = new NetworkPlayer(client);
            this.Server.SendBroadcast(new ConnectedPlayerMessage { playerId = player.PlayerId, isMe = false });
            this.Server.AddPlayer(player);
            this.Server.Send(new ConnectedPlayerMessage { playerId = player.PlayerId, isMe = true }, client);

            this.Server.AllPlayers().ForEach(each => {
                if (each.Client == client) { return; }
                var connected = new ConnectedPlayerMessage();
                connected.playerId = each.PlayerId;
                connected.isMe = false;
                this.Server.Send(connected, client);
            });
        }

        void INetworkingServerDelegate.NetworkingServerClientDidDisconnect(NetworkClient client) {
            var player = this.Server.FindPlayer(client);
            if (player != null) { this.Server.Delegate?.GameServerPlayerDidDisconnect(player); }
        }

        #endregion
    }
}