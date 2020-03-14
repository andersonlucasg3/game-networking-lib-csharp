using NUnit.Framework;
using GameNetworking;
using Networking;
using Tests.Core.Model;
using Messages.Models;
using System.Collections.Generic;
using GameNetworking.Models.Client;
using GameNetworking.Commons;
using System;

namespace Tests.Core {
    public class GameServerClientTests {
        private readonly string host = "127.0.0.1";
        private readonly int port = 30000;

        private INetworking New() {
            return new NetSocket(new SocketMock {
                blocking = false,
                noDelay = true
            });
        }

        private void New(out GameServer server, out ServerListener listener) {
            listener = new ServerListener();
            server = new GameServer(this.New(), new MainThreadDispatcher()) {
                listener = listener
            };
        }

        private void New(out GameClient client, out ClientListener listener) {
            listener = new ClientListener();
            client = new GameClient(this.New(), new MainThreadDispatcher()) {
                listener = listener
            };
        }

        private void Update(GameServer server) {
            server.Update();
        }

        private void Update(GameClient client) {
            client.Update();
        }

        [Test]
        public void TestConnectDisconnect() {
            this.New(out GameClient client, out ClientListener clientListener);
            this.New(out GameServer server, out ServerListener serverListener);

            server.Listen(this.port);

            client.Connect(this.host, this.port);

            this.Update(server);
            this.Update(client);

            Assert.IsTrue(clientListener.connectedCalled);

            Assert.AreEqual(client.player.playerId, serverListener.connectedPlayers[0].playerId);

            var playerId = client.player.playerId;
            var serverPlayer = server.FindPlayer(playerId);

            Assert.IsNotNull(serverPlayer);

            client.Disconnect();

            this.Update(client);
            this.Update(server);

            var notServerPlayer = server.FindPlayer(playerId);

            Assert.IsTrue(clientListener.disconnectCalled);
            Assert.IsNull(notServerPlayer);

            Assert.AreEqual(client.player.playerId, serverListener.disconnectedPlayers[0].playerId);

            Assert.AreEqual(0, clientListener.disconnectedPlayers.Count);
        }

        [Test]
        public void TestMultiPlayerConnectDisconnect() {
            this.New(out GameClient client1, out ClientListener clientListener1);
            this.New(out GameClient client2, out ClientListener clientListener2);
            this.New(out GameClient client3, out ClientListener clientListener3);
            this.New(out GameServer server, out ServerListener serverListener);

            void UpdateAction() {
                this.Update(server);
                this.Update(server);
                this.Update(server);
                this.Update(client1);
                this.Update(client2);
                this.Update(client3);
            }

            server.Listen(this.port);

            client1.Connect(this.host, this.port);
            client2.Connect(this.host, this.port);
            client3.Connect(this.host, this.port);

            UpdateAction();

            Assert.IsTrue(clientListener1.connectedCalled);
            Assert.IsTrue(clientListener2.connectedCalled);
            Assert.IsTrue(clientListener3.connectedCalled);
            Assert.AreEqual(client1.player.playerId, serverListener.connectedPlayers[0].playerId);
            Assert.AreEqual(client2.player.playerId, serverListener.connectedPlayers[1].playerId);
            Assert.AreEqual(client3.player.playerId, serverListener.connectedPlayers[2].playerId);

            var playerId1 = client1.player.playerId;
            var playerId2 = client2.player.playerId;
            var playerId3 = client3.player.playerId;

            var serverPlayer1 = server.FindPlayer(playerId1);
            var serverPlayer2 = server.FindPlayer(playerId2);
            var serverPlayer3 = server.FindPlayer(playerId3);

            Assert.IsNotNull(serverPlayer1);
            Assert.IsNotNull(serverPlayer2);
            Assert.IsNotNull(serverPlayer3);

            client3.Disconnect();

            UpdateAction();

            Assert.IsNotNull(server.FindPlayer(playerId1));
            Assert.IsNotNull(server.FindPlayer(playerId2));
            Assert.IsNull(server.FindPlayer(playerId3));

            Assert.IsFalse(clientListener1.disconnectCalled);
            Assert.IsFalse(clientListener2.disconnectCalled);
            Assert.IsTrue(clientListener3.disconnectCalled);

            Assert.AreEqual(1, clientListener1.disconnectedPlayers.Count);
            Assert.AreEqual(1, clientListener2.disconnectedPlayers.Count);
            Assert.AreEqual(0, clientListener3.disconnectedPlayers.Count);

            Assert.AreEqual(client3.player.playerId, clientListener1.disconnectedPlayers[0].playerId);
            Assert.AreEqual(client3.player.playerId, clientListener2.disconnectedPlayers[0].playerId);

            Assert.AreNotEqual(client1.player.playerId, client2.player.playerId);
            Assert.AreNotEqual(client1.player.playerId, client3.player.playerId);
            Assert.AreNotEqual(client2.player.playerId, client3.player.playerId);

            Assert.AreEqual(1, serverListener.disconnectedPlayers.Count);
        }

        [Test]
        public void TestClientReconnect() {
            this.New(out GameClient client1, out ClientListener listener1_c);
            this.New(out GameClient client2, out _);
            this.New(out GameServer server, out _);

            void Update() {
                this.Update(server);
                this.Update(server);
                this.Update(client1);
                this.Update(client2);
            }

            server.Listen(this.port);

            client1.Connect(this.host, this.port);
            client2.Connect(this.host, this.port);

            Update();

            Update();

            var disconnectedPlayerId = client2.player.playerId;

            client2.Disconnect();

            Update();

            Assert.IsNull(server.FindPlayer(client2.player.playerId));
            ClientListener listener2_c;
            this.New(out client2, out listener2_c);

            client2.Connect(this.host, this.port);

            Update();

            Update();

            Assert.AreEqual(1, listener1_c.disconnectedPlayers.Count);
            Assert.AreEqual(0, listener2_c.disconnectedPlayers.Count);

            Assert.AreEqual(disconnectedPlayerId, listener1_c.disconnectedPlayers[0].playerId);
        }

        [Test]
        public void TestClientPingBroadcast() {
            this.New(out GameClient client1, out _);
            this.New(out GameClient client2, out _);
            this.New(out GameServer server, out _);

            void Update() {
                this.Update(server);
                this.Update(server);
                this.Update(client1);
                this.Update(client2);
            }

            server.Listen(this.port);

            client1.Connect(this.host, this.port);
            client2.Connect(this.host, this.port);

            Update();
            Update();
            Update();
            Update();
            Update();

            var serverPlayer1 = server.FindPlayer(client1.player.playerId);
            var serverPlayer2 = server.FindPlayer(client2.player.playerId);
            var serverPing1 = serverPlayer1.mostRecentPingValue;
            var serverPing2 = serverPlayer2.mostRecentPingValue;

            Assert.AreEqual(serverPing1, client1.player.mostRecentPingValue);
            Assert.AreEqual(serverPing2, client2.player.mostRecentPingValue);

            var client1client2Ping = client1.GetPing(client2.player.playerId);
            var client2client1Ping = client2.GetPing(client1.player.playerId);

            Assert.AreEqual(client1.player.mostRecentPingValue, client2client1Ping);
            Assert.AreEqual(client2.player.mostRecentPingValue, client1client2Ping);
        }
    }

    class ClientListener : IGameClientListener {
        public readonly List<MessageContainer> receivedMessages = new List<MessageContainer>();
        public readonly List<NetworkPlayer> disconnectedPlayers = new List<NetworkPlayer>();
        public bool connectedCalled { get; private set; }
        public bool connectTimeoutCalled { get; private set; }
        public bool disconnectCalled { get; private set; }

        #region IGameClientListener

        void IGameClientListener.GameClientDidConnect() {
            this.connectedCalled = true;
        }

        void IGameClientListener.GameClientConnectDidTimeout() {
            this.connectTimeoutCalled = true;
        }

        void IGameClientListener.GameClientDidDisconnect() {
            this.disconnectCalled = true;
        }

        void IGameClientListener.GameClientDidReceiveMessage(MessageContainer container) {
            this.receivedMessages.Add(container);
        }

        void IGameClientListener.GameClientNetworkPlayerDidDisconnect(NetworkPlayer player) {
            this.disconnectedPlayers.Add(player);
        }

        #endregion
    }

    class ServerListener : IGameServerListener {
        public readonly List<GameNetworking.Models.Server.NetworkPlayer> connectedPlayers = new List<GameNetworking.Models.Server.NetworkPlayer>();
        public readonly List<GameNetworking.Models.Server.NetworkPlayer> disconnectedPlayers = new List<GameNetworking.Models.Server.NetworkPlayer>();

        #region IGameServerListener

        void IGameServerListener.GameServerPlayerDidConnect(GameNetworking.Models.Server.NetworkPlayer player) {
            connectedPlayers.Add(player);
        }

        void IGameServerListener.GameServerPlayerDidDisconnect(GameNetworking.Models.Server.NetworkPlayer player) {
            disconnectedPlayers.Add(player);
        }

        void IGameServerListener.GameServerDidReceiveClientMessage(MessageContainer container, GameNetworking.Models.Server.NetworkPlayer player) {

        }

        #endregion
    }

    class MainThreadDispatcher : IMainThreadDispatcher {
        public void Enqueue(Action action) {
            action.Invoke();
        }
    }
}