using System;
using System.Collections.Generic;
using System.Net;
using GameNetworking;
using GameNetworking.Commons;
using GameNetworking.Networking;
using Networking.Sockets;

using UnreliablePlayer = GameNetworking.Commons.Models.Server.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace TestServerApp {
    class Program : IMainThreadDispatcher {
        private readonly List<Action> actions = new List<Action>();

        static void Main(string[] _) {
            var program = new Program();
            UnreliableGameServer<UnreliablePlayer> server = new UnreliableGameServer<UnreliablePlayer>(new UnreliableNetworkingServer(new UnreliableSocket(new UDPSocket())), program);

            server.Start(IPAddress.Any.ToString(), 64000);

            while (true) {
                var copyActions = new List<Action>(program.actions);
                program.actions.RemoveAll(_ => true);
                copyActions.ForEach(a => a.Invoke());

                server.Update();
            }
        }

        public void Enqueue(Action action) {
            this.actions.Add(action);
        }
    }
}