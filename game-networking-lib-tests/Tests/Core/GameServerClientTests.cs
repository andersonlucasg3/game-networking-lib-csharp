using NUnit.Framework;
using Messages.Models;
using System.Collections.Generic;
using GameNetworking.Commons;
using System;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Server;
using GameNetworking.Networking.Commons;
using Networking.Commons.Sockets;
using GameNetworking.Commons.Models;
using Networking.Commons.Models;
using Test.Core.Model;
using System.Threading;
using System.Linq;

namespace Tests.Core {
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

            var player1 = client1.FindPlayer(player => player.isLocalPlayer);
            var player2 = client2.FindPlayer(player => player.isLocalPlayer);
            
            Update();
            
            var serverPlayer1 = server.FindPlayer(player1.playerId);
            var serverPlayer2 = server.FindPlayer(player2.playerId);
            var serverPing1 = serverPlayer1.mostRecentPingValue;
            var serverPing2 = serverPlayer2.mostRecentPingValue;
            
            Update();
            
            Assert.Less(MathF.Abs(serverPing1 - player1.mostRecentPingValue), 0.02F);
            Assert.Less(MathF.Abs(serverPing2 - player2.mostRecentPingValue), 0.02F);

            var client1client2Ping = client1.GetPing(player2.playerId);
            var client2client1Ping = client2.GetPing(player1.playerId);

            Update();

            Assert.Less(MathF.Abs(player1.mostRecentPingValue - client2client1Ping), 0.02F);
            Assert.Less(MathF.Abs(player2.mostRecentPingValue - client1client2Ping), 0.02F);
        }

        [Test]
        public void TestConnectionTimeOutClient() {
            this.NewClient(out TGameClient client1, out TClientListener clientListener);
            this.NewServer(out TGameServer server, out TServerListener serverListener);

            client1.timeOutDelay = 1F;
            server.timeOutDelay = 1F;

            server.Start("0.0.0.0", 1);

            client1.Connect("0.0.0.0", 1);

            server.Update();
            client1.Update();

            var localPlayer = client1.localPlayer;

            Thread.Sleep((int)(client1.timeOutDelay * 1000));

            client1.Update();

            Assert.IsTrue(clientListener.disconnectCalled);

            server.Update();

            Assert.AreEqual(localPlayer.playerId, serverListener.disconnectedPlayers.First().playerId);
        }
    }

    class MainThreadDispatcher : IMainThreadDispatcher {
        [MTAThread]
        public void Enqueue(Action action) {
            action.Invoke();
        }
    }
}
