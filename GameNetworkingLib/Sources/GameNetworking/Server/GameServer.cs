using System;
using System.Net;
using GameNetworking.Channels;
using GameNetworking.Executors.Server;
using GameNetworking.Messages;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
using GameNetworking.Sockets;

namespace GameNetworking.Server {
    public interface IGameServerListener<TPlayer>
        where TPlayer : IPlayer {
        void GameServerPlayerDidConnect(TPlayer player, Channel channel);
        void GameServerPlayerDidDisconnect(TPlayer player);
        void GameServerDidReceiveClientMessage(MessageContainer container, TPlayer player);
    }

    public interface IGameServer<TPlayer>
        where TPlayer : class, IPlayer {
        NetworkServer networkServer { get; }
        IGameServerPingController<TPlayer> pingController { get; }
        IReadOnlyPlayerCollection<int, TPlayer> playerCollection { get; }

        IGameServerListener<TPlayer> listener { get; set; }

        void Start(int port);
        void Stop();

        void Update();

        void SendBroadcast(ITypedMessage message, Channel channel);
        void SendBroadcast(ITypedMessage message, Predicate<TPlayer> predicate, Channel channel);
    }

    public class GameServer<TPlayer> : IGameServer<TPlayer>, IGameServerClientAcceptorListener<TPlayer>, INetworkServerListener
        where TPlayer : Player, new() {
        private readonly GameServerMessageRouter<TPlayer> router;
        private readonly GameServerClientAcceptor<TPlayer> clientAcceptor;

        internal readonly PlayerCollection<int, TPlayer> _playerCollection;

        public NetworkServer networkServer { get; private set; }
        public IReadOnlyPlayerCollection<int, TPlayer> playerCollection => this._playerCollection;
        public IGameServerPingController<TPlayer> pingController { get; }

        public IGameServerListener<TPlayer> listener { get; set; }

        public GameServer(NetworkServer networkServer, GameServerMessageRouter<TPlayer> router) {
            this.clientAcceptor = new GameServerClientAcceptor<TPlayer>() { listener = this };

            this.networkServer = networkServer;
            this.networkServer.listener = this;

            this._playerCollection = new PlayerCollection<int, TPlayer>();
            this.pingController = new GameServerPingController<TPlayer>(this._playerCollection);

            this.router = router;
            this.router.Configure(this);
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

        void IGameServerClientAcceptorListener<TPlayer>.ClientAcceptorPlayerDidConnect(TPlayer player) {
            player.listener = this.router;
            this._playerCollection.Add(player.playerId, player);
            this.listener?.GameServerPlayerDidConnect(player, Channel.reliable);
        }

        void IGameServerClientAcceptorListener<TPlayer>.ClientAcceptorPlayerDidDisconnect(TPlayer player) {
            player.listener = null;
            this._playerCollection.Remove(player.playerId);
            this.listener?.GameServerPlayerDidDisconnect(player);
        }

        void INetworkServerListener.NetworkServerDidAcceptPlayer(ReliableChannel reliable, UnreliableChannel unreliable) {
            this.clientAcceptor.NetworkServerDidAcceptPlayer(reliable, unreliable);
        }

        void INetworkServerListener.NetworkServerPlayerDidDisconnect(ReliableChannel channel) {
            this.clientAcceptor.NetworkServerPlayerDidDisconnect(channel);
        }

        void INetworkServerListener.NetworkServerDidReceiveUnidentifiedMessage(MessageContainer container, NetEndPoint from) {
            if (container.Is((int)MessageType.natIdentifier)) {
                new NatIdentifierRequestExecutor<TPlayer>(this, from, container.Parse<NatIdentifierRequestMessage>()).Execute();
            }
        }
    }
}