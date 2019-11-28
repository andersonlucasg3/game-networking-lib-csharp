using NUnit.Framework;
using GameNetworking;
using Networking;
using Tests.Core.Model;
using Messages.Models;
using UnityEngine;
using System.Collections.Generic;
using GameNetworking.Models.Client;
using GameNetworking.Messages.Client;
using GameNetworking.Messages;

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
            server = new GameServer(this.New()) {
                listener = listener
            };
        }

        private void New(out GameClient client, out ClientListener listener) {
            listener = new ClientListener();
            client = new GameClient(this.New()) {
                listener = listener,
                instanceListener = listener
            };
        }

        private void MainThreadRun() {
            if (UnityMainThreadDispatcher.instance == null) {
                UnityMainThreadDispatcher instance = new UnityMainThreadDispatcher();
                instance.Awake();
            }
            UnityMainThreadDispatcher.instance.Update();
        }

        private void Update(GameServer server) {
            server.Update();
            this.MainThreadRun();
        }

        private void Update(GameClient client) {
            client.Update();
            this.MainThreadRun();
        }

        [SetUp]
        public void SetUp() {
            TimeHelp.ResetTime();
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
        public void TestSpawnObject() {
            this.New(out GameClient client, out ClientListener clientListener);
            this.New(out GameServer server, out ServerListener serverListener);

            server.Listen(this.port);

            client.Connect(this.host, this.port);

            this.Update(server);
            this.Update(client);

            var spawnId = 1024;

            var playerId = client.player.playerId;

            client.Send(new SpawnRequestMessage { spawnObjectId = spawnId });

            this.Update(server);
            this.Update(client);

            Assert.IsTrue(serverListener.spawnObjectIds.ContainsKey(playerId));
            Assert.AreEqual(spawnId, serverListener.spawnObjectIds[playerId]);
            Assert.AreEqual(1, serverListener.spawnObjectIds.Count);
            Assert.IsNotNull(server.FindPlayer(client.player.playerId).gameObject);

            this.Update(server);
            this.Update(client);

            Assert.IsTrue(clientListener.spawnObjectIds.ContainsKey(playerId));
            Assert.AreEqual(spawnId, clientListener.spawnObjectIds[playerId]);
            Assert.AreEqual(1, clientListener.spawnObjectIds.Count);
            Assert.IsNotNull(client.player.gameObject);
        }

        [Test]
        public void TestMultiPlayerSpawnObject() {
            this.New(out GameClient client1, out ClientListener listener1_c);
            this.New(out GameClient client2, out ClientListener listener2_c);
            this.New(out GameClient client3, out ClientListener listener3_c);
            this.New(out GameServer server, out ServerListener listener_s);

            void Update() {
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

            Update();

            var spawnId1 = 1234;
            var spawnId2 = 4321;
            var spawnId3 = 6169;

            var playerId1 = client1.player.playerId;
            var playerId2 = client2.player.playerId;
            var playerId3 = client3.player.playerId;

            client1.Send(new SpawnRequestMessage { spawnObjectId = spawnId1 });
            client2.Send(new SpawnRequestMessage { spawnObjectId = spawnId2 });
            client3.Send(new SpawnRequestMessage { spawnObjectId = spawnId3 });

            Update();

            Assert.IsNotNull(client1.player.gameObject);
            Assert.IsNotNull(client2.player.gameObject);
            Assert.IsNotNull(client3.player.gameObject);

            Assert.AreEqual(spawnId1, client1.player.spawnId);
            Assert.AreEqual(spawnId2, client2.player.spawnId);
            Assert.AreEqual(spawnId3, client3.player.spawnId);

            void assertComplex(int pId, int sId) {
                Assert.AreEqual(sId, listener1_c.spawnObjectIds[pId]);
                Assert.AreEqual(sId, listener2_c.spawnObjectIds[pId]);
                Assert.AreEqual(sId, listener3_c.spawnObjectIds[pId]);
            }

            assertComplex(playerId1, spawnId1);
            assertComplex(playerId2, spawnId2);
            assertComplex(playerId3, spawnId3);

            Assert.AreEqual(3, listener1_c.spawnObjectIds.Count);
            Assert.AreEqual(3, listener2_c.spawnObjectIds.Count);
            Assert.AreEqual(3, listener3_c.spawnObjectIds.Count);

            Assert.AreEqual(spawnId1, listener_s.spawnObjectIds[playerId1]);
            Assert.AreEqual(spawnId2, listener_s.spawnObjectIds[playerId2]);
            Assert.AreEqual(spawnId3, listener_s.spawnObjectIds[playerId3]);

            Assert.AreEqual(3, listener_s.spawnObjectIds.Count);
        }

        [Test]
        public void TestClientReconnect() {
            this.New(out GameClient client1, out ClientListener listener1_c);
            this.New(out GameClient client2, out ClientListener listener2_c);
            this.New(out GameServer server, out ServerListener listener_s);

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

            var spawnId = 1;

            client1.Send(new SpawnRequestMessage { spawnObjectId = spawnId });
            client2.Send(new SpawnRequestMessage { spawnObjectId = spawnId });

            Update();

            var disconnectedPlayerId = client2.player.playerId;

            client2.Disconnect();

            Update();

            Assert.IsNull(server.FindPlayer(client2.player.playerId));

            this.New(out client2, out listener2_c);

            client2.Connect(this.host, this.port);

            Update();

            var newSpawnId = 2;

            client2.Send(new SpawnRequestMessage { spawnObjectId = newSpawnId });

            Update();

            Assert.AreEqual(spawnId, client1.player.spawnId);
            Assert.AreEqual(newSpawnId, client2.player.spawnId);

            Assert.AreEqual(2, listener1_c.spawnObjectIds.Count);
            Assert.AreEqual(2, listener2_c.spawnObjectIds.Count);

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
                Time.time += 1F;
            }

            server.Listen(this.port);

            client1.Connect(this.host, this.port);
            client2.Connect(this.host, this.port);

            Update();

            var spawnId = 1;

            client1.Send(new SpawnRequestMessage { spawnObjectId = spawnId });
            client2.Send(new SpawnRequestMessage { spawnObjectId = spawnId });

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

    class ClientListener : IGameClientListener, IGameClientInstanceListener {
        public readonly List<MessageContainer> receivedMessages = new List<MessageContainer>();
        public readonly List<NetworkPlayer> disconnectedPlayers = new List<NetworkPlayer>();
        public readonly Dictionary<int, int> spawnObjectIds = new Dictionary<int, int>();
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

        GameObject IGameClientListener.GameClientSpawnCharacter(NetworkPlayer player) {
            this.spawnObjectIds[player.playerId] = player.spawnId;
            return new GameObject();
        }

        void IGameClientListener.GameClientNetworkPlayerDidDisconnect(NetworkPlayer player) {
            this.disconnectedPlayers.Add(player);
            this.spawnObjectIds.Remove(player.playerId);
        }

        bool IGameClientInstanceListener.GameInstanceSyncPlayer(NetworkPlayer player, Vector3 position, Vector3 eulerAngles) {
            return false;
        }

        #endregion
    }

    class ServerListener : IGameServerListener {
        public readonly List<GameNetworking.Models.Server.NetworkPlayer> disconnectedPlayers = new List<GameNetworking.Models.Server.NetworkPlayer>();
        public readonly Dictionary<int, int> spawnObjectIds = new Dictionary<int, int>();

        #region IGameServerListener

        void IGameServerListener.GameServerPlayerDidDisconnect(GameNetworking.Models.Server.NetworkPlayer player) {
            this.disconnectedPlayers.Add(player);
            this.spawnObjectIds.Remove(player.playerId);
        }

        GameObject IGameServerListener.GameServerSpawnCharacter(GameNetworking.Models.Server.NetworkPlayer player) {
            this.spawnObjectIds[player.playerId] = player.spawnId;
            return new GameObject();
        }

        void IGameServerListener.GameServerDidReceiveClientMessage(MessageContainer container, GameNetworking.Models.Server.NetworkPlayer player) {

        }

        #endregion
    }
}