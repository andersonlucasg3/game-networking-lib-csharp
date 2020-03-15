using System;

namespace GameNetworking.Models {
    namespace Contract {
        namespace Server {
            public interface INetworkPlayer {
                int playerId { get; }
                float mostRecentPingValue { get; }
            }
        }
        namespace Client {
            public interface INetworkPlayer : Server.INetworkPlayer {
                bool isLocalPlayer { get; }
            }
        }
    }

    namespace Server {
        public class NetworkPlayer : Contract.Server.INetworkPlayer {
            private static readonly Random random = new Random();

            internal NetworkClient client {
                get; set;
            }

            public int playerId {
                get; internal set;
            }

            public float mostRecentPingValue { get; internal set; }

            public NetworkPlayer() { }

            public override bool Equals(object obj) {
                if (obj is NetworkPlayer) {
                    return this.playerId == ((NetworkPlayer)obj).playerId;
                } else if (obj is NetworkClient) {
                    return this.client == ((NetworkClient)obj);
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
        }
    }
}
