using GameNetworking.Channels;
using GameNetworking.Commons.Client;
using GameNetworking.Executors.Client;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Commons;
using GameNetworking.Sockets;

namespace GameNetworking.Client {
    public interface IGameClientListener<TPlayer>
        where TPlayer : IPlayer {
        void GameClientDidConnect(Channel channel);
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();

        void GameClientDidReceiveMessage(MessageContainer container);
        void GameClientPlayerDidConnect(TPlayer player);
        void GameClientDidIdentifyLocalPlayer(TPlayer player);
        void GameClientPlayerDidDisconnect(TPlayer player);
    }

    public interface IGameClient<TPlayer>
        where TPlayer : Player {
        IReadOnlyPlayerCollection<int, TPlayer> playerCollection { get; }
        TPlayer localPlayer { get; }

        IGameClientListener<TPlayer> listener { get; set; }

        void Connect(string host, int port);
        void Disconnect();

        void Update();

        void Send(ITypedMessage message, Channel channel);
    }

    internal interface IRemoteClientListener {
        void RemoteClientDidConnect(int playerId, bool isLocalPlayer);
        void RemoteClientDidDisconnect(int playerId);
    }

    public class GameClient<TPlayer> : IGameClient<TPlayer>, IRemoteClientListener, INetworkClientListener, IMessageAckHelperListener<NatIdentifierResponseMessage>
        where TPlayer : Player, new() {
        private readonly GameClientMessageRouter<TPlayer> router;
        private readonly PlayerCollection<int, TPlayer> _playerCollection = new PlayerCollection<int, TPlayer>();

        private MessageAckHelper<NatIdentifierRequestMessage, NatIdentifierResponseMessage> natIdentifierAckHelper;

        public NetworkClient networkClient { get; }
        public IReadOnlyPlayerCollection<int, TPlayer> playerCollection => this._playerCollection;
        public TPlayer localPlayer { get; private set; }

        public IGameClientListener<TPlayer> listener { get; set; }

        public GameClient(NetworkClient networkClient, GameClientMessageRouter<TPlayer> router) {
            this.networkClient = networkClient;
            this.networkClient.listener = this;

            this.natIdentifierAckHelper = new MessageAckHelper<NatIdentifierRequestMessage, NatIdentifierResponseMessage>(
                this.networkClient.unreliableChannel, router, 10, 2F
            ) { listener = this };

            this.router = router;
            this.router.Configure(this);
        }

        public void Connect(string host, int port) => this.networkClient.Connect(host, port);
        public void Disconnect() => this.networkClient.Disconnect();
        public void Send(ITypedMessage message, Channel channel) {
            switch (channel) {
                case Channel.reliable:
                    this.networkClient.reliableChannel.Send(message);
                    break;
                case Channel.unreliable:
                    var remote = this.networkClient.remoteEndPoint;
                    this.networkClient.unreliableChannel.Send(message, remote);
                    break;
            }
        }

        public virtual void Update() {
            this.natIdentifierAckHelper?.Update();
        }

        void INetworkClientListener.NetworkClientDidConnect() => this.listener?.GameClientDidConnect(Channel.reliable);
        void INetworkClientListener.NetworkClientConnectDidTimeout() => this.listener?.GameClientConnectDidTimeout();
        void INetworkClientListener.NetworkClientDidReceiveMessage(MessageContainer container) => this.router.Route(null, container);

        void INetworkClientListener.NetworkClientDidReceiveMessage(MessageContainer container, NetEndPoint from) {
            IClientMessageRouter router = (IClientMessageRouter)this.natIdentifierAckHelper ?? this.router;
            router.Route(from, container);
        }

        void INetworkClientListener.NetworkClientDidDisconnect() {
            this.listener?.GameClientDidDisconnect();
            this._playerCollection.Clear();
            this.localPlayer = null;
        }

        void IRemoteClientListener.RemoteClientDidConnect(int playerId, bool isLocalPlayer) {
            var player = new TPlayer();
            player.Configure(playerId, isLocalPlayer);

            this._playerCollection.Add(playerId, player);

            this.listener?.GameClientPlayerDidConnect(player);
            if (player.isLocalPlayer) {
                this.localPlayer = player;
                this.listener?.GameClientDidIdentifyLocalPlayer(player);

                var endPoint = this.networkClient.localEndPoint;
                var remote = this.networkClient.remoteEndPoint;
                this.natIdentifierAckHelper.Start(new NatIdentifierRequestMessage(player.playerId, endPoint.host, endPoint.port), remote);
            }
        }

        void IRemoteClientListener.RemoteClientDidDisconnect(int playerId) {
            var player = this._playerCollection.Remove(playerId);
            this.listener?.GameClientPlayerDidDisconnect(player);
        }

        void IMessageAckHelperListener<NatIdentifierResponseMessage>.MessageAckHelperFailed() {
            this.Disconnect();
            this.natIdentifierAckHelper = null;
        }

        void IMessageAckHelperListener<NatIdentifierResponseMessage>.MessageAckHelperReceivedExpectedResponse(NetEndPoint from, NatIdentifierResponseMessage message) {
            new NatIdentifierResponseExecutor<TPlayer>(this, from, message).Execute();
            this.natIdentifierAckHelper = null;
        }
    }
}