using GameNetworking.Commmons.Client;
using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Contract.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Networking.Commons;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Executors.Client {
    internal class ConnectedPlayerExecutor<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient> : 
        BaseExecutor<GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient>>
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TNetworkingClient : INetworkingClient<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private readonly ConnectedPlayerMessage message;

        internal ConnectedPlayerExecutor(GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient> client, ConnectedPlayerMessage message) : base(client) {
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