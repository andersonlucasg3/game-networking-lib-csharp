using System.Collections.Generic;
using Messages.Models;
using System;
using Commons;

namespace GameNetworking {
    using Networking;
    using Models;
    using Models.Server;
    using Messages;

    public class GameServer: WeakDelegate<IGameServerDelegate>, IGameInstance, INetworkingServerDelegate, INetworkingServerMessagesDelegate {
        private readonly NetworkPlayersStorage playersStorage;

        private readonly GameServerClientAcceptor clientAcceptor;
        private readonly GameServerMessageRouter router;

        internal readonly NetworkingServer networkingServer;
        
        public readonly GameServerPingController pingController;
        public readonly GameServerSyncController syncController;

        public GameServer() {
            this.playersStorage = new NetworkPlayersStorage();

            this.networkingServer = new NetworkingServer();

            this.clientAcceptor = new GameServerClientAcceptor(this);
            this.router = new GameServerMessageRouter(this);

            this.syncController = new GameServerSyncController(this, this.playersStorage);
            this.pingController = new GameServerPingController(this, this.playersStorage);

            this.networkingServer.Delegate = this;
            this.networkingServer.MessagesDelegate = this;
        }

        public void Listen(int port) {
            this.networkingServer.Listen(port);
        }

        public void Stop() {
            this.networkingServer.Stop();
        }

        public void StartGame() {
            this.playersStorage.ForEach((each) => {
                this.networkingServer.Send(new StartGameMessage(), each.Client);
            });
        }

        public float GetPing(NetworkPlayer player) {
            return this.pingController.GetPingValue(player);
        }

        public void Update() {
            this.networkingServer.Update();
            this.syncController.Update();
            this.pingController.Update();
        }

        internal void AddPlayer(NetworkPlayer player) {
            this.playersStorage.Add(player);
        }

        internal void RemovePlayer(NetworkPlayer player) {
            this.playersStorage.Remove(player);
        }

        internal NetworkPlayer FindPlayer(NetworkClient client) {
            return this.playersStorage.Find(player => player.Client == client);
        }

        internal NetworkPlayer FindPlayer(int playerId) {
            return this.playersStorage.Find(player => player.PlayerId == playerId);
        }

        internal List<NetworkPlayer> AllPlayers() {
            return this.playersStorage.Players;
        }

        internal void SendBroadcast(ITypedMessage message) {
            this.networkingServer.SendBroadcast(message, this.playersStorage.ConvertAll(c => c.Client));
        }

        internal void SendBroadcast(ITypedMessage message, NetworkClient excludeClient) {
            List<NetworkClient> clientList = this.playersStorage.ConvertFindingAll(
                player => player.Client != excludeClient, 
                player => player.Client
            );
            this.networkingServer.SendBroadcast(message, clientList);
        }

        internal void Send(ITypedMessage message, NetworkClient client) {
            this.networkingServer.Send(message, client);
        }

        #region INetworkingServerMessagesDelegate

        void INetworkingServerMessagesDelegate.NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client) {
            this.router.Route(container, client);
        }

        #endregion

        #region INetworkingServerDelegate

        void INetworkingServerDelegate.NetworkingServerDidAcceptClient(NetworkClient client) {
            this.clientAcceptor.AcceptClient(client);
        }

        void INetworkingServerDelegate.NetworkingServerClientDidDisconnect(NetworkClient client) {
            this.clientAcceptor.Disconnect(client);
        }

        #endregion
    }
}
