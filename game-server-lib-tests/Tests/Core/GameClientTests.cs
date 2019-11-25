using NUnit.Framework;
using GameNetworking;
using Networking;
using Tests.Core.Model;
using Messages.Models;
using UnityEngine;
using System.Collections.Generic;
using GameNetworking.Models.Client;
using GameNetworking.Messages.Client;

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

        [Test]
        public void TestConnectDisconnect() {
            this.New(out GameClient client, out ClientListener clientListener);
            this.New(out GameServer server, out ServerListener serverListener);

            server.Listen(this.port);

            client.Connect(this.host, this.port);

            this.Update(server);
            this.Update(client);

            Assert.IsTrue(clientListener.connectedCalled);

            var playerId = client.Player.PlayerId;
            var serverPlayer = server.FindPlayer(playerId);

            Assert.IsNotNull(client.Player);
            Assert.IsNotNull(serverPlayer);

            client.Disconnect();

            this.Update(client);
            this.Update(server);

            var notServerPlayer = server.FindPlayer(playerId);

            Assert.IsTrue(clientListener.disconnectCalled);
            Assert.IsNull(notServerPlayer);

            Assert.AreEqual(client.Player.PlayerId, serverListener.disconnectedPlayers[0].PlayerId);
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

            client.Send(new SpawnRequestMessage { spawnObjectId = spawnId });

            this.Update(server);
            this.Update(client);

            Assert.IsTrue(serverListener.spawnObjectIds.Contains(spawnId));
            Assert.AreEqual(1, serverListener.spawnObjectIds.Count);
            Assert.IsNotNull(server.FindPlayer(client.Player.PlayerId).gameObject);

            this.Update(server);
            this.Update(client);

            Assert.IsTrue(clientListener.spawnObjectIds.Contains(spawnId));
            Assert.AreEqual(1, clientListener.spawnObjectIds.Count);
            Assert.IsNotNull(client.Player.gameObject);
        }
    }

    class ClientListener : IGameClientListener, IGameClientInstanceListener {
        public readonly Queue<MessageContainer> receivedMessages = new Queue<MessageContainer>();
        public readonly List<int> spawnObjectIds = new List<int>();
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
            this.receivedMessages.Enqueue(container);
        }

        GameObject IGameClientListener.GameClientSpawnCharacter(NetworkPlayer player) {
            this.spawnObjectIds.Add(player.SpawnId);
            return new GameObject();
        }

        void IGameClientListener.GameClientNetworkPlayerDidDisconnect(NetworkPlayer player) {

        }

        bool IGameClientInstanceListener.GameInstanceSyncPlayer(NetworkPlayer player, Vector3 position, Vector3 eulerAngles) {
            return false;
        }

        #endregion
    }

    class ServerListener : IGameServerListener {
        public readonly List<GameNetworking.Models.Server.NetworkPlayer> disconnectedPlayers = new List<GameNetworking.Models.Server.NetworkPlayer>();
        public readonly List<int> spawnObjectIds = new List<int>();

        #region IGameServerListener

        void IGameServerListener.GameServerPlayerDidDisconnect(GameNetworking.Models.Server.NetworkPlayer player) {
            this.disconnectedPlayers.Add(player);
        }

        GameObject IGameServerListener.GameServerSpawnCharacter(GameNetworking.Models.Server.NetworkPlayer player) {
            this.spawnObjectIds.Add(player.SpawnId);
            return new GameObject();
        }

        void IGameServerListener.GameServerDidReceiveClientMessage(MessageContainer container, GameNetworking.Models.Server.NetworkPlayer player) {

        }

        #endregion
    }
}