namespace GameNetworking {
    using Models;
    using Models.Contract.Server;
    using Messages.Server;
    using Commons;
    using Logging;

    internal class GameServerClientAcceptor<PlayerType> : BaseWorker<GameServer<PlayerType>> where PlayerType : class, INetworkPlayer, new() {
        public GameServerClientAcceptor(GameServer<PlayerType> server, IMainThreadDispatcher dispatcher) : base(server, dispatcher) { }

        public void AcceptClient(NetworkClient client) {
            var player = new PlayerType() { client = client };
            this.instance.AddPlayer(player);
            instance.listener.GameServerPlayerDidConnect(player);

            Logger.Log($"(AcceptClient) count {this.instance.AllPlayers().Count}");

            var players = this.instance.AllPlayers();
            PlayerType each;
            for (int i = 0; i < players.Count; i++) {
                each = players[i];

                // Sends the connected player message to all players
                this.instance.Send(new ConnectedPlayerMessage {
                    playerId = player.playerId,
                    isMe = (player.Equals(each))
                }, each);

                if (each.Equals(player)) { return; }

                // Sends the existing players to the player that just connected
                this.instance.Send(new ConnectedPlayerMessage {
                    playerId = each.playerId,
                    isMe = false
                }, player);
            }
        }

        public void Disconnect(PlayerType player) {
            this.instance.RemovePlayer(player);

            Logger.Log($"(Disconnect) count {this.instance.AllPlayers().Count}");

            if (player != null) {
                this.instance.listener?.GameServerPlayerDidDisconnect(player);
                this.instance.SendBroadcast(new DisconnectedPlayerMessage { playerId = player.playerId });
            }
        }
    }
}