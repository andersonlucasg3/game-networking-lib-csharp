using System;
using System.Net;
using GameNetworking.Channels;
using GameNetworking.Commons;
using GameNetworking.Executors.Server;
using GameNetworking.Messages;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Server
{
    public interface IGameServerListener<in TPlayer>
        where TPlayer : IPlayer
    {
        void GameServerPlayerDidConnect(TPlayer player, ChannelType channelType);
        void GameServerPlayerDidDisconnect(TPlayer player);
        void GameServerDidReceiveClientMessage(MessageContainer container, TPlayer player);
    }

    public class GameServer<TPlayer> : IGameServerClientAcceptorListener<TPlayer>, INetworkServerUnidentifiedMessageListener where TPlayer : Player, new()
    {
        private readonly PlayerCollection<int, TPlayer> _playerCollection;
        private readonly GameServerMessageRouter<TPlayer> _router;
        
        public NetworkServer networkServer { get; }
        public IGameServerPingController<TPlayer> pingController { get; }

        public IGameServerListener<TPlayer> listener { get; set; }
        public IReadOnlyPlayerCollection<int, TPlayer> playerCollection => _playerCollection;

        public GameServer(NetworkServer networkServer, GameServerMessageRouter<TPlayer> router)
        {
            ThreadChecker.AssertMainThread();

            GameServerClientAcceptor<TPlayer> clientAcceptor = new GameServerClientAcceptor<TPlayer> {listener = this};

            this.networkServer = networkServer;
            this.networkServer.listener = clientAcceptor;

            _playerCollection = new PlayerCollection<int, TPlayer>();
            pingController = new GameServerPingController<TPlayer>(_playerCollection);

            _router = router;
            _router.Configure(this);
        }
        
        public void SendBroadcast(ITypedMessage message, ChannelType channelType)
        {
            var sendValue = new SendValue {message = message, channelType = channelType};
            _playerCollection.ForEach(Send, sendValue);
        }

        public void StartReliable(int port)
        {
            ThreadChecker.AssertMainThread();

            networkServer.StartReliable(new NetEndPoint(IPAddress.Any, port));
        }

        public void StartUnreliable(int port)
        {
            ThreadChecker.AssertMainThread();
            
            networkServer.StartUnreliable(new NetEndPoint(IPAddress.Any, port));
        }

        public void StartAll(int port)
        {
            ThreadChecker.AssertMainThread();
            
            StartReliable(port);
            StartUnreliable(port);
        }

        public void StopReliable()
        {
            ThreadChecker.AssertMainThread();

            networkServer.StopReliable();
        }

        public void StopUnreliable()
        {
            ThreadChecker.AssertMainThread();
            
            networkServer.StopUnreliable();
        }

        public void StopAll()
        {
            ThreadChecker.AssertMainThread();
            
            networkServer.StopAll();
        }

        public void Update()
        {
            ThreadChecker.AssertMainThread();

            pingController.Update();
        }

        private static void Send(TPlayer player, SendValue value)
        {
            ThreadChecker.AssertMainThread();

            player.Send(value.message, value.channelType);
        }

        public void SendBroadcast(ITypedMessage message, Predicate<TPlayer> predicate, ChannelType channelType)
        {
            ThreadChecker.AssertMainThread();

            var sendValue = new SendValue {message = message, predicate = predicate, channelType = channelType};
            _playerCollection.ForEach(SendPredicate, sendValue);
        }

        private static void SendPredicate(TPlayer player, SendValue value)
        {
            ThreadChecker.AssertMainThread();

            if (value.predicate(player)) player.Send(value.message, value.channelType);
        }
        
        void IGameServerClientAcceptorListener<TPlayer>.ClientAcceptorPlayerDidConnect(TPlayer player)
        {
            player.listener = _router;
            _playerCollection.Add(player.playerId, player);
            _router.dispatcher.Enqueue(() => listener?.GameServerPlayerDidConnect(player, ChannelType.reliable));
        }

        void IGameServerClientAcceptorListener<TPlayer>.ClientAcceptorPlayerDidDisconnect(TPlayer player)
        {
            ThreadChecker.AssertReliableChannel();

            player.listener = null;
            _playerCollection.Remove(player.playerId);
            if (player.remoteIdentifiedEndPoint.HasValue) networkServer.Unregister(player.remoteIdentifiedEndPoint.Value);
            _router.dispatcher.Enqueue(() => listener?.GameServerPlayerDidDisconnect(player));
        }
        
        void INetworkServerUnidentifiedMessageListener.NetworkServerDidReceiveUnidentifiedMessage(MessageContainer container, NetEndPoint from)
        {
            ThreadChecker.AssertUnreliableChannel();

            if (!container.Is((int) MessageType.natIdentifier)) return;

            var serverModel = new GameServerMessageRouter<TPlayer>.ServerModel<NetEndPoint>(this, from);

            var executor = new Executor<NatIdentifierRequestExecutor<TPlayer>, 
                GameServerMessageRouter<TPlayer>.ServerModel<NetEndPoint>,
                NatIdentifierRequestMessage>(serverModel, container);
            _router.dispatcher.Enqueue(executor.Execute);
        }

        private struct SendValue
        {
            public ITypedMessage message;
            public ChannelType channelType;
            public Predicate<TPlayer> predicate;
        }
    }
}
