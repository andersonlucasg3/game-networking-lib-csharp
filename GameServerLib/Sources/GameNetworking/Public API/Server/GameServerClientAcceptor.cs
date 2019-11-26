namespace GameNetworking {
    using Models;
    using Models.Server;
    using Messages.Server;
    using Commons;
    using Logging;

    internal class GameServerClientAcceptor : BaseWorker<GameServer> {
        public GameServerClientAcceptor(GameServer server) : base(server) { }

        public void AcceptClient(NetworkClient client) {
            NetworkPlayer player = new NetworkPlayer(client);
            this.instance.AddPlayer(player);
            
            Logger.Log($"(AcceptClient) count {this.instance.AllPlayers().Count}");


            this.instance.AllPlayers().ForEach(each => {
                // Sends the connected player message to all players
                this.instance.Send(new ConnectedPlayerMessage {
                    playerId = player.playerId,
                    isMe = (player == each)
                }, each.client);

                if (each == player) { return; }

                // Sends the existing players to the player that just connected
                this.instance.Send(new ConnectedPlayerMessage {
                    playerId = each.playerId,
                    isMe = false
                }, player.client);
            });
        }

        public void Disconnect(NetworkClient client) {
            var player = this.instance.FindPlayer(client);
            this.instance.RemovePlayer(player);

            Logger.Log($"(Disconnect) count {this.instance.AllPlayers().Count}");

            if (player != null) {
                this.instance.listener?.GameServerPlayerDidDisconnect(player);
                this.instance.SendBroadcast(new DisconnectedPlayerMessage { playerId = player.playerId });
            }
        }
    }
}