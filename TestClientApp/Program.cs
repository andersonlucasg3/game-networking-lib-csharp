#if !UNITY_64

using System;
using System.Collections.Generic;
using System.Net;
using GameNetworking;
using GameNetworking.Commons;
using GameNetworking.Networking;
using Networking.Sockets;

using UnreliablePlayer = GameNetworking.Commons.Models.Client.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace TestClientApp {
    class Program : IMainThreadDispatcher {
        private static double currentTime => TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
        private static double lastConnectTime = 0;

        private readonly List<Action> actions = new List<Action>();

        static void Main(string[] _) {
            var program = new Program();
            UnreliableGameClient<UnreliablePlayer> client = new UnreliableGameClient<UnreliablePlayer>(new UnreliableNetworkingClient(new UnreliableSocket(new UDPSocket())), program);

            client.Start(IPAddress.Any.ToString(), 63000);
            client.Connect("127.0.0.1", 64000);

            lastConnectTime = currentTime;

            while (true) {
                var copyActions = new List<Action>(program.actions);
                program.actions.RemoveAll(_ => true);
                copyActions.ForEach(each => each.Invoke());

                client.Update();

                if (currentTime - lastConnectTime > 10) {
                    lastConnectTime = currentTime;

                    client.Disconnect();

                    client.Update();

                    client.Start(IPAddress.Any.ToString(), 63000);
                    client.Connect("127.0.0.1", 64000);
                }
            }
        }

        public void Enqueue(Action action) {
            this.actions.Add(action);
        }
    }
}

#endif