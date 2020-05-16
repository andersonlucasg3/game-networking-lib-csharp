#if !UNITY_64

using System;
using System.Collections.Generic;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;
using GameNetworking.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Sockets;
using Logging;

namespace TestServerApp {
    class Program : IMainThreadDispatcher, IGameServerListener<Player> {
        private readonly List<Action> actions = new List<Action>();

        private GameServer<Player> server;

        static void Main(string[] _) {
            var program = new Program();
            program.server = new GameServer<Player>(new NetworkServer(new TcpSocket(), new UdpSocket()), new GameServerMessageRouter<Player>(program));

            program.server.Start(64000);

            program.server.listener = program;

            while (true) {
                lock (program) {
                    program.actions.ForEach(a => a.Invoke());
                    program.actions.RemoveAll(_ => true);
                }

                program.server.Update();
            }
        }

        public void Enqueue(Action action) {
            lock (this) { this.actions.Add(action); }
        }

        public void GameServerPlayerDidConnect(Player player, Channel channel) {
            Logger.Log($"GameServerPlayerDidConnect - {channel}");
        }

        public void GameServerPlayerDidDisconnect(Player player) {
            Logger.Log("GameServerPlayerDidDisconnect");
        }

        public void GameServerDidReceiveClientMessage(MessageContainer container, Player player) {
            Logger.Log($"GameServerDidReceiveClientMessage - type {container.type}");
            if (container.type == 1001) {
                this.Send(container);
            }
        }

        private void Send(MessageContainer message) {
            this.Enqueue(new Executor<Executor, GameServer<Player>, Message>(this.server, message).Execute);
        }
    }

    struct Message : ITypedMessage {
        public int type => 1001;

        public int playerId;
        public int messageId;

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

    struct Executor : IExecutor<GameServer<Player>, Message> {
        void IExecutor<GameServer<Player>, Message>.Execute(GameServer<Player> model, Message message) {
            var player = model.playerCollection.FindPlayer(message.playerId);
            Logger.Log($"Received message from playerId-{message.playerId}, as playerId-{player.playerId}, messageId-{message.messageId}");
            model.SendBroadcast(message, Channel.reliable);
            model.SendBroadcast(message, Channel.unreliable);
        }
    }
}

#endif