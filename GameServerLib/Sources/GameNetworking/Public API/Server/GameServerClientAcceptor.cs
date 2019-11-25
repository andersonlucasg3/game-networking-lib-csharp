namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;
    using Commons;

    internal class GameServerClientAcceptor : BaseWorker<GameServer> {
        public GameServerClientAcceptor(GameServer server) : base(server) { }

        public void AcceptClient(NetworkClient client) {
            NetworkPlayer player = new NetworkPlayer(client);
            this.Instance.SendBroadcast(new ConnectedPlayerMessage { playerId = player.PlayerId, isMe = false });
            this.Instance.AddPlayer(player);
            this.Instance.Send(new ConnectedPlayerMessage { playerId = player.PlayerId, isMe = true }, client);

            Logging.Logger.Log(this.GetType(), $"(AcceptClient) count {this.Instance.AllPlayers().Count}");

            this.Instance.AllPlayers().ForEach(each => {
                if (each.Client == client) { return; }
                this.Instance.Send(new ConnectedPlayerMessage {
                    playerId = each.PlayerId,
                    isMe = false
                }, client);
            });
        }

        public void Disconnect(NetworkClient client) {
            var player = this.Instance.FindPlayer(client);
            this.Instance.RemovePlayer(player);

            Logging.Logger.Log(this.GetType(), $"(Disconnect) count {this.Instance.AllPlayers().Count}");

            if (player != null) {
                this.Instance.listener?.GameServerPlayerDidDisconnect(player);
                this.Instance.SendBroadcast(new DisconnectedPlayerMessage { playerId = player.PlayerId });
            }
        }
    }
}