using System;
using System.Net;
using GameNetworking;
using GameNetworking.Commons;
using GameNetworking.Networking;
using Networking.Sockets;

using UnreliablePlayer = GameNetworking.Commons.Models.Server.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace TestClientApp {
    class Program : IMainThreadDispatcher {
        static void Main(string[] args) {
            UnreliableGameServer<UnreliablePlayer> server = new UnreliableGameServer<UnreliablePlayer>(new UnreliableNetworkingServer(new UnreliableSocket(new UDPSocket())), new Program());

            server.Start(IPAddress.Any.ToString(), 64000);

            while (true) {
                server.Update();
            }
        }

        public void Enqueue(Action action) {
            action.Invoke();
        }
    }
}
