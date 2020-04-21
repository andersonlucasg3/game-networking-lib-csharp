using GameNetworking.Channels;
using GameNetworking.Commons.Client;
using GameNetworking.Messages.Models;
using GameNetworking.Networking;

namespace GameNetworking.Client {
    public interface IGameClientListener<TPlayer>
        where TPlayer : IPlayer {
        void GameClientDidConnect();
        void GameClientConnectDidTimeout();
        void GameClientDidDisconnect();

        void GameClientDidReceiveMessage(MessageContainer container);
        void GameClientPlayerDidConnect(TPlayer player);
        void GameClientDidIdentifyLocalPlayer(TPlayer player);
        void GameClientPlayerDidDisconnect(TPlayer player);
    }

    public interface IGameClient<TPlayer>
        where TPlayer : class, IPlayer {
        IReadOnlyPlayerCollection<int, TPlayer> playerCollection { get; }
        TPlayer localPlayer { get; }
        double timeOutDelay { get; set; }

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
        where TPlayer : class, IPlayer, new() { 
        private readonly INetworkClient networkClient;
        private readonly GameClientMessageRouter<TPlayer> router;

        internal readonly PlayerCollection<int, TPlayer> _playerCollection;

        public IReadOnlyPlayerCollection<int, TPlayer> playerCollection => this._playerCollection;
        public TPlayer localPlayer { get; private set; }
        public double timeOutDelay { get; set; } = 10F;

        public IGameClientListener<TPlayer> listener { get; set; }

        public GameClient(INetworkClient networkClient, GameClientMessageRouter<TPlayer> router) {
            this.networkClient = networkClient;
            this.networkClient.listener = this;

            this._playerCollection = new PlayerCollection<int, TPlayer>();

            this.router = router;
            this.router.Configure(this);
        }

        public void Connect(string host, int port) => this.networkClient.Connect(host, port);
        public void Disconnect() => this.networkClient.Disconnect();
        public void Send(ITypedMessage message, Channel channel) => this.networkClient.Send(message, channel);

        public virtual void Update() {
            this.networkClient.Receive();
            this.networkClient.Flush();
        }

        void INetworkClientListener.NetworkClientDidConnect() => this.listener?.GameClientDidConnect();
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
                this.listener?.GameClientDidIdentifyLocalPlayer(player);
            }
        }

        void IRemoteClientListener.RemoteClientDidDisconnect(int playerId) {
            var player = this._playerCollection.Remove(playerId);
            this.listener?.GameClientPlayerDidDisconnect(player);
        }
    }
}