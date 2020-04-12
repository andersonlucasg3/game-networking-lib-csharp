using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using GameNetworking;
using GameNetworking.Commons;
using GameNetworking.Networking;
using Networking.Sockets;

using UnreliablePlayer = GameNetworking.Commons.Models.Client.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace TestClientApp {
    class Program : IMainThreadDispatcher {
        private readonly List<Action> actions = new List<Action>();

        static void Main(string[] args) {
            var program = new Program();
            UnreliableGameClient<UnreliablePlayer> client = new UnreliableGameClient<UnreliablePlayer>(new UnreliableNetworkingClient(new UnreliableSocket(new UDPSocket())), program);

            client.Start(IPAddress.Any.ToString(), 63000);
            client.Connect(args[0], 64000);

            while (true) {
                var copyActions = new List<Action>(program.actions);
                program.actions.RemoveAll(_ => true);
                copyActions.ForEach(each => each.Invoke());

                client.Update();
            }
        }

        public void Enqueue(Action action) {
            this.actions.Add(action);
        }
    }
}
