using System;
using System.Net;
using GameNetworking;
using GameNetworking.Commons;
using GameNetworking.Networking;
using Networking.Sockets;

using UnreliablePlayer = GameNetworking.Commons.Models.Client.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace TestClientApp {

    class Program : IMainThreadDispatcher {
        static void Main(string[] args) {
            UnreliableGameClient<UnreliablePlayer> client = new UnreliableGameClient<UnreliablePlayer>(new UnreliableNetworkingClient(new UnreliableSocket(new UDPSocket())), new Program());

            client.Start(IPAddress.Any.ToString(), 63000);
            client.Connect(args[0], 64000);

            while (true) {
                client.Update();
            }
        }

        public void Enqueue(Action action) {
            action.Invoke();
        }
    }
}
