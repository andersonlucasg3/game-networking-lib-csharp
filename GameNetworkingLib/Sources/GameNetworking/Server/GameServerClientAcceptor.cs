using GameNetworking.Channels;
using GameNetworking.Messages.Server;
using Logging;

namespace GameNetworking.Server {
    public interface IGameServerClientAcceptorListener<TPlayer> : IGameServer<TPlayer>
        where TPlayer : class, IPlayer {
        void ClientAcceptorPlayerDidConnect(TPlayer player);
        void ClientAcceptorPlayerDidDisconnect(TPlayer player);
    }

    public sealed class GameServerClientAcceptor<TPlayer>
        where TPlayer : class, IPlayer {
        private int playerIdCounter = 0;

        public IGameServerClientAcceptorListener<TPlayer> listener { get; set; }

        public GameServerClientAcceptor() { }

        public void AcceptClient(TPlayer player) {
            player.Configure(this.playerIdCounter++);

            if (Logger.IsLoggingEnabled) { Logger.Log($"(AcceptClient) count {this.listener.playerCollection.count}"); }

            var players = this.listener.playerCollection;
            TPlayer each;
            for (int i = 0; i < players.count; i++) {
                each = players[i];

                // Sends the connected player message to all players
                var connectedAll = new ConnectedPlayerMessage {
                    playerId = player.playerId,
                    isMe = player.Equals(each)
                };
                each.Send(connectedAll, Channel.reliable);

                if (each.Equals(player)) { continue; }

                // Sends the existing players to the player that just connected
                var connectedSelf = new ConnectedPlayerMessage {
                    playerId = each.playerId,
                    isMe = false
                };
                player.Send(connectedSelf, Channel.reliable);
            }

            this.listener.ClientAcceptorPlayerDidConnect(player);
        }

        public void Disconnect(TPlayer player) {
            if (Logger.IsLoggingEnabled) { Logger.Log($"(Disconnect) count {this.listener.playerCollection.count}"); }

            if (player != null) {
                this.listener.ClientAcceptorPlayerDidDisconnect(player);
                this.listener.SendBroadcast(new DisconnectedPlayerMessage { playerId = player.playerId }, Channel.reliable);
            }
        }
    }
}