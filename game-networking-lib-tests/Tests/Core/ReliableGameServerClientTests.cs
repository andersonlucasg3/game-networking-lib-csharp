using System.Collections.Generic;
using System.Threading;
using GameNetworking;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Logging;
using Messages.Models;
using Networking;
using Networking.Models;
using Networking.Sockets;
using NUnit.Framework;
using Tests.Core.Model;

using ReliableClientPlayer = GameNetworking.Commons.Models.Client.NetworkPlayer<Networking.Sockets.ITCPSocket, GameNetworking.Networking.Models.ReliableNetworkClient, Networking.Models.ReliableNetClient>;
using ReliableServerPlayer = GameNetworking.Commons.Models.Server.NetworkPlayer<Networking.Sockets.ITCPSocket, GameNetworking.Networking.Models.ReliableNetworkClient, Networking.Models.ReliableNetClient>;

namespace Tests.Core {
    public class ReliableGameServerClientTests : GameServerClientTests<
                ReliableNetworkingServer, ReliableNetworkingClient, ReliableGameServer<ReliableServerPlayer>, ReliableGameClient<ReliableClientPlayer>,
                ReliableServerPlayer, ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient, ReliableClientAcceptor<ReliableServerPlayer>,
                ReliableGameServerClientTests.ServerListener, ReliableGameServerClientTests.ClientListener
            > {

        protected override ReliableNetworkingClient NewClient() => new ReliableNetworkingClient(new ReliableSocket(new ReliableSocketMock()));
        protected override ReliableNetworkingServer NewServer() => new ReliableNetworkingServer(new ReliableSocket(new ReliableSocketMock()));

        protected override void NewServer(out ReliableGameServer<ReliableServerPlayer> server, out ServerListener listener) {
            var newListener = new ServerListener();
            server = new ReliableGameServer<ReliableServerPlayer>(this.NewServer(), new MainThreadDispatcher()) {
                listener = newListener
            };
            listener = newListener;
        }

        protected override void NewClient(out ReliableGameClient<ReliableClientPlayer> client, out ClientListener listener) {
            var newListener = new ClientListener();
            client = new ReliableGameClient<ReliableClientPlayer>(this.NewClient(), new MainThreadDispatcher()) {
                listener = newListener
            };
            listener = newListener;
        }

        [Test]
        public void TestRealSocketConnection() {
            Logger.IsLoggingEnabled = true;

            var mainThreadDispatcher = new MainThreadDispatcher();

            ServerListener serverListener = new ServerListener();
            ClientListener clientListener = new ClientListener();

            var server = new ReliableGameServer<ReliableServerPlayer>(new ReliableNetworkingServer(new ReliableSocket(new TCPNonBlockingSocket())), mainThreadDispatcher) { listener = serverListener };
            var client = new ReliableGameClient<ReliableClientPlayer>(new ReliableNetworkingClient(new ReliableSocket(new TCPNonBlockingSocket())), mainThreadDispatcher) { listener = clientListener };

            void Update() {
                this.Update(server);
                this.Update(server);
                this.Update(client);
                this.Update(client);
            }

            var localIP = "127.0.0.1";

            server.Start(localIP, 64000);
            client.Connect(localIP, 64000);

            Update();
            Update();

            Assert.AreEqual(1, serverListener.connectedPlayers.Count);
            Assert.IsTrue(clientListener.connectedCalled);

            void ValidateProcessTiming() {
                Update();
                Update();

                var pingValue = server.pingController.GetPingValue(client.FindPlayer(p => p.isLocalPlayer));
                Logger.Log($"Current ping value: {pingValue}");
                Assert.Less(pingValue, .3F);
            }

            var sleepMillis = 10;
            var loopCount = 1000;
            Logger.Log($"Will take {sleepMillis * loopCount / 1000} seconds to finish.");
            for (int index = 0; index < loopCount; index++) {
                Thread.Sleep(sleepMillis);

                ValidateProcessTiming();

                Logger.Log($"Current at index: {index}");
            }
        }

        public class ClientListener : IClientListener<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient> {
            public List<MessageContainer> receivedMessages { get; } = new List<MessageContainer>();
            public List<ReliableClientPlayer> disconnectedPlayers { get; } = new List<ReliableClientPlayer>();
            public bool connectedCalled { get; private set; }
            public bool connectTimeoutCalled { get; private set; }
            public bool disconnectCalled { get; private set; }
            public ReliableClientPlayer localPlayer { get; private set; }

            #region IGameClientListener

            void IGameClient<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.GameClientDidConnect() => this.connectedCalled = true;
            void IGameClient<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.GameClientConnectDidTimeout() => this.connectTimeoutCalled = true;
            void IGameClient<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.GameClientDidDisconnect() => this.disconnectCalled = true;
            void IGameClient<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.GameClientDidIdentifyLocalPlayer(ReliableClientPlayer player) => this.localPlayer = player;
            void IGameClient<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.GameClientDidReceiveMessage(MessageContainer container) => this.receivedMessages.Add(container);
            void IGameClient<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.GameClientNetworkPlayerDidDisconnect(ReliableClientPlayer player) => this.disconnectedPlayers.Add(player);

            #endregion
        }

        public class ServerListener : IServerListener<ReliableServerPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient> {
            public List<ReliableServerPlayer> connectedPlayers { get; } = new List<ReliableServerPlayer>();
            public List<ReliableServerPlayer> disconnectedPlayers { get; } = new List<ReliableServerPlayer>();

            #region IGameServerListener

            void IGameServer<ReliableServerPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.GameServerPlayerDidConnect(ReliableServerPlayer player) => connectedPlayers.Add(player);
            void IGameServer<ReliableServerPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.GameServerPlayerDidDisconnect(ReliableServerPlayer player) => disconnectedPlayers.Add(player);
            void IGameServer<ReliableServerPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.GameServerDidReceiveClientMessage(MessageContainer container, ReliableServerPlayer player) => Assert.NotNull(player);

            #endregion
        }
    }
}