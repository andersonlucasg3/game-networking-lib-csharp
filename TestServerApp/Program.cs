#if !UNITY_64

using System;
using System.Collections.Generic;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Server;
using GameNetworking.Sockets;
using Logging;

namespace TestServerApp {
    class Program : IMainThreadDispatcher, IGameServerListener<Player> {
        private readonly List<Action> actions = new List<Action>();

        private GameServer<Player> server;
        private int counter = 0;

        static void Main(string[] _) {
            var program = new Program();
            program.server = new GameServer<Player>(new NetworkServer(new TcpSocket(), new UdpSocket()), new GameServerMessageRouter<Player>(program));

            program.server.Start(64000);

            program.server.listener = program;

            while (true) {
                var copyActions = new List<Action>(program.actions);
                program.actions.RemoveAll(_ => true);
                copyActions.ForEach(a => a.Invoke());

                program.server.Update();
            }
        }

        public void Enqueue(Action action) {
            this.actions.Add(action);
        }

        public void GameServerPlayerDidConnect(Player player, Channel channel) {
            Logger.Log($"GameServerPlayerDidConnect - {channel}");
        }

        public void GameServerPlayerDidDisconnect(Player player) {
            Logger.Log("GameServerPlayerDidDisconnect");
        }

        public void GameServerDidReceiveClientMessage(MessageContainer container, Player player) {
            Logger.Log("GameServerDidReceiveClientMessage");
            if (container.type == 1001) {
                this.Send(player);
            }
        }

        private void Send(Player player) {
            player.Send(new Message(), Channel.unreliable);
            Logger.Log($"Send message! {counter++}");
        }
    }

    class Message : ITypedMessage {
        public int type => 1002;

        public void Decode(IDecoder decoder) { }
        public void Encode(IEncoder encoder) { }
    }
}

#endif