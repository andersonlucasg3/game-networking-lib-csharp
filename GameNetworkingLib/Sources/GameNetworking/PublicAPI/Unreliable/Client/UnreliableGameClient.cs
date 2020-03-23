using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking {
    public class UnreliableGameClient<TPlayer> : GameClient<UnreliableNetworkingClient, TPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>
        where TPlayer : class, INetworkPlayer<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, new() {
        public UnreliableGameClient(UnreliableNetworkingClient backend, IMainThreadDispatcher dispatcher) : base(backend, dispatcher) { }

        public void Connect(string host, int port) {
            this.networkingClient.Connect(host, port);
        }
    }
}