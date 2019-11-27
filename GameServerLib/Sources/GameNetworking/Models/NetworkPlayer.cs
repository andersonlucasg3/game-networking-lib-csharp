using UnityEngine;
using System;
using GameNetworking.Models.Contract;
using GameNetworking.Models.Contract.Server;
using GameNetworking.Models.Contract.Client;

namespace GameNetworking.Models {
    namespace Contract {
        namespace Server {
            public interface INetworkPlayer {
                int playerId { get; }
                int spawnId { get; }

                GameObject gameObject { get; }
                Transform transform { get; }

                void Despawn();
            }
        }
        namespace Client {
            public interface INetworkPlayer : Server.INetworkPlayer {
                bool isLocalPlayer { get; }
            }
        }
    }

    namespace Server {
        public class NetworkPlayer: Contract.Server.INetworkPlayer {
            private static readonly Random random = new Random();

            private WeakReference weakGameObject;

            internal NetworkClient client {
                get; private set;
            }

            public int playerId {
                get; private set;
            }

            public int spawnId {
                get; internal set;
            }

            public GameObject gameObject {
                get { return this.weakGameObject?.Target as GameObject; }
                internal set { this.weakGameObject = new WeakReference(value); }
            }

            public Transform transform {
                get { return this.gameObject?.transform; }
            }

            internal InputState inputState = new InputState();

            internal NetworkPlayer(NetworkClient client) {
                this.playerId = random.Next();
                this.client = client;
            }

            internal NetworkPlayer(int playerId) {
                this.playerId = playerId;
            }

            public void Despawn() {
                GameObject.Destroy(this.gameObject);
                this.gameObject = null;
            }

            public override bool Equals(object obj) {
                if (obj is NetworkPlayer) {
                    return this.playerId == ((NetworkPlayer)obj).playerId;
                }
                return object.Equals(this, obj);
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }
    }

    namespace Client {
        public class NetworkPlayer : Server.NetworkPlayer, Contract.Client.INetworkPlayer {
            public bool isLocalPlayer {
                get; internal set;
            }

            public NetworkPlayer(int playerId) : base(playerId) { }
        }
    }
}
