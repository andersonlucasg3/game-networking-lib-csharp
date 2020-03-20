using System.Collections.Generic;
using Messages.Models;
using GameNetworking.Networking.Commons;
using Networking.Sockets;
using Networking.Models;

namespace GameNetworking {
    using Models;
    using Models.Server;
    using Commons;
    using GameNetworking.Networking;
    using GameNetworking.Networking.Models;

    public class ReliableGameServer<TPlayer> : GameServer<TPlayer>,
        INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener, 
        INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IMessagesListener 
        where TPlayer : NetworkPlayer, new() {

        public interface IListener {
            void GameServerPlayerDidConnect(TPlayer player);
            void GameServerPlayerDidDisconnect(TPlayer player);
            void GameServerDidReceiveClientMessage(MessageContainer container, TPlayer player);
        }

        private readonly GameServerClientAcceptor<TPlayer> clientAcceptor;
        
        internal readonly ReliableNetworkingServer networkingServer;

        public IListener listener { get; set; }

        

        public void Listen(int port) {
            this.networkingServer.Listen(port);
        }

        public void Stop() {
            this.networkingServer.Stop();
        }

        public float GetPing(TPlayer player) {
            return this.pingController.GetPingValue(player);
        }

        public void Disconnect(TPlayer player) {
            this.networkingServer.Disconnect(player.client);
        }

        public void Update() {
            this.networkingServer.Update();
            this.pingController.Update();
        }

        internal void AddPlayer(TPlayer player) {
            this.playersStorage.Add(player);
        }

        internal void RemovePlayer(TPlayer player) {
            this.playersStorage.Remove(player.playerId);
        }

        public TPlayer FindPlayer(int playerId) {
            if (this.playersStorage.TryGetPlayer(playerId, out TPlayer player)) {
                return player;
            }
            return null;
        }

        public List<TPlayer> AllPlayers() {
            return this.playersStorage.players;
        }

        public void SendBroadcast(ITypedMessage message) {
            this.networkingServer.SendBroadcast(message, this.AllPlayers().ConvertAll(c => c.client));
        }

        public void SendBroadcast(ITypedMessage message, TPlayer excludePlayer) {
            TPlayer player;
            for (int i = 0; i < this.playersStorage.players.Count; i++) {
                player = this.playersStorage.players[i];
                if (player != excludePlayer) {
                    this.networkingServer.Send(message, player.client);
                }
            }
        }

        public void Send(ITypedMessage message, TPlayer player) {
            this.networkingServer.Send(message, player.client);
        }

        #region INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IMessagesListener

        void INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IMessagesListener.NetworkingServerDidReadMessage(MessageContainer container, ReliableNetworkClient client) {
            var player = this.playersStorage.Find(client);
            this.router.Route(container, player);
        }

        #endregion

        #region INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener

        void INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.NetworkingServerDidAcceptClient(ReliableNetworkClient client) {
            this.clientAcceptor.AcceptClient(client);
        }

        void INetworkingServer<ITCPSocket, ReliableNetworkClient, ReliableNetClient>.IListener.NetworkingServerClientDidDisconnect(ReliableNetworkClient client) {
            var player = this.playersStorage.Find(client);
            this.clientAcceptor.Disconnect(player);
        }

        #endregion
    }
}
