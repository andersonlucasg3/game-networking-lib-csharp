using System.Linq;
using System.Net;
using GameNetworking.Channels;
using GameNetworking.Commons.Client;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;
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

    public class GameClient<TPlayer> : IGameClient<TPlayer>, IRemoteClientListener, INetworkClientListener
        where TPlayer : Player, new() {
        private readonly INetworkClient networkClient;
        private readonly GameClientMessageRouter<TPlayer> router;
        private readonly PlayerCollection<int, TPlayer> _playerCollection = new PlayerCollection<int, TPlayer>();

        public IReadOnlyPlayerCollection<int, TPlayer> playerCollection => this._playerCollection;
        public TPlayer localPlayer { get; private set; }

        public IGameClientListener<TPlayer> listener { get; set; }

        public GameClient(INetworkClient networkClient, GameClientMessageRouter<TPlayer> router) {
            this.networkClient = networkClient;
            this.networkClient.listener = this;

            this.router = router;
            this.router.Configure(this);
        }

        public void Connect(string host, int port) => this.networkClient.Connect(host, port);
        public void Disconnect() => this.networkClient.Disconnect();
        public void Send(ITypedMessage message, Channel channel) => this.networkClient.Send(message, channel);

        public virtual void Update() {
            this.networkClient.Flush();
        }

        void INetworkClientListener.NetworkClientDidConnect(NetEndPoint endPoint) {
            this.listener?.GameClientDidConnect(Channel.reliable);

            Dns.BeginGetHostEntry(IPAddress.Any, ar => {
                var externalEntry = Dns.EndGetHostEntry(ar);
                if (externalEntry.AddressList.Length > 0) {
                    var externalIp = externalEntry.AddressList.First();
                    var natIdentifier = new NatIdentifierRequestMessage { remoteIp = externalIp.ToString(), port = endPoint.port };
                    this.networkClient.Send(natIdentifier, Channel.reliable);
                } else {
                    // TODO: Maybe give the aplication a informative error
                    this.Disconnect();
                }
            }, null);
        }

        void INetworkClientListener.NetworkClientConnectDidTimeout() => this.listener?.GameClientConnectDidTimeout();
        void INetworkClientListener.NetworkClientDidReceiveMessage(MessageContainer container) => this.router.Route(container);

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
            }
        }

        void IRemoteClientListener.RemoteClientDidDisconnect(int playerId) {
            var player = this._playerCollection.Remove(playerId);
            this.listener?.GameClientPlayerDidDisconnect(player);
        }
    }
}