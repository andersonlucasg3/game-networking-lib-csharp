using UnityEngine;
using System.Collections.Generic;
using Messages.Models;
using System;

namespace GameNetworking {
    using Networking;
    using Models;
    using Messages;

    public class GameServer: MonoBehaviour, INetworkingServerDelegate {
        private NetworkingServer networkingServer;
        private List<NetworkClient> connectedClients;
        private Dictionary<NetworkClient, WeakReference<GameObject>> clientInstances;

        private WeakReference weakDelegate;

        public IGameServerDelegate Delegate {
            get { return this.weakDelegate?.Target as IGameServerDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public void Listen(int port) {
            this.networkingServer.Listen(port);
        }

        #region MonoBehaviour

        private void Start() {
            this.clientInstances = new Dictionary<NetworkClient, WeakReference<GameObject>>();
            this.networkingServer = new NetworkingServer();
            this.networkingServer.Delegate = this;
        }

        private void FixedUpdate() {
            this.networkingServer.AcceptClient();
            this.connectedClients.ForEach((each) => { this.networkingServer.Read(each); });
            this.connectedClients.ForEach((each) => { this.networkingServer.Flush(each); });
        }

        #endregion

        #region INetworkingServerDelegate

        void INetworkingServerDelegate.NetworkingServerDidAcceptClient(NetworkClient client) {
            this.connectedClients.Add(client);
        }

        void INetworkingServerDelegate.NetworkingServerDidReadMessage(MessageContainer container, NetworkClient client) {
            if (container.Is(typeof(SpawnMessage))) {
                var message = container.Parse<SpawnMessage>();
                var gameObject = this.Delegate?.GameServerSpawnCharacter(message.spawnId);
                this.clientInstances[client] = new WeakReference<GameObject>(gameObject);
            }
        }

        #endregion
    }
}