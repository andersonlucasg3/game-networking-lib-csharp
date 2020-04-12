using System;
using System.Net;
using GameNetworking;
using GameNetworking.Commons;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Networking.Sockets;

using UnreliablePlayer = GameNetworking.Commons.Models.Client.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace TestServerApp {
    class Program : IMainThreadDispatcher {
        static void Main(string[] args) {
            UnreliableGameClient<UnreliablePlayer> client = new UnreliableGameClient<UnreliablePlayer>(new UnreliableNetworkingClient(new UnreliableSocket(new UDPSocket())), new Program());

            client.Start("127.0.0.1", 63000);
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