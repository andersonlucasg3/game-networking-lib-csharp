using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Messages.Server;
using GameNetworking.Networking.Commons;
using Logging;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons.Server {
    public abstract class GameServerClientAcceptor<TGame, TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>
        where TGame : IGameServer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient>
        where TNetworkingServer : INetworkingServer<TSocket, TClient, TNetClient>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        public interface IListener : IGameServer<TNetworkingServer, TPlayer, TSocket, TClient, TNetClient> {
            void ClientAcceptorPlayerDidConnect(TPlayer player);
            void ClientAcceptorPlayerDidDisconnect(TPlayer player);
        }

        private int playerIdCounter = 0;

        public IListener listener { get; set; }

        public GameServerClientAcceptor() { }

        public void AcceptClient(TClient client) {
            var player = new TPlayer();
            player.Configure(client, this.playerIdCounter++);

            this.listener.AddPlayer(player);

            if (Logger.IsLoggingEnabled) { Logger.Log($"(AcceptClient) count {this.listener.AllPlayers().Count}"); }

            var players = this.listener.AllPlayers();
            TPlayer each;
            for (int i = 0; i < players.Count; i++) {
                each = players[i];

                // Sends the connected player message to all players
                this.listener.Send(new ConnectedPlayerMessage {
                    playerId = player.playerId,
                    isMe = player.Equals(each)
                }, each);

                if (each.Equals(player)) { continue; }

                // Sends the existing players to the player that just connected
                this.listener.Send(new ConnectedPlayerMessage {
                    playerId = each.playerId,
                    isMe = false
                }, player);
            }

            this.listener.ClientAcceptorPlayerDidConnect(player);
        }

        public void Disconnect(TPlayer player) {
            if (player == null) { return; }

            this.listener.RemovePlayer(player);

            if (Logger.IsLoggingEnabled) { Logger.Log($"(Disconnect) count {this.listener.AllPlayers().Count}"); }

            if (player != null) {
                this.listener.ClientAcceptorPlayerDidDisconnect(player);
                this.listener.SendBroadcast(new DisconnectedPlayerMessage { playerId = player.playerId });
            }
        }
    }
}