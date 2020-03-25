using GameNetworking.Commons.Client;
using GameNetworking.Commons;
using GameNetworking.Commons.Models;
using GameNetworking.Messages.Server;
using Networking.Commons.Models;
using Networking.Commons.Sockets;
using GameNetworking.Commons.Models.Client;

namespace GameNetworking.Executors.Client {
    internal class ConnectedPlayerExecutor<TPlayer, TSocket, TClient, TNetClient> : BaseExecutor<IGameClient<TPlayer, TSocket, TClient, TNetClient>>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private readonly ConnectedPlayerMessage message;

        internal ConnectedPlayerExecutor(IGameClient<TPlayer, TSocket, TClient, TNetClient> client, ConnectedPlayerMessage message) : base(client) {
            this.message = message;
        }

        public override void Execute() {
            var player = new TPlayer() {
                playerId = this.message.playerId,
                isLocalPlayer = this.message.isMe
            };
            this.instance.AddPlayer(player);

            if (player.isLocalPlayer) {
                this.instance.listener?.GameClientDidIdentifyLocalPlayer(player);
            }
        }
    }
}