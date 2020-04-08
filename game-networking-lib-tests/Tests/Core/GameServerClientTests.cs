using NUnit.Framework;
using GameNetworking;
using Tests.Core.Model;
using Messages.Models;
using System.Collections.Generic;
using GameNetworking.Commons;
using Networking;
using System;
using GameNetworking.Networking;
using GameNetworking.Commons.Client;
using Networking.Sockets;
using GameNetworking.Networking.Models;
using Networking.Models;
using GameNetworking.Commons.Server;
using GameNetworking.Networking.Commons;
using Networking.Commons.Sockets;
using GameNetworking.Commons.Models;
using Networking.Commons.Models;
using Test.Core.Model;

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

    public interface IClientListener<TPlayer, TSocket, TClient, TNetClient> : IGameClient<TPlayer, TSocket, TClient, TNetClient>.IListener
        where TPlayer : class, GameNetworking.Commons.Models.Client.INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        
        List<MessageContainer> receivedMessages { get; }
        List<TPlayer> disconnectedPlayers { get; }
        bool connectedCalled { get; }
        bool connectTimeoutCalled { get; }
        bool disconnectCalled { get; }
        TPlayer localPlayer { get; }
    }

    public interface IServerListener<TPlayer, TSocket, TClient, TNetClient> : IGameServer<TPlayer, TSocket, TClient, TNetClient>.IListener
        where TPlayer : class, GameNetworking.Commons.Models.Server.INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {
        
        List<TPlayer> connectedPlayers { get; }
        List<TPlayer> disconnectedPlayers { get; }
    }

    public abstract class GameServerClientTests<TNetworkingServer, TNetworkingClient, TGameServer, TGameClient, TServerPlayer, TClientPlayer, TSocket, TClient, TNetClient, TClientAcceptor, TServerListener, TClientListener>
        where TNetworkingServer : INetworkingServer<TSocket, TClient, TNetClient>
        where TNetworkingClient : INetworkingClient<TSocket, TClient, TNetClient>
        where TGameServer : IGameServer<TServerPlayer, TSocket, TClient, TNetClient>
        where TGameClient : IGameClient<TClientPlayer, TSocket, TClient, TNetClient>
        where TServerPlayer : class, GameNetworking.Commons.Models.Server.INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TClientPlayer : class, GameNetworking.Commons.Models.Client.INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient>
        where TClientAcceptor : GameServerClientAcceptor<TGameServer, TNetworkingServer, TServerPlayer, TSocket, TClient, TNetClient>, new()
        where TServerListener : IServerListener<TServerPlayer, TSocket, TClient, TNetClient>
        where TClientListener : IClientListener<TClientPlayer, TSocket, TClient, TNetClient> {

        [SetUp]
        public void SetUp() {
            UnreliableSocketMock.Setup();
        }

        protected abstract TNetworkingServer NewServer();
        protected abstract TNetworkingClient NewClient();
        protected abstract void NewServer(out TGameServer server, out TServerListener listener);
        protected abstract void NewClient(out TGameClient client, out TClientListener listener);

        protected void Update(TGameServer server) {
            server.Update();
        }

        protected void Update(TGameClient client) {
            client.Update();
        }

        [Test] 
        public void TestConnectDisconnect() {
            this.NewClient(out TGameClient client, out TClientListener clientListener);
            this.NewServer(out TGameServer server, out TServerListener serverListener);

            server.Start("0.0.0.0", 1);

            client.Connect("0.0.0.0", 1);

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
            this.Update(client);

            var notServerPlayer = server.FindPlayer(playerId);

            Assert.IsTrue(clientListener.disconnectCalled);
            Assert.IsNull(notServerPlayer);

            Assert.AreEqual(player.playerId, serverListener.disconnectedPlayers[0].playerId);

            Assert.AreEqual(0, clientListener.disconnectedPlayers.Count);
        }

        [Test] 
        public void TestMultiPlayerConnectDisconnect() {
            this.NewClient(out TGameClient client1, out TClientListener clientListener1);
            this.NewClient(out TGameClient client2, out TClientListener clientListener2);
            this.NewClient(out TGameClient client3, out TClientListener clientListener3);
            this.NewServer(out TGameServer server, out TServerListener serverListener);

            void UpdateAction() {
                this.Update(server);
                this.Update(server);
                this.Update(server);
                this.Update(client1);
                this.Update(client2);
                this.Update(client3);
            }

            server.Start("0.0.0.0", 1);

            client1.Connect("0.0.0.0", 1);
            client2.Connect("0.0.0.0", 1);
            client3.Connect("0.0.0.0", 1);

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
            this.NewClient(out TGameClient client1, out TClientListener listener1_c);
            this.NewClient(out TGameClient client2, out _);
            this.NewServer(out TGameServer server, out _);

            void Update() {
                this.Update(server);
                this.Update(server);
                this.Update(client1);
                this.Update(client2);
            }

            server.Start("0.0.0.0", 1);

            client1.Connect("0.0.0.0", 1);
            client2.Connect("0.0.0.0", 1);

            Update();

            Update();

            var player2 = client2.FindPlayer(player => player.isLocalPlayer);
            var disconnectedPlayerId = player2.playerId;

            client2.Disconnect();

            Update();

            Assert.IsNull(server.FindPlayer(player2.playerId));
            this.NewClient(out client2, out TClientListener listener2_c);

            client2.Connect("0.0.0.0", 1);

            Update();

            Update();

            Assert.AreEqual(1, listener1_c.disconnectedPlayers.Count);
            Assert.AreEqual(0, listener2_c.disconnectedPlayers.Count);

            Assert.AreEqual(disconnectedPlayerId, listener1_c.disconnectedPlayers[0].playerId);
        }

        [Test]
        public void TestClientPingBroadcast() {
            this.NewClient(out TGameClient client1, out _);
            this.NewClient(out TGameClient client2, out _);
            this.NewServer(out TGameServer server, out _);

            void Update() {
                this.Update(server);
                this.Update(server);
                this.Update(client1);
                this.Update(client2);
            }

            server.Start("0.0.0.0", 1);

            client1.Connect("0.0.0.0", 1);
            client2.Connect("0.0.0.0", 1);

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

    class MainThreadDispatcher : IMainThreadDispatcher {
        public void Enqueue(Action action) {
            action.Invoke();
        }
    }
}
