using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Messages.Server;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Executors.Client {
    internal class DisconnectedPlayerExecutor<TPlayer, TSocket, TClient, TNetClient> : BaseExecutor<IGameClient<TPlayer, TSocket, TClient, TNetClient>>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket: ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        private readonly DisconnectedPlayerMessage message;

        internal DisconnectedPlayerExecutor(IGameClient<TPlayer, TSocket, TClient, TNetClient> client, DisconnectedPlayerMessage message) : base(client) {
            this.message = message;
        }

        public override void Execute() {
            var player = this.instance.RemovePlayer(this.message.playerId);
            if (player != null) { this.instance.listener?.GameClientNetworkPlayerDidDisconnect(player); }
        }
    }
}