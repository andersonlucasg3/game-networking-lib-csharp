using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Messages.Server;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Executors.Client {
    internal class PingResultRequestExecutor<TPlayer, TSocket, TClient, TNetClient> : BaseExecutor<IGameClient<TPlayer, TSocket, TClient, TNetClient>>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private readonly PingResultRequestMessage message;

        public PingResultRequestExecutor(IGameClient<TPlayer, TSocket, TClient, TNetClient> client, PingResultRequestMessage message) : base(client) {
            this.message = message;
        }

        public override void Execute() {
            if (!(this.instance.FindPlayer(this.message.playerId) is NetworkPlayer<TSocket, TClient, TNetClient> player)) { return; }
            player.mostRecentPingValue = this.message.pingValue;
        }
    }
}
