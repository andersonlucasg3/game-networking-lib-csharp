using Logging;
using Networking.Sockets;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Commons;
using GameNetworking.Networking.Models;
using Networking.Models;
using GameNetworking.Messages.Server;

namespace GameNetworking {
    internal class GameServerClientAcceptor<TPlayer>
        where TPlayer : NetworkPlayer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>, new() {
        private readonly ReliableGameServer<TPlayer> server;
        private readonly IMainThreadDispatcher dispatcher;

        private int playerIdCounter = 1;

        public GameServerClientAcceptor(ReliableGameServer<TPlayer> server, IMainThreadDispatcher dispatcher) {
            this.server = server;
            this.dispatcher = dispatcher;
        }

        public void AcceptClient(ReliableNetworkClient client) {
            var player = new TPlayer() { client = client, playerId = this.playerIdCounter };

            this.playerIdCounter++;

            this.server.AddPlayer(player);

            Logger.Log($"(AcceptClient) count {this.server.AllPlayers().Count}");

            var players = this.server.AllPlayers();
            TPlayer each;
            for (int i = 0; i < players.Count; i++) {
                each = players[i];

                Logger.Log($"Sending ConnectedPlayerMessage from {player.playerId} to {each.playerId}");

                // Sends the connected player message to all players
                this.server.Send(new ConnectedPlayerMessage {
                    playerId = player.playerId,
                    isMe = player.Equals(each)
                }, each);

                if (each.Equals(player)) { continue; }

                Logger.Log($"Sending ConnectedPlayerMessage from {each.playerId} to {player.playerId}");

                // Sends the existing players to the player that just connected
                this.server.Send(new ConnectedPlayerMessage {
                    playerId = each.playerId,
                    isMe = false
                }, player);
            }

            this.server.listener?.GameServerPlayerDidConnect(player);
        }

        public void Disconnect(TPlayer player) {
            this.server.RemovePlayer(player);

            Logger.Log($"(Disconnect) count {this.server.AllPlayers().Count}");

            if (player != null) {
                this.server.listener?.GameServerPlayerDidDisconnect(player);
                this.server.SendBroadcast(new DisconnectedPlayerMessage { playerId = player.playerId });
            }
        }
    }
}