using UnityEngine;
using System.Collections.Generic;
using Messages.Models;
using Messages.Coders;
using System;

namespace GameNetworking {
    using Networking;
    using Models;
    using Messages;
    using Messages.Models;

    public class GameServer: INetworkingServerDelegate {
        private readonly NetworkingServer networkingServer;
        private readonly List<KeyValuePair<NetworkClient, NetworkPlayer>> connectedPlayers;

        private WeakReference weakDelegate;

        public IGameServerDelegate Delegate {
            get { return this.weakDelegate?.Target as IGameServerDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public GameServer() {
            this.connectedPlayers = new List<KeyValuePair<NetworkClient, NetworkPlayer>>();
            this.networkingServer = new NetworkingServer {
                Delegate = this
            };
        }

        public void Listen(int port) {
            this.networkingServer.Listen(port);
        }

        public void StartGame() {
            this.connectedPlayers.ForEach((each) => {
                this.networkingServer.Send(new StartGameMessage(), each.Key);
            });
        }

        public void Update() {
            this.networkingServer.AcceptClient();
            this.connectedPlayers.ForEach((each) => { this.networkingServer.Read(each.Key); });
            this.connectedPlayers.ForEach((each) => { this.networkingServer.Flush(each.Key); });
        }

        #region INetworkingServerDelegate

        void INetworkingServerDelegate.NetworkingServerDidAcceptClient(NetworkClient client) {
            NetworkPlayer player = new NetworkPlayer();
            this.BroadcastMessage(new PlayerMirrorInfo {
                playerId = player.PlayerId
            });
            this.connectedPlayers.Add(new KeyValuePair<NetworkClient, NetworkPlayer>(client, player));
        }

        void INetworkingServerDelegate.NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client) {
            var playerInfo = this.connectedPlayers.Find(x => x.Key.Equals(client)).Value;
            if (container.Is(typeof(SpawnMessage))) {
                var message = container.Parse<SpawnMessage>();
                var spawnedObject = this.Delegate?.GameServerSpawnCharacter(message.spawnId, playerInfo);
                playerInfo.GameObject = spawnedObject;
                this.BroadcastSpawn(playerInfo, message.spawnId);
            } else {
                this.Delegate?.GameServerDidReceiveClientMessage(container, playerInfo);
            }
        }

        #endregion

        private void BroadcastMessage(IEncodable message) {
            this.connectedPlayers.ForEach(each => this.networkingServer.Send(message, each.Key));
        }

        private void BroadcastSpawn(NetworkPlayer player, int spawnId) {
            this.BroadcastMessage(new SpawnMessage {
                spawnId = spawnId,
                playerId = player.PlayerId,
                position = player.GameObject.transform.position.ToVec3(),
                rotation = player.GameObject.transform.eulerAngles.ToVec3()
            });
        }
    }
}
