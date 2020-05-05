#if !UNITY_64

using System;
using System.Threading;
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
        private int? playerId;

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
                }

                Thread.Sleep(250);

                if (program.playerId.HasValue) {
                    program.Send();
                }
            }
        }

        public void Enqueue(Action action) {
            lock (this) { this.actions.Add(action); }
        }

        public void GameClientDidConnect(Channel channel) {
            Logger.Log($"GameClientDidConnect - {channel}");
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

            this.playerId = player.playerId;
            this.Send();
        }

        public void GameClientPlayerDidConnect(Player player) {
            Logger.Log($"GameClientPlayerDidConnect player-{player}");
        }

        public void GameClientPlayerDidDisconnect(Player player) {
            Logger.Log($"GameClientPlayerDidDisconnect player-{player}");
        }

        public void GameClientDidReceiveMessage(MessageContainer container) {
            Logger.Log($"GameClientDidReceiveMessage - type: {container.type}");
            if (container.type == 1001) {
                var message = container.Parse<Message>();
                Logger.Log($"Received message to playerId-{message.playerId}, and I'm playerId-{playerId.Value}, with id-{message.messageId}");
            }

        }

        private void Send() {
            this.client.Send(new Message(this.playerId.Value, this.counter++), Channel.unreliable);
        }
    }

    class Message : ITypedMessage {
        public int type => 1001;

        public int playerId;
        public int messageId;

        public Message() { }

        public Message(int playerId, int messageId) {
            this.playerId = playerId;
            this.messageId = messageId;
        }

        public void Decode(IDecoder decoder) {
            this.playerId = decoder.GetInt();
            this.messageId = decoder.GetInt();
        }

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
            encoder.Encode(this.messageId);
        }
    }
}

#endif