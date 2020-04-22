#if !UNITY_64

using System;
using System.Collections.Generic;
using GameNetworking.Channels;
using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Sockets;
using Logging;

namespace TestClientApp {
    class Program : IMainThreadDispatcher, IGameClientListener<Player> {
        private readonly List<Action> actions = new List<Action>();

        private GameClient<Player> client;
        private int counter = 0;

        static void Main(string[] _) {
            var program = new Program();
            program.client = new GameClient<Player>(new NetworkClient(new TcpSocket(), new UdpSocket()), new GameClientMessageRouter<Player>(program));

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

        public void GameClientDidIdentifyLocalPlayer(Player player) {
            Logger.Log("GameClientDidIdentifyLocalPlayer");
            this.Send();
        }

        public void GameClientPlayerDidConnect(Player player) {
            Logger.Log("GameClientNetworkPlayerDidConnect");
        }

        public void GameClientPlayerDidDisconnect(Player player) {
            Logger.Log("GameClientNetworkPlayerDidDisconnect");
        }

        public void GameClientDidReceiveMessage(MessageContainer container) {
            Logger.Log("GameClientDidReceiveMessage");
            if (container.type == 1002) {
                this.Send();
            }
        }

        private void Send() {
            this.client.Send(new Message(), Channel.unreliable);
            Logger.Log($"Send message! {counter++}");
            if (this.client.localPlayer == null) { return; }
            var ping = this.client.localPlayer.mostRecentPingValue;
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