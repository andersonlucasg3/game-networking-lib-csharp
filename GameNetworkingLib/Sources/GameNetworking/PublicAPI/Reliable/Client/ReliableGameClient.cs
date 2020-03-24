using GameNetworking.Commons;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Commons.Client;
using Networking.Sockets;
using GameNetworking.Networking.Models;
using Networking.Models;
using GameNetworking.Networking;

namespace GameNetworking {
    public class ReliableGameClient<TPlayer> : GameClient<ReliableNetworkingClient, TPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>
        where TPlayer : class, INetworkPlayer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>, new() {
        private readonly GameClientConnection<TPlayer> connection;

        public ReliableGameClient(ReliableNetworkingClient socket, IMainThreadDispatcher dispatcher) : base(socket, dispatcher) {
            this.connection = new GameClientConnection<TPlayer>(this);
        }

        public override void Connect(string host, int port) {
            this.connection.Connect(host, port);
        }

        public override void Disconnect() {
            this.connection.Disconnect();
        }
    }
}