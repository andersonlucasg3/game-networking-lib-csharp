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

        private bool askToSend = false;

        static void Main(string[] _) {
            var program = new Program();
            program.client = new GameClient<Player>(new NetworkClient(new TcpSocket(), new UdpSocket()), new GameClientMessageRouter<Player>(program));

            program.client.Connect("127.0.0.1", 64000);

            program.client.listener = program;

            while (true) {
                lock (program) {
                    program.actions.ForEach(each => each?.Invoke());
                    program.actions.RemoveAll(_ => true);
                    program.client.Update();

                    if (program.askToSend) {
                        Console.ReadLine();
                        program.Send();
                    }
                }
            }
        }

        public void Enqueue(Action action) {
            lock (this) { this.actions.Add(action); }
        }

        public void GameClientDidConnect(Channel channel) {
            Logger.Log($"GameClientDidConnect - {channel}");
            if (channel == Channel.unreliable) {
                this.Send();
                this.askToSend = true;
            }
        }

        public void GameClientConnectDidTimeout() {
            Logger.Log("GameClientConnectDidTimeout");
        }

        public void GameClientDidDisconnect() {
            Logger.Log("GameClientDidDisconnect");
        }

        public void GameClientDidIdentifyLocalPlayer(Player player) {
            Logger.Log("GameClientDidIdentifyLocalPlayer");
            Logger.Log($"Identified as {player.playerId}");
            Logger.Log($"Is local player {player.isLocalPlayer}");
        }

        public void GameClientPlayerDidConnect(Player player) {
            Logger.Log($"GameClientPlayerDidConnect player-{player}");
        }

        public void GameClientPlayerDidDisconnect(Player player) {
            Logger.Log($"GameClientPlayerDidDisconnect player-{player}");
        }

        public void GameClientDidReceiveMessage(MessageContainer container) {
            Logger.Log("GameClientDidReceiveMessage");
            Logger.Log($"Received message type: {container.type}");
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