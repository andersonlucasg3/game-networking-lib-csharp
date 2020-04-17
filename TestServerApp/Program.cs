#if !UNITY_64

using System;
using System.Collections.Generic;
using GameNetworking;
using GameNetworking.Commons;
using GameNetworking.Commons.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Logging;
using Messages.Coders;
using Messages.Models;
using Networking.Models;
using Networking.Sockets;

using UnreliablePlayer = GameNetworking.Commons.Models.Server.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace TestServerApp {
    class Program : IMainThreadDispatcher, IGameServerListener<UnreliablePlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient> {
        private readonly List<Action> actions = new List<Action>();

        private UnreliableGameServer<UnreliablePlayer> server;
        private int counter = 0;

        static void Main(string[] _) {
            var program = new Program();
            program.server = new UnreliableGameServer<UnreliablePlayer>(new UnreliableNetworkingServer(new UnreliableSocket(new UDPSocket()), program), program);

            program.server.Start("127.0.0.1", 64000);

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

        public void GameServerPlayerDidConnect(UnreliablePlayer player) {
            Logger.Log("GameServerPlayerDidConnect");
        }

        public void GameServerPlayerDidDisconnect(UnreliablePlayer player) {
            Logger.Log("GameServerPlayerDidDisconnect");
        }

        public void GameServerDidReceiveClientMessage(MessageContainer container, UnreliablePlayer player) {
            Logger.Log("GameServerDidReceiveClientMessage");
            if (container.type == 1001) {
                this.Send(player);
            }
        }

        private void Send(UnreliablePlayer player) {
            this.server.Send(new Message(), player);
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