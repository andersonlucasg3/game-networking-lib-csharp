using System;
using GameNetworking.Models.Contract.Server;

namespace GameNetworking.Models {
    namespace Contract {
        namespace Server {
            public interface INetworkPlayer : IEquatable<INetworkPlayer> {
                int playerId { get; }
                float mostRecentPingValue { get; }

                NetworkClient client { get; }
            }
        }
        namespace Client {
            public interface INetworkPlayer : Server.INetworkPlayer {
                bool isLocalPlayer { get; }
            }
        }
    }

    namespace Server {
        public class NetworkPlayer : INetworkPlayer, IEquatable<INetworkPlayer> {
            public NetworkClient client { get; internal set; }
            public int playerId { get; internal set; }
            public float mostRecentPingValue { get; internal set; }

            public NetworkPlayer() { }

            public override bool Equals(object obj) {
                INetworkPlayer self = this;
                if (obj is NetworkPlayer player) {
                    return this.Equals(player);
                } else if (obj is NetworkClient client) {
                    return self.client.Equals(client);
                }
                return object.Equals(this, obj);
            }

            public bool Equals(INetworkPlayer other) {
                INetworkPlayer self = this;
                return self.playerId == other.playerId;
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }
    }

    namespace Client {
        public class NetworkPlayer : Server.NetworkPlayer, Contract.Client.INetworkPlayer {
            public bool isLocalPlayer { get; internal set; }
        }
    }
}
