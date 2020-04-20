#if !UNITY_64

using System.Collections.Generic;
using GameNetworking;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Server;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;
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
            this.NewServer(out server, out listener, out _);
        }

        protected override void NewServer(out ReliableGameServer<ReliableServerPlayer> server, out ServerListener listener, out ReliableNetworkingServer networkingServer) {
            var newListener = new ServerListener();
            networkingServer = this.NewServer();
            server = new ReliableGameServer<ReliableServerPlayer>(networkingServer, new MainThreadDispatcher()) { listener = newListener };
            listener = newListener;
        }

        protected override void NewClient(out ReliableGameClient<ReliableClientPlayer> client, out ClientListener listener) {
            var newListener = new ClientListener();
            client = new ReliableGameClient<ReliableClientPlayer>(this.NewClient(), new MainThreadDispatcher()) { listener = newListener };
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

            void IGameClientListener<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.GameClientDidConnect() => this.connectedCalled = true;
            void IGameClientListener<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.GameClientConnectDidTimeout() => this.connectTimeoutCalled = true;
            void IGameClientListener<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.GameClientDidDisconnect() => this.disconnectCalled = true;
            void IGameClientListener<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.GameClientDidIdentifyLocalPlayer(ReliableClientPlayer player) => this.localPlayer = player;
            void IGameClientListener<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.GameClientDidReceiveMessage(MessageContainer container) => this.receivedMessages.Add(container);
            void IGameClientListener<ReliableClientPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.GameClientNetworkPlayerDidDisconnect(ReliableClientPlayer player) => this.disconnectedPlayers.Add(player);

#endregion
        }

        public class ServerListener : IServerListener<ReliableServerPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient> {
            public List<ReliableServerPlayer> connectedPlayers { get; } = new List<ReliableServerPlayer>();
            public List<ReliableServerPlayer> disconnectedPlayers { get; } = new List<ReliableServerPlayer>();

#region IGameServerListener

            void IGameServerListener<ReliableServerPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.GameServerPlayerDidConnect(ReliableServerPlayer player) => connectedPlayers.Add(player);
            void IGameServerListener<ReliableServerPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.GameServerPlayerDidDisconnect(ReliableServerPlayer player) => disconnectedPlayers.Add(player);
            void IGameServerListener<ReliableServerPlayer, ITCPSocket, ReliableNetworkClient, ReliableNetClient>.GameServerDidReceiveClientMessage(MessageContainer container, ReliableServerPlayer player) => Assert.NotNull(player);

#endregion
        }
    }
}

#endif