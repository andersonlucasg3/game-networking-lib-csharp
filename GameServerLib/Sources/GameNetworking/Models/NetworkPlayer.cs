using UnityEngine;
using System;

namespace GameNetworking.Models {
    namespace Server {
        public class NetworkPlayer {
            private static readonly System.Random random = new System.Random();

            private WeakReference weakGameObject;

            internal NetworkClient Client {
                get; private set;
            }

            public int PlayerId {
                get; private set;
            }

            public int SpawnId {
                get; set;
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
                this.PlayerId = random.Next();
                this.Client = client;
            }

            internal NetworkPlayer(int playerId) {
                this.PlayerId = playerId;
            }

            public void Despawn() {
                GameObject.Destroy(this.gameObject);
                this.gameObject = null;
            }

            public override bool Equals(object obj) {
                if (obj is NetworkPlayer) {
                    return this.PlayerId == ((NetworkPlayer)obj).PlayerId;
                }
                return object.Equals(this, obj);
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }
    }

    namespace Client {
        public class NetworkPlayer : Server.NetworkPlayer {
            public bool IsLocalPlayer {
                get; internal set;
            }

            public NetworkPlayer(int playerId) : base(playerId) { }
        }
    }
}
