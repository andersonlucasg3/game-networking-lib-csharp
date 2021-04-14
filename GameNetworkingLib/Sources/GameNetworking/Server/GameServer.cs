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
        void GameServerPlayerDidConnect(TPlayer player, Channel channel);
        void GameServerPlayerDidDisconnect(TPlayer player);
        void GameServerDidReceiveClientMessage(MessageContainer container, TPlayer player);
    }

    public class GameServer<TPlayer> : IGameServerClientAcceptorListener<TPlayer>, INetworkServerListener where TPlayer : Player, new()
    {
        private readonly PlayerCollection<int, TPlayer> _playerCollection;
        private readonly GameServerClientAcceptor<TPlayer> _clientAcceptor;
        private readonly GameServerMessageRouter<TPlayer> _router;

        public GameServer(NetworkServer networkServer, GameServerMessageRouter<TPlayer> router)
        {
            ThreadChecker.AssertMainThread();

            _clientAcceptor = new GameServerClientAcceptor<TPlayer> {listener = this};

            this.networkServer = networkServer;
            this.networkServer.listener = this;

            _playerCollection = new PlayerCollection<int, TPlayer>();
            pingController = new GameServerPingController<TPlayer>(_playerCollection);

            _router = router;
            _router.Configure(this);
        }

        public NetworkServer networkServer { get; }
        public IGameServerPingController<TPlayer> pingController { get; }

        public IGameServerListener<TPlayer> listener { get; set; }
        public IReadOnlyPlayerCollection<int, TPlayer> playerCollection => _playerCollection;

        public void SendBroadcast(ITypedMessage message, Channel channel)
        {
            var sendValue = new SendValue {message = message, channel = channel};
            _playerCollection.ForEach(Send, sendValue);
        }

        void IGameServerClientAcceptorListener<TPlayer>.ClientAcceptorPlayerDidConnect(TPlayer player)
        {
            player.listener = _router;
            _playerCollection.Add(player.playerId, player);
            _router.dispatcher.Enqueue(() => listener?.GameServerPlayerDidConnect(player, Channel.reliable));
        }

        void IGameServerClientAcceptorListener<TPlayer>.ClientAcceptorPlayerDidDisconnect(TPlayer player)
        {
            ThreadChecker.AssertReliableChannel();

            player.listener = null;
            _playerCollection.Remove(player.playerId);
            if (player.remoteIdentifiedEndPoint.HasValue) networkServer.Unregister(player.remoteIdentifiedEndPoint.Value);
            _router.dispatcher.Enqueue(() => listener?.GameServerPlayerDidDisconnect(player));
        }

        void INetworkServerListener.NetworkServerDidAcceptPlayer(ReliableChannel reliable, UnreliableChannel unreliable)
        {
            _clientAcceptor.NetworkServerDidAcceptPlayer(reliable, unreliable);
        }

        void INetworkServerListener.NetworkServerPlayerDidDisconnect(ReliableChannel channel)
        {
            ThreadChecker.AssertReliableChannel();

            _clientAcceptor.NetworkServerPlayerDidDisconnect(channel);
        }

        void INetworkServerListener.NetworkServerDidReceiveUnidentifiedMessage(MessageContainer container, NetEndPoint from)
        {
            ThreadChecker.AssertUnreliableChannel();

            if (!container.Is((int) MessageType.natIdentifier)) return;

            var serverModel = new GameServerMessageRouter<TPlayer>.ServerModel<NetEndPoint>(this, from);

            var executor = new Executor<NatIdentifierRequestExecutor<TPlayer>, GameServerMessageRouter<TPlayer>.ServerModel<NetEndPoint>,
                NatIdentifierRequestMessage>(serverModel, container);
            _router.dispatcher.Enqueue(executor.Execute);
        }

        public void Start(int port)
        {
            ThreadChecker.AssertMainThread();

            networkServer.Start(new NetEndPoint(IPAddress.Any, port));
        }

        public void Stop()
        {
            ThreadChecker.AssertMainThread();

            networkServer.Stop();
        }

        public void Update()
        {
            ThreadChecker.AssertMainThread();

            pingController.Update();
        }

        private void Send(TPlayer player, SendValue value)
        {
            ThreadChecker.AssertMainThread();

            player.Send(value.message, value.channel);
        }

        public void SendBroadcast(ITypedMessage message, Predicate<TPlayer> predicate, Channel channel)
        {
            ThreadChecker.AssertMainThread();

            var sendValue = new SendValue {message = message, predicate = predicate, channel = channel};
            _playerCollection.ForEach(SendPredicate, sendValue);
        }

        private void SendPredicate(TPlayer player, SendValue value)
        {
            ThreadChecker.AssertMainThread();

            if (value.predicate(player)) player.Send(value.message, value.channel);
        }

        private struct SendValue
        {
            public ITypedMessage message;
            public Channel channel;
            public Predicate<TPlayer> predicate;
        }
    }
}
