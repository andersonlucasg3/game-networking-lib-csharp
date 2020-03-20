namespace GameNetworking {
    using Models;
    using Messages.Server;
    using Commons;
    using Logging;
    using GameNetworking.Models.Server;

    internal class GameServerClientAcceptor<PlayerType> : BaseExecutor<ReliableGameServer<PlayerType>> where PlayerType : NetworkPlayer, new() {
        private int playerIdCounter = 1;

        public GameServerClientAcceptor(ReliableGameServer<PlayerType> server, IMainThreadDispatcher dispatcher) : base(server, dispatcher) { }

        public void AcceptClient(NetworkClient client) {
            var player = new PlayerType() { client = client, playerId = this.playerIdCounter };

            this.playerIdCounter++;

            this.instance.AddPlayer(player);

            Logger.Log($"(AcceptClient) count {this.instance.AllPlayers().Count}");

            var players = this.instance.AllPlayers();
            PlayerType each;
            for (int i = 0; i < players.Count; i++) {
                each = players[i];

                Logger.Log($"Sending ConnectedPlayerMessage from {player.playerId} to {each.playerId}");

                // Sends the connected player message to all players
                this.instance.Send(new ConnectedPlayerMessage {
                    playerId = player.playerId,
                    isMe = player.Equals(each)
                }, each);

                if (each.Equals(player)) { continue; }

                Logger.Log($"Sending ConnectedPlayerMessage from {each.playerId} to {player.playerId}");

                // Sends the existing players to the player that just connected
                this.instance.Send(new ConnectedPlayerMessage {
                    playerId = each.playerId,
                    isMe = false
                }, player);
            }

            instance.listener.GameServerPlayerDidConnect(player);
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