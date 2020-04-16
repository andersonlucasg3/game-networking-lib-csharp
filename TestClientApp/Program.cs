#if !UNITY_64

using System;
using System.Collections.Generic;
using System.Net;
using GameNetworking;
using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Logging;
using Messages.Coders;
using Messages.Models;
using Networking.Models;
using Networking.Sockets;

using UnreliablePlayer = GameNetworking.Commons.Models.Client.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace TestClientApp {
    class Program : IMainThreadDispatcher, IGameClientListener<UnreliablePlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient> {
        private readonly List<Action> actions = new List<Action>();

        private UnreliableGameClient<UnreliablePlayer> client;

        static void Main(string[] _) {
            var program = new Program();
            program.client = new UnreliableGameClient<UnreliablePlayer>(new UnreliableNetworkingClient(new UnreliableSocket(new UDPSocket())), program);

            program.client.Start(IPAddress.Any.ToString(), 63000);
            program.client.Connect("127.0.0.1", 64000);

            program.client.listener = program;

            program.Send();

            while (true) {
                var copyActions = new List<Action>(program.actions);
                program.actions.RemoveAll(_ => true);
                copyActions.ForEach(each => each.Invoke());

                program.client.Update();
            }
        }

        public void Enqueue(Action action) {
            this.actions.Add(action);
        }

        public void GameClientDidConnect() { }

        public void GameClientConnectDidTimeout() { }

        public void GameClientDidDisconnect() { }

        public void GameClientDidIdentifyLocalPlayer(UnreliablePlayer player) { }

        public void GameClientNetworkPlayerDidDisconnect(UnreliablePlayer player) { }

        public void GameClientDidReceiveMessage(MessageContainer container) {
            if (container.type == 1002) {
                this.Send();
            }
        }

        private void Send() {
            this.client.Send(new Message());
            Logger.Log("Send message!");
        }
    }

    class Message : ITypedMessage {
        public int type => 1001;

        public void Decode(IDecoder decoder) { }
        public void Encode(IEncoder encoder) { }
    }
}

#endif