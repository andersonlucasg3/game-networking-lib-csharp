using GameNetworking.Commons;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Server;
using GameNetworking.Messages.Server;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Executors.Server {

    internal class PongRequestExecutor<TPlayer, TSocket, TClient, TNetClient> : BaseExecutor<GameServer<TPlayer, TSocket, TClient, TNetClient>> 
        where TPlayer : NetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private TPlayer player;

        public PongRequestExecutor(GameServer<TPlayer, TSocket, TClient, TNetClient> server, TPlayer player) : base(server) {
            this.player = player;
        }

        public override void Execute() {
            this.instance.pingController.PongReceived(this.player);

            var players = this.instance.AllPlayers();
            PingResultRequestMessage message;
            TPlayer player;
            for (int i = 0; i < players.Count; i++) {
                player = players[i];
                message = new PingResultRequestMessage(player.playerId, player.mostRecentPingValue);
                this.instance.Send(message, this.player);
            }
        }
    }
}