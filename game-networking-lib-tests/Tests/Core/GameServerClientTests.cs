#if !UNITY_64

using NUnit.Framework;
using System.Collections.Generic;
using GameNetworking.Commons;
using System;
using System.Threading;
using System.Linq;
using GameNetworking.Messages.Models;
using GameNetworking.Client;
using GameNetworking.Networking;
using GameNetworking.Sockets;
using GameNetworking.Server;

using ServerPlayer = GameNetworking.Server.Player;
using ClientPlayer = GameNetworking.Client.Player;
using GameNetworking.Commons.Client;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;

namespace Tests.Core {
    public interface IClientListener : IGameClientListener<ClientPlayer> {
        List<MessageContainer> receivedMessages { get; }
        List<ClientPlayer> disconnectedPlayers { get; }
        bool connectedCalled { get; }
        bool connectTimeoutCalled { get; }
        bool disconnectCalled { get; }
        ClientPlayer localPlayer { get; }
    }

    public interface IServerListener : IGameServerListener<ServerPlayer> {
        List<ServerPlayer> connectedPlayers { get; }
        List<ServerPlayer> disconnectedPlayers { get; }
    }

    public class ClientListener : IClientListener {
        public List<MessageContainer> receivedMessages { get; } = new List<MessageContainer>();
        public List<ClientPlayer> connectedPlayers { get; } = new List<ClientPlayer>();
        public List<ClientPlayer> disconnectedPlayers { get; } = new List<ClientPlayer>();
        public bool connectedCalled { get; private set; }
        public bool connectTimeoutCalled { get; private set; }
        public bool disconnectCalled { get; private set; }
        public ClientPlayer localPlayer { get; private set; }

        #region IGameClientListener

        void IGameClientListener<ClientPlayer>.GameClientDidConnect() => this.connectedCalled = true;
        void IGameClientListener<ClientPlayer>.GameClientConnectDidTimeout() => this.connectTimeoutCalled = true;
        void IGameClientListener<ClientPlayer>.GameClientDidDisconnect() => this.disconnectCalled = true;
        void IGameClientListener<ClientPlayer>.GameClientDidIdentifyLocalPlayer(ClientPlayer player) => this.localPlayer = player;
        void IGameClientListener<ClientPlayer>.GameClientDidReceiveMessage(MessageContainer container) => this.receivedMessages.Add(container);
        void IGameClientListener<ClientPlayer>.GameClientPlayerDidConnect(ClientPlayer player) => this.connectedPlayers.Add(player);
        void IGameClientListener<ClientPlayer>.GameClientPlayerDidDisconnect(ClientPlayer player) => this.disconnectedPlayers.Add(player);

        #endregion
    }

    public class ServerListener : IServerListener {
        public ServerListener() {
        }

        public List<ServerPlayer> connectedPlayers { get; } = new List<ServerPlayer>();
        public List<ServerPlayer> disconnectedPlayers { get; } = new List<ServerPlayer>();

        #region IGameServerListener

        void IGameServerListener<ServerPlayer>.GameServerPlayerDidConnect(ServerPlayer player) => connectedPlayers.Add(player);
        void IGameServerListener<ServerPlayer>.GameServerPlayerDidDisconnect(ServerPlayer player) => disconnectedPlayers.Add(player);
        void IGameServerListener<ServerPlayer>.GameServerDidReceiveClientMessage(MessageContainer container, ServerPlayer player) => Assert.NotNull(player);

        #endregion
    }

    public class GameServerClientTests {
        private const string hostIp = "127.0.0.1";

        private NetworkClient NewClient() => new NetworkClient(new TcpSocket(), new UdpSocket());
        private NetworkServer NewServer() => new NetworkServer(new TcpSocket(), new UdpSocket());

        private void NewServer(out GameServer<ServerPlayer> server, out ServerListener listener) {
            this.NewServer(out server, out listener, out _);
        }

        private void NewServer(out GameServer<ServerPlayer> server, out ServerListener listener, out NetworkServer networkServer) {
            var newListener = new ServerListener();
            networkServer = this.NewServer();
            server = new GameServer<ServerPlayer>(networkServer, new GameServerMessageRouter<ServerPlayer>(new MainThreadDispatcher())) { listener = newListener };
            listener = newListener;
        }

        private void NewClient(out GameClient<ClientPlayer> client, out ClientListener listener) {
            var newListener = new ClientListener();
            client = new GameClient<ClientPlayer>(this.NewClient(), new GameClientMessageRouter<ClientPlayer>(new MainThreadDispatcher())) { listener = newListener };
            listener = newListener;
        }

        [Test] 
        public void TestConnectDisconnect() {
            this.NewServer(out GameServer<ServerPlayer> server, out ServerListener serverListener);
            this.NewClient(out GameClient<ClientPlayer> client, out ClientListener clientListener);

            server.Start(5000);

            server.Update();

            client.Connect(hostIp, 5000);

            server.Update();
            client.Update();

            MainThreadDispatcher.Execute();

            server.Update();
            client.Update();

            MainThreadDispatcher.Execute();

            server.Update();
            client.Update();

            MainThreadDispatcher.Execute();

            Assert.IsTrue(clientListener.connectedCalled);

            var player = client.playerCollection.FindPlayer(player => player.isLocalPlayer);
            Assert.IsNotNull(player);
            Assert.IsNotNull(clientListener.localPlayer);
            Assert.AreEqual(player.playerId, serverListener.connectedPlayers[0].playerId);

            var playerId = player.playerId;
            var serverPlayer = server.playerCollection.FindPlayer(playerId);

            Assert.IsNotNull(serverPlayer);

            client.Disconnect();

            client.Update();
            server.Update();

            MainThreadDispatcher.Execute();

            client.Update();
            server.Update();

            MainThreadDispatcher.Execute();

            var notServerPlayer = server.playerCollection.FindPlayer(playerId);

            Assert.IsTrue(clientListener.disconnectCalled);
            Assert.IsNull(notServerPlayer);

            Assert.AreEqual(player.playerId, serverListener.disconnectedPlayers[0].playerId);

            Assert.AreEqual(0, clientListener.disconnectedPlayers.Count);

            server.Stop();

            Thread.Sleep(2000);
        }

        [Test] 
        public void TestMultiPlayerConnectDisconnect() {
            this.NewClient(out GameClient<ClientPlayer> client1, out ClientListener clientListener1);
            this.NewClient(out GameClient<ClientPlayer> client2, out ClientListener clientListener2);
            this.NewClient(out GameClient<ClientPlayer> client3, out ClientListener clientListener3);
            this.NewServer(out GameServer<ServerPlayer> server, out ServerListener serverListener);

            void UpdateAction() {
                server.Update();
                client1.Update();

                MainThreadDispatcher.Execute();

                server.Update();
                client2.Update();

                MainThreadDispatcher.Execute();

                server.Update();
                client3.Update();

                MainThreadDispatcher.Execute();
            }

            server.Start(5000);

            UpdateAction();

            client1.Connect(hostIp, 5000);

            UpdateAction();

            client2.Connect(hostIp, 5000);

            UpdateAction();

            client3.Connect(hostIp, 5000);

            UpdateAction();

            Assert.IsTrue(clientListener1.connectedCalled);
            Assert.IsTrue(clientListener2.connectedCalled);
            Assert.IsTrue(clientListener3.connectedCalled);

            Assert.IsNotNull(clientListener1.localPlayer);
            Assert.IsNotNull(clientListener2.localPlayer);
            Assert.IsNotNull(clientListener3.localPlayer);

            var player1 = client1.playerCollection.FindPlayer(player => player.isLocalPlayer);
            var player2 = client2.playerCollection.FindPlayer(player => player.isLocalPlayer);
            var player3 = client3.playerCollection.FindPlayer(player => player.isLocalPlayer);

            Assert.AreEqual(player1.playerId, serverListener.connectedPlayers[0].playerId);
            Assert.AreEqual(player2.playerId, serverListener.connectedPlayers[1].playerId);
            Assert.AreEqual(player3.playerId, serverListener.connectedPlayers[2].playerId);

            var playerId1 = player1.playerId;
            var playerId2 = player2.playerId;
            var playerId3 = player3.playerId;

            var serverPlayer1 = server.playerCollection.FindPlayer(playerId1);
            var serverPlayer2 = server.playerCollection.FindPlayer(playerId2);
            var serverPlayer3 = server.playerCollection.FindPlayer(playerId3);

            Assert.IsNotNull(serverPlayer1);
            Assert.IsNotNull(serverPlayer2);
            Assert.IsNotNull(serverPlayer3);

            UpdateAction();

            client3.Disconnect();

            UpdateAction();

            Assert.IsNotNull(server.playerCollection.FindPlayer(playerId1));
            Assert.IsNotNull(server.playerCollection.FindPlayer(playerId2));
            Assert.IsNull(server.playerCollection.FindPlayer(playerId3));

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

            Assert.AreEqual(1, client1.playerCollection.values.FindAll(p => p.isLocalPlayer).Count);
            Assert.AreEqual(1, client2.playerCollection.values.FindAll(p => p.isLocalPlayer).Count);
            Assert.AreEqual(0, client3.playerCollection.values.FindAll(p => p.isLocalPlayer).Count);

            server.Stop();

            Thread.Sleep(2000);
        }

        [Test]
        public void TestClientReconnect() {
            this.NewClient(out GameClient<ClientPlayer> client1, out ClientListener listener1_c);
            this.NewClient(out GameClient<ClientPlayer> client2, out _);
            this.NewServer(out GameServer<ServerPlayer> server, out _);

            void Update() {
                server.Update();
                client1.Update();

                MainThreadDispatcher.Execute();

                server.Update();
                client2.Update();

                MainThreadDispatcher.Execute();
            }

            server.Start(5000);

            Update();

            client1.Connect(hostIp, 5000);

            Update();

            client2.Connect(hostIp, 5000);

            Update();

            var player2 = client2.playerCollection.FindPlayer(player => player.isLocalPlayer);
            var disconnectedPlayerId = player2.playerId;

            client2.Disconnect();

            Update();

            Assert.IsNull(server.playerCollection.FindPlayer(player2.playerId));
            this.NewClient(out client2, out ClientListener listener2_c);

            client2.Connect(hostIp, 5000);

            Update();

            Assert.AreEqual(1, listener1_c.disconnectedPlayers.Count);
            Assert.AreEqual(0, listener2_c.disconnectedPlayers.Count);

            Assert.AreEqual(disconnectedPlayerId, listener1_c.disconnectedPlayers[0].playerId);

            server.Stop();

            Thread.Sleep(2000);
        }

        [Test]
        public void TestClientPingBroadcast() {
            this.NewClient(out GameClient<ClientPlayer> client1, out _);
            this.NewClient(out GameClient<ClientPlayer> client2, out _);
            this.NewServer(out GameServer<ServerPlayer> server, out _);

            void Update() {
                server.Update();
                client1.Update();

                MainThreadDispatcher.Execute();

                server.Update();
                client2.Update();

                MainThreadDispatcher.Execute();
            }

            server.Start(5000);

            Update();

            client1.Connect(hostIp, 5000);
            
            Update();

            client2.Connect(hostIp, 5000);

            Update();

            var player1 = client1.playerCollection.FindPlayer(player => player.isLocalPlayer);
            var player2 = client2.playerCollection.FindPlayer(player => player.isLocalPlayer);
            
            Update();
            
            var serverPlayer1 = server.playerCollection.FindPlayer(player1.playerId);
            var serverPlayer2 = server.playerCollection.FindPlayer(player2.playerId);
            var serverPing1 = serverPlayer1.mostRecentPingValue;
            var serverPing2 = serverPlayer2.mostRecentPingValue;
            
            Update();
            
            Assert.Less(MathF.Abs(serverPing1 - player1.mostRecentPingValue), 0.02F);
            Assert.Less(MathF.Abs(serverPing2 - player2.mostRecentPingValue), 0.02F);

            var client1client2Ping = client1.playerCollection[player2.playerId].mostRecentPingValue;
            var client2client1Ping = client2.playerCollection[player1.playerId].mostRecentPingValue;

            Update();

            Assert.Less(MathF.Abs(player1.mostRecentPingValue - client2client1Ping), 0.02F);
            Assert.Less(MathF.Abs(player2.mostRecentPingValue - client1client2Ping), 0.02F);

            server.Stop();

            Thread.Sleep(2000);
        }

        [Test]
        public void TestOneClientDisconnectAndReconnect() {
            this.NewClient(out GameClient<ClientPlayer> client1, out ClientListener clientListener);
            this.NewServer(out GameServer<ServerPlayer> server, out ServerListener _);

            void Update() {
                server.Update();
                client1.Update();

                MainThreadDispatcher.Execute();
            }

            server.Start(5000);

            Update();

            client1.Connect(hostIp, 5000);

            Update();
            
            Assert.IsTrue(clientListener.localPlayer.playerId == 0);

            client1.Disconnect();

            Update();

            this.NewClient(out client1, out clientListener);

            client1.Connect(hostIp, 5000);

            Update();

            Assert.IsTrue(clientListener.localPlayer.playerId == 1);

            server.Stop();

            Thread.Sleep(2000);
        }
    }

    class MainThreadDispatcher : IMainThreadDispatcher {
        public static readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        void IMainThreadDispatcher.Enqueue(Action action) {
            actions.Enqueue(action);
        }

        public static void Execute() {
            Thread.Sleep(250);

            while (actions.TryDequeue(out Action action)) { action.Invoke(); }
        }
    }
}

#endif