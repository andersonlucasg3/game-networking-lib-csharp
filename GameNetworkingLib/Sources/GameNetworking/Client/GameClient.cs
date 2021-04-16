using System;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Executors.Client;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Client
{
    public interface IGameClientListener<TPlayer>
        where TPlayer : IPlayer
    {
        void GameClientDidConnect(ChannelType channelType);
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();

        void GameClientDidReceiveMessage(MessageContainer container);
        void GameClientPlayerDidConnect(TPlayer player);
        void GameClientDidIdentifyLocalPlayer(TPlayer player);
        void GameClientPlayerDidDisconnect(TPlayer player);
    }

    internal interface IRemoteClientListener
    {
        void RemoteClientDidConnect(int playerId, bool isLocalPlayer);
        void RemoteClientDidDisconnect(int playerId);
    }

    public class GameClient<TPlayer> : IRemoteClientListener, INetworkClientListener, IMessageAckHelperListener<NatIdentifierResponseMessage>
        where TPlayer : Player, new()
    {
        private readonly PlayerCollection<int, TPlayer> _playerCollection = new PlayerCollection<int, TPlayer>();
        private readonly GameClientMessageRouter<TPlayer> router;

        private MessageAckHelper<NatIdentifierRequestMessage, NatIdentifierResponseMessage> natIdentifierAckHelper;

        public NetworkClient networkClient { get; }

        public IReadOnlyPlayerCollection<int, TPlayer> playerCollection => _playerCollection;

        public TPlayer localPlayer { get; private set; }

        public IGameClientListener<TPlayer> listener { get; set; }

        public GameClient(NetworkClient networkClient, GameClientMessageRouter<TPlayer> router)
        {
            ThreadChecker.AssertMainThread();

            this.networkClient = networkClient;
            this.networkClient.listener = this;

            natIdentifierAckHelper = new MessageAckHelper<NatIdentifierRequestMessage, NatIdentifierResponseMessage>(
                this.networkClient.unreliableChannel, router, 10, 2F
            ) {listener = this};

            this.router = router;
            this.router.Configure(this);
        }

        public void ConnectReliable(string host, int port)
        {
            ThreadChecker.AssertMainThread();
            networkClient.ConnectReliable(host, port);
        }

        public void ConnectUnreliable(string host, int port)
        {
            ThreadChecker.AssertMainThread();
            networkClient.ConnectUnreliable(host, port);
        }

        public void ConnectAll(string host, int port)
        {
            ThreadChecker.AssertMainThread();
            ConnectReliable(host, port);
            ConnectUnreliable(host, port);
        }

        public void DisconnectReliable()
        {
            ThreadChecker.AssertMainThread();
            networkClient.DisconnectReliable();
        }

        public void DisconnectUnreliable()
        {
            ThreadChecker.AssertMainThread();
            networkClient.DisconnectUnreliable();
        }

        public void DisconnectAll()
        {
            ThreadChecker.AssertMainThread();
            DisconnectReliable();
            DisconnectUnreliable();
        }

        public void Send(ITypedMessage message, ChannelType channelType)
        {
            ThreadChecker.AssertMainThread();
            switch (channelType)
            {
                case ChannelType.reliable:
                    networkClient.reliableChannel.Send(message);
                    break;
                case ChannelType.unreliable:
                    var remote = networkClient.remoteEndPoint;
                    networkClient.unreliableChannel.Send(message, remote);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channelType), channelType, null);
            }
        }

        public virtual void Update()
        {
            ThreadChecker.AssertMainThread();
            natIdentifierAckHelper?.Update();
        }

        void IMessageAckHelperListener<NatIdentifierResponseMessage>.MessageAckHelperFailed()
        {
            DisconnectUnreliable();
            natIdentifierAckHelper = null;
        }

        void IMessageAckHelperListener<NatIdentifierResponseMessage>.MessageAckHelperReceivedExpectedResponse(NetEndPoint from, NatIdentifierResponseMessage message)
        {
            ThreadChecker.AssertUnreliableChannel();

            router.dispatcher.Enqueue(() => new NatIdentifierResponseExecutor<TPlayer>().Execute(this, message));
            natIdentifierAckHelper = null;
        }

        void INetworkClientListener.NetworkClientReliableChannelConnected()
        {
            router.dispatcher.Enqueue(() => listener?.GameClientDidConnect(ChannelType.reliable));
        }

        void INetworkClientListener.NetworkClientReliableChannelTimedOut()
        {
            router.dispatcher.Enqueue(() => listener?.GameClientConnectDidTimeout());
        }

        void INetworkClientListener.NetworkClientDidReceiveMessage(MessageContainer container)
        {
            ThreadChecker.AssertReliableChannel();
            router.Route(container);
        }

        void INetworkClientListener.NetworkClientDidReceiveMessage(MessageContainer container, NetEndPoint from)
        {
            ThreadChecker.AssertUnreliableChannel();
            if (natIdentifierAckHelper != null)
            {
                natIdentifierAckHelper.Route(@from, container);
            }
            else
            {
                router.Route(container);
            }
        }

        void INetworkClientListener.NetworkClientReliableChannelDisconnected()
        {
            ThreadChecker.AssertReliableChannel();
            router.dispatcher.Enqueue(() => listener?.GameClientDidDisconnect());
            _playerCollection.Clear();
            localPlayer = null;
        }

        void IRemoteClientListener.RemoteClientDidConnect(int playerId, bool isLocalPlayer)
        {
            var player = new TPlayer();
            player.Configure(playerId, isLocalPlayer);

            _playerCollection.Add(playerId, player);

            listener?.GameClientPlayerDidConnect(player);
            
            if (!player.isLocalPlayer) return;
            
            localPlayer = player;
            router.dispatcher.Enqueue(() => listener?.GameClientDidIdentifyLocalPlayer(player));

            var endPoint = networkClient.localEndPoint;
            var remote = networkClient.remoteEndPoint;
            natIdentifierAckHelper.Start(new NatIdentifierRequestMessage(player.playerId, endPoint.address, endPoint.port), remote);
        }

        void IRemoteClientListener.RemoteClientDidDisconnect(int playerId)
        {
            ThreadChecker.AssertReliableChannel();

            var player = _playerCollection.Remove(playerId);
            router.dispatcher.Enqueue(() => listener?.GameClientPlayerDidDisconnect(player));
        }
    }
}
