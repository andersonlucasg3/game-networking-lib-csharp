using System;
using GameNetworking.Models.Contract.Server;

namespace GameNetworking.Models {
    namespace Contract {
        namespace Server {
            public interface INetworkPlayer : IEquatable<INetworkPlayer> {
                int playerId { get; internal set; }
                float mostRecentPingValue { get; internal set; }

                NetworkClient client { get; internal set; }
            }
        }
        namespace Client {
            public interface INetworkPlayer : Server.INetworkPlayer {
                bool isLocalPlayer { get; internal set; }
            }
        }
    }

    namespace Server {
        public class NetworkPlayer : Contract.Server.INetworkPlayer {
            private static readonly Random random = new Random();

            NetworkClient Contract.Server.INetworkPlayer.client { get; set; }
            int Contract.Server.INetworkPlayer.playerId { get; set; }
            float Contract.Server.INetworkPlayer.mostRecentPingValue { get; set; }

            internal NetworkClient client { get; set; }

            public int playerId { get; internal set; }
            public float mostRecentPingValue { get; internal set; }

            public NetworkPlayer() { playerId = random.Next(); }

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

            public bool Equals(INetworkPlayer other) {
                return this.playerId == other?.playerId;
            }
        }
    }

    namespace Client {
        public class NetworkPlayer : Server.NetworkPlayer, Contract.Client.INetworkPlayer {
            bool Contract.Client.INetworkPlayer.isLocalPlayer { get; set; }
            public bool isLocalPlayer { get; internal set; }
        }
    }
}
