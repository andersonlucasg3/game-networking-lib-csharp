namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;
    using Commons;

    internal class GameServerClientAcceptor: BaseWorker<GameServer> {
        public GameServerClientAcceptor(GameServer server) : base(server) { }

        public void AcceptClient(NetworkClient client) {
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

        public void Disconnect(NetworkClient client) {
            var player = this.Instance.FindPlayer(client);
            this.Instance.RemovePlayer(player);
            if (player != null) { this.Instance.Delegate?.GameServerPlayerDidDisconnect(player); }
        }
    }
}