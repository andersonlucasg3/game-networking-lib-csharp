namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;

    internal class GameServerClientAcceptor: BaseWorker<GameServer>, INetworkingServerDelegate {
        public GameServerClientAcceptor(GameServer server) : base(server) {
            this.Instance.networkingServer.Delegate = this;
        }

        #region INetworkingServerDelegate

        void INetworkingServerDelegate.NetworkingServerDidAcceptClient(NetworkClient client) {
            NetworkPlayer player = new NetworkPlayer(client);
            this.Instance.SendBroadcast(new ConnectedPlayerMessage { playerId = player.PlayerId, isMe = false });
            this.Instance.AddPlayer(player);
            this.Instance.Send(new ConnectedPlayerMessage { playerId = player.PlayerId, isMe = true }, client);

            this.Instance.AllPlayers().ForEach(each => {
                if (each.Client == client) { return; }
                var connected = new ConnectedPlayerMessage();
                connected.playerId = each.PlayerId;
                connected.isMe = false;
                this.Instance.Send(connected, client);
            });
        }

        void INetworkingServerDelegate.NetworkingServerClientDidDisconnect(NetworkClient client) {
            var player = this.Instance.FindPlayer(client);
            if (player != null) { this.Instance.Delegate?.GameServerPlayerDidDisconnect(player); }
        }

        #endregion
    }
}