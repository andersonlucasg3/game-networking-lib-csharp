#if !UNITY_64

using System;
using System.Collections.Generic;
using GameNetworking;
using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Logging;
using Networking.Models;
using Networking.Sockets;

using UnreliablePlayer = GameNetworking.Commons.Models.Client.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace TestClientApp {
    class Program : IMainThreadDispatcher, IGameClientListener<UnreliablePlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient> {
        private readonly List<Action> actions = new List<Action>();

        private UnreliableGameClient<UnreliablePlayer> client;
        private int counter = 0;

        static void Main(string[] _) {
            var program = new Program();
            program.client = new UnreliableGameClient<UnreliablePlayer>(new UnreliableNetworkingClient(new UnreliableSocket(new UDPSocket())), program);

            program.client.Start("0.0.0.0", 63000);
            program.client.Connect("127.0.0.1", 64000);

            program.client.listener = program;

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

        public void GameClientDidConnect() {
            Logger.Log("GameClientDidConnect");
        }

        public void GameClientConnectDidTimeout() {
            Logger.Log("GameClientConnectDidTimeout");
        }

        public void GameClientDidDisconnect() {
            Logger.Log("GameClientDidDisconnect");
        }

        public void GameClientDidIdentifyLocalPlayer(UnreliablePlayer player) {
            Logger.Log("GameClientDidIdentifyLocalPlayer");
            this.Send();
        }

        public void GameClientNetworkPlayerDidDisconnect(UnreliablePlayer player) {
            Logger.Log("GameClientNetworkPlayerDidDisconnect");
        }

        public void GameClientDidReceiveMessage(MessageContainer container) {
            Logger.Log("GameClientDidReceiveMessage");
            if (container.type == 1002) {
                this.Send();
            }
        }

        private void Send() {
            this.client.Send(new Message());
            Logger.Log($"Send message! {counter++}");
            if (this.client.localPlayer == null) { return; }
            var ping = this.client.GetPing(this.client.localPlayer.playerId);
            Logger.Log($"Player ping {ping}");
        }
    }

    class Message : ITypedMessage {
        public int type => 1001;

        public void Decode(IDecoder decoder) { }
        public void Encode(IEncoder encoder) { }
    }
}

#endif