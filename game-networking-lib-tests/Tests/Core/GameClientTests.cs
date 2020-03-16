using NUnit.Framework;
using GameNetworking;
using Networking;
using Tests.Core.Model;
using Messages.Models;
using System.Collections.Generic;
using GameNetworking.Commons;
using System;

using ClientPlayer = GameNetworking.Models.Client.NetworkPlayer;
using ServerPlayer = GameNetworking.Models.Server.NetworkPlayer;

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

        private void New(out GameServer<ServerPlayer> server, out ServerListener listener) {
            listener = new ServerListener();
            server = new GameServer<ServerPlayer>(this.New(), new MainThreadDispatcher()) {
                listener = listener
            };
        }

        private void New(out GameClient<ClientPlayer> client, out ClientListener listener) {
            listener = new ClientListener();
            client = new GameClient<ClientPlayer>(this.New(), new MainThreadDispatcher()) {
                listener = listener
            };
        }

        private void Update(GameServer<ServerPlayer> server) {
            server.Update();
        }

        private void Update(GameClient<ClientPlayer> client) {
            client.Update();
        }

        [Test]
        public void TestConnectDisconnect() {
            this.New(out GameClient<ClientPlayer> client, out ClientListener clientListener);
            this.New(out GameServer<ServerPlayer> server, out ServerListener serverListener);

            server.Listen(this.port);

            client.Connect(this.host, this.port);

            this.Update(server);
            this.Update(client);

            Assert.IsTrue(clientListener.connectedCalled);

            Assert.IsNotNull(clientListener.localPlayer);
            var player = client.FindPlayer(player => player.isLocalPlayer);
            Assert.AreEqual(player.playerId, serverListener.connectedPlayers[0].playerId);

            var playerId = player.playerId;
            var serverPlayer = server.FindPlayer(playerId);

            Assert.IsNotNull(serverPlayer);

            client.Disconnect();

            this.Update(client);
            this.Update(server);

            var notServerPlayer = server.FindPlayer(playerId);

            Assert.IsTrue(clientListener.disconnectCalled);
            Assert.IsNull(notServerPlayer);

            Assert.AreEqual(player.playerId, serverListener.disconnectedPlayers[0].playerId);

            Assert.AreEqual(0, clientListener.disconnectedPlayers.Count);
        }

        [Test]
        public void TestMultiPlayerConnectDisconnect() {
            this.New(out GameClient<ClientPlayer> client1, out ClientListener clientListener1);
            this.New(out GameClient<ClientPlayer> client2, out ClientListener clientListener2);
            this.New(out GameClient<ClientPlayer> client3, out ClientListener clientListener3);
            this.New(out GameServer<ServerPlayer> server, out ServerListener serverListener);

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

            Assert.IsNotNull(clientListener1.localPlayer);
            Assert.IsNotNull(clientListener2.localPlayer);
            Assert.IsNotNull(clientListener3.localPlayer);

            var player1 = client1.FindPlayer(player => player.isLocalPlayer);
            var player2 = client2.FindPlayer(player => player.isLocalPlayer);
            var player3 = client3.FindPlayer(player => player.isLocalPlayer);

            Assert.AreEqual(player1.playerId, serverListener.connectedPlayers[0].playerId);
            Assert.AreEqual(player2.playerId, serverListener.connectedPlayers[1].playerId);
            Assert.AreEqual(player3.playerId, serverListener.connectedPlayers[2].playerId);

            var playerId1 = player1.playerId;
            var playerId2 = player2.playerId;
            var playerId3 = player3.playerId;

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

            Assert.AreEqual(player3.playerId, clientListener1.disconnectedPlayers[0].playerId);
            Assert.AreEqual(player3.playerId, clientListener2.disconnectedPlayers[0].playerId);

            Assert.AreNotEqual(player1.playerId, player2.playerId);
            Assert.AreNotEqual(player1.playerId, player3.playerId);
            Assert.AreNotEqual(player2.playerId, player3.playerId);

            Assert.AreEqual(1, serverListener.disconnectedPlayers.Count);

            Assert.AreEqual(1, client1.AllPlayers().FindAll(p => p.isLocalPlayer).Count);
            Assert.AreEqual(1, client2.AllPlayers().FindAll(p => p.isLocalPlayer).Count);
            Assert.AreEqual(1, client3.AllPlayers().FindAll(p => p.isLocalPlayer).Count);
        }

        [Test]
        public void TestClientReconnect() {
            this.New(out GameClient<ClientPlayer> client1, out ClientListener listener1_c);
            this.New(out GameClient<ClientPlayer> client2, out _);
            this.New(out GameServer<ServerPlayer> server, out _);

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

            var player2 = client2.FindPlayer(player => player.isLocalPlayer);
            var disconnectedPlayerId = player2.playerId;

            client2.Disconnect();

            Update();

            Assert.IsNull(server.FindPlayer(player2.playerId));
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
            this.New(out GameClient<ClientPlayer> client1, out _);
            this.New(out GameClient<ClientPlayer> client2, out _);
            this.New(out GameServer<ServerPlayer> server, out _);

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

            var player1 = client1.FindPlayer(player => player.isLocalPlayer);
            var player2 = client2.FindPlayer(player => player.isLocalPlayer);

            var serverPlayer1 = server.FindPlayer(player1.playerId);
            var serverPlayer2 = server.FindPlayer(player2.playerId);
            var serverPing1 = serverPlayer1.mostRecentPingValue;
            var serverPing2 = serverPlayer2.mostRecentPingValue;

            Assert.AreEqual(serverPing1, player1.mostRecentPingValue);
            Assert.AreEqual(serverPing2, player2.mostRecentPingValue);

            var client1client2Ping = client1.GetPing(player2.playerId);
            var client2client1Ping = client2.GetPing(player1.playerId);

            Assert.AreEqual(player1.mostRecentPingValue, client2client1Ping);
            Assert.AreEqual(player2.mostRecentPingValue, client1client2Ping);
        }
    }

    class ClientListener : GameClient<ClientPlayer>.IListener {
        public readonly List<MessageContainer> receivedMessages = new List<MessageContainer>();
        public readonly List<ClientPlayer> disconnectedPlayers = new List<ClientPlayer>();
        public bool connectedCalled { get; private set; }
        public bool connectTimeoutCalled { get; private set; }
        public bool disconnectCalled { get; private set; }
        public ClientPlayer localPlayer { get; private set; }

        #region IGameClientListener

        void GameClient<ClientPlayer>.IListener.GameClientDidConnect() {
            this.connectedCalled = true;
        }

        void GameClient<ClientPlayer>.IListener.GameClientConnectDidTimeout() {
            this.connectTimeoutCalled = true;
        }

        void GameClient<ClientPlayer>.IListener.GameClientDidDisconnect() {
            this.disconnectCalled = true;
        }

        void GameClient<ClientPlayer>.IListener.GameClientDidIdentifyLocalPlayer(ClientPlayer player) {
            this.localPlayer = player;
        }

        void GameClient<ClientPlayer>.IListener.GameClientDidReceiveMessage(MessageContainer container) {
            this.receivedMessages.Add(container);
        }

        void GameClient<ClientPlayer>.IListener.GameClientNetworkPlayerDidDisconnect(ClientPlayer player) {
            this.disconnectedPlayers.Add(player);
        }

        #endregion
    }

    class ServerListener : GameServer<ServerPlayer>.IListener {
        public readonly List<GameNetworking.Models.Server.NetworkPlayer> connectedPlayers = new List<GameNetworking.Models.Server.NetworkPlayer>();
        public readonly List<GameNetworking.Models.Server.NetworkPlayer> disconnectedPlayers = new List<GameNetworking.Models.Server.NetworkPlayer>();

        #region IGameServerListener

        void GameServer<ServerPlayer>.IListener.GameServerPlayerDidConnect(GameNetworking.Models.Server.NetworkPlayer player) {
            connectedPlayers.Add(player);
        }

        void GameServer<ServerPlayer>.IListener.GameServerPlayerDidDisconnect(GameNetworking.Models.Server.NetworkPlayer player) {
            disconnectedPlayers.Add(player);
        }

        void GameServer<ServerPlayer>.IListener.GameServerDidReceiveClientMessage(MessageContainer container, GameNetworking.Models.Server.NetworkPlayer player) {
            Assert.NotNull(player);
        }

        #endregion
    }

    class MainThreadDispatcher : IMainThreadDispatcher {
        public void Enqueue(Action action) {
            action.Invoke();
        }
    }
}
