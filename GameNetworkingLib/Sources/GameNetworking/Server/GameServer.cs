using System;
using System.Collections.Concurrent;
using System.Net;
using GameNetworking.Channels;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Sockets;

namespace GameNetworking.Server {
    public interface IGameServerListener<TPlayer>
        where TPlayer : IPlayer {
        void GameServerPlayerDidConnect(TPlayer player);
        void GameServerPlayerDidDisconnect(TPlayer player);
        void GameServerDidReceiveClientMessage(MessageContainer container, TPlayer player);
    }

    public interface IGameServer<TPlayer>
        where TPlayer : class, IPlayer {
        NetworkServer networkServer { get; }
        IGameServerPingController<TPlayer> pingController { get; }
        IReadOnlyPlayerCollection<int, TPlayer> playerCollection { get; }

        double timeOutDelay { get; set; }

        IGameServerListener<TPlayer> listener { get; set; }

        void Start(int port);
        void Stop();

        void Update();

        void SendBroadcast(ITypedMessage message, Channel channel);
        void SendBroadcast(ITypedMessage message, Predicate<TPlayer> predicate, Channel channel);
    }

    public class GameServer<TPlayer> : IGameServer<TPlayer>, INetworkServerListener, IGameServerClientAcceptorListener<TPlayer>
        where TPlayer : class, IPlayer, new() {
        private readonly GameServerMessageRouter<TPlayer> router;
        private readonly GameServerClientAcceptor<TPlayer> clientAcceptor;
        private readonly ConcurrentDictionary<IChannel, TPlayer> channelCollection;

        internal readonly PlayerCollection<int, TPlayer> _playerCollection;

        public NetworkServer networkServer { get; private set; }
        public IReadOnlyPlayerCollection<int, TPlayer> playerCollection => this._playerCollection;
        public IGameServerPingController<TPlayer> pingController { get; }
        public double timeOutDelay { get; set; } = 10F;

        public IGameServerListener<TPlayer> listener { get; set; }

        public GameServer(NetworkServer networkServer, GameServerMessageRouter<TPlayer> router) {
            this.networkServer = networkServer;
            this.networkServer.listener = this;

            this.channelCollection = new ConcurrentDictionary<IChannel, TPlayer>();
            this._playerCollection = new PlayerCollection<int, TPlayer>();
            this.pingController = new GameServerPingController<TPlayer>(this._playerCollection);

            this.router = router;
            this.router.Configure(this);

            this.clientAcceptor = new GameServerClientAcceptor<TPlayer>() { listener = this };
        }

        public void Start(int port) {
            this.networkServer.Start(new NetEndPoint(IPAddress.Any.ToString(), port));
        }

        public void Stop() {
            this.networkServer.Stop();
        }

        public void Update() {
            this.networkServer.Update();
            this.pingController.Update();
        }

        public void SendBroadcast(ITypedMessage message, Channel channel) {
            this._playerCollection.ForEach((player) => player.Send(message, channel));
        }

        public void SendBroadcast(ITypedMessage message, Predicate<TPlayer> predicate, Channel channel) {
            this._playerCollection.ForEach((player) => { if (predicate(player)) { player.Send(message, channel); } });
        }

        void INetworkServerListener.NetworkServerDidAcceptPlayer(ReliableChannel reliable, UnreliableChannel unreliable) {
            var player = new TPlayer();
            player.Configure(reliable, unreliable);

            this.channelCollection[reliable] = player;

            this.clientAcceptor.AcceptClient(player);
        }

        void INetworkServerListener.NetworkServerPlayerDidDisconnect(ReliableChannel channel) {
            if (this.channelCollection.TryRemove(channel, out TPlayer player)) {
                this.clientAcceptor.Disconnect(player);
            }
        }

        void IGameServerClientAcceptorListener<TPlayer>.ClientAcceptorPlayerDidConnect(TPlayer player) {
            this.listener?.GameServerPlayerDidConnect(player);
        }

        void IGameServerClientAcceptorListener<TPlayer>.ClientAcceptorPlayerDidDisconnect(TPlayer player) {
            this.listener?.GameServerPlayerDidDisconnect(player);
        }
    }
}