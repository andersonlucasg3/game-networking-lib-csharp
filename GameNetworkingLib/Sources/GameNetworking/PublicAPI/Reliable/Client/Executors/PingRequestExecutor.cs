using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Contract.Client;
using GameNetworking.Messages.Client;
using GameNetworking.Networking.Commons;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Executors.Client {
    internal class PingRequestExecutor<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient> : IExecutor 
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TNetworkingClient : INetworkingClient<TSocket, TClient, TNetClient>
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        private GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient> client;

        public PingRequestExecutor(GameClient<TNetworkingClient, TPlayer, TSocket, TClient, TNetClient> client) {
            this.client = client;
        }

        public void Execute() {
            this.client.Send(new PongRequestMessage());
        }
    }
}