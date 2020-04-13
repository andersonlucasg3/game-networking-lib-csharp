using System.Collections.Generic;
using System.Threading;
using GameNetworking;
using GameNetworking.Commons.Client;
using GameNetworking.Messages;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
using Messages.Models;
using Networking.Models;
using Networking.Sockets;
using NUnit.Framework;
using Test.Core.Model;

using UnreliableClientPlayer = GameNetworking.Commons.Models.Client.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;
using UnreliableServerPlayer = GameNetworking.Commons.Models.Server.NetworkPlayer<Networking.Sockets.IUDPSocket, GameNetworking.Networking.Models.UnreliableNetworkClient, Networking.Models.UnreliableNetClient>;

namespace Tests.Core {
    public class UnreliableGameServerClientTests : GameServerClientTests<
            UnreliableNetworkingServer, UnreliableNetworkingClient, UnreliableGameServer<UnreliableServerPlayer>, UnreliableGameClient<UnreliableClientPlayer>,
            UnreliableServerPlayer, UnreliableClientPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient, UnreliableClientAcceptor<UnreliableServerPlayer>,
            UnreliableGameServerClientTests.ServerListener, UnreliableGameServerClientTests.ClientListener
        > {

        private static int ipCounter = 0;

        protected override UnreliableNetworkingClient NewClient() => new UnreliableNetworkingClient(new UnreliableSocket(new UnreliableSocketMock()));
        protected override UnreliableNetworkingServer NewServer() => new UnreliableNetworkingServer(new UnreliableSocket(new UnreliableSocketMock()));

        protected override void NewServer(out UnreliableGameServer<UnreliableServerPlayer> server, out ServerListener listener) {
            NewServer(out server, out listener, out _);
        }

        protected override void NewServer(out UnreliableGameServer<UnreliableServerPlayer> server, out ServerListener listener, out UnreliableNetworkingServer networkingServer) {
            var newListener = new ServerListener();
            networkingServer = this.NewServer();
            server = new UnreliableGameServer<UnreliableServerPlayer>(networkingServer, new MainThreadDispatcher()) { listener = newListener };
            listener = newListener;
        }

        protected override void NewClient(out UnreliableGameClient<UnreliableClientPlayer> client, out ClientListener listener) {
            var newListener = new ClientListener();
            client = new UnreliableGameClient<UnreliableClientPlayer>(this.NewClient(), new MainThreadDispatcher()) { listener = newListener };
            client.Start($"192.168.0.{ipCounter}", 5);
            ipCounter++;
            listener = newListener;
        }

        [Test]
        public void TestClientConnectionController() {
            var sender = new MessageSender();

            var timeOutCalled = false;
            var conn = new UnreliableClientConnectionController(sender, () => timeOutCalled = true) { secondsBetweenRetries = .1f };

            conn.Connect();

            Assert.AreEqual(MessageType.connect, (MessageType)sender.sentMessage.type);

            sender.sentMessage = null;

            var sleepTime = (int)(conn.secondsBetweenRetries * 1000);

            void Update() {
                Thread.Sleep(sleepTime);

                conn.Update();

                if (!timeOutCalled) {
                    Assert.AreEqual(MessageType.connect, (MessageType)sender.sentMessage.type);
                    sender.sentMessage = null;
                }
            }

            Update();
            Update();
            Update();
            Update();

            Assert.IsTrue(timeOutCalled);

            timeOutCalled = false;

            conn.Connect();

            conn.Update();

            conn.Stop();

            conn.Update();
            conn.Update();
            conn.Update();
            conn.Update();

            Assert.IsFalse(timeOutCalled);
        }

        public class ClientListener : IClientListener<UnreliableClientPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient> {
            public List<MessageContainer> receivedMessages { get; } = new List<MessageContainer>();
            public List<UnreliableClientPlayer> disconnectedPlayers { get; } = new List<UnreliableClientPlayer>();
            public bool connectedCalled { get; private set; }
            public bool connectTimeoutCalled { get; private set; }
            public bool disconnectCalled { get; private set; }
            public UnreliableClientPlayer localPlayer { get; private set; }

            #region IGameClientListener

            public void GameClientDidConnect() => this.connectedCalled = true;
            public void GameClientConnectDidTimeout() => this.connectTimeoutCalled = true;
            public void GameClientDidDisconnect() => this.disconnectCalled = true;
            public void GameClientDidIdentifyLocalPlayer(UnreliableClientPlayer player) => this.localPlayer = player;
            public void GameClientDidReceiveMessage(MessageContainer container) => this.receivedMessages.Add(container);
            public void GameClientNetworkPlayerDidDisconnect(UnreliableClientPlayer player) => this.disconnectedPlayers.Add(player);

            #endregion
        }

        public class ServerListener : IServerListener<UnreliableServerPlayer, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient> {
            public List<UnreliableServerPlayer> connectedPlayers { get; } = new List<UnreliableServerPlayer>();
            public List<UnreliableServerPlayer> disconnectedPlayers { get; } = new List<UnreliableServerPlayer>();

            #region IGameServerListener

            public void GameServerPlayerDidConnect(UnreliableServerPlayer player) => connectedPlayers.Add(player);
            public void GameServerPlayerDidDisconnect(UnreliableServerPlayer player) => disconnectedPlayers.Add(player);
            public void GameServerDidReceiveClientMessage(MessageContainer container, UnreliableServerPlayer player) => Assert.NotNull(player);

            #endregion
        }

        public class MessageSender : IGameClientMessageSender {
            public ITypedMessage sentMessage = null;

            public void Send(ITypedMessage message) {
                this.sentMessage = message;
            }
        }
    }
}