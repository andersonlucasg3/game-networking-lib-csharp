using System;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons.Models {
    namespace Server {
        public interface INetworkPlayer<TSocket, TClient, TNetClient> : IEquatable<INetworkPlayer<TSocket, TClient, TNetClient>>
                where TSocket : ISocket
                where TClient : INetworkClient<TSocket, TNetClient>
                where TNetClient : INetClient<TSocket, TNetClient> {
            TClient client { get; internal set; }
            int playerId { get; internal set; }
            float mostRecentPingValue { get; internal set; }
            double lastReceivedPingRequest { get; internal set; }
        }

        public class NetworkPlayer<TSocket, TClient, TNetClient> : INetworkPlayer<TSocket, TClient, TNetClient>
            where TSocket : ISocket
            where TClient : INetworkClient<TSocket, TNetClient>
            where TNetClient : INetClient<TSocket, TNetClient> {
            private INetworkPlayer<TSocket, TClient, TNetClient> self => this;

            TClient INetworkPlayer<TSocket, TClient, TNetClient>.client { get; set; }
            int INetworkPlayer<TSocket, TClient, TNetClient>.playerId { get; set; }
            float INetworkPlayer<TSocket, TClient, TNetClient>.mostRecentPingValue { get; set; }
            double INetworkPlayer<TSocket, TClient, TNetClient>.lastReceivedPingRequest { get; set; }

            public TClient client { get => self.client; internal set => self.client = value; }
            public int playerId { get => self.playerId; internal set => self.playerId = value; }
            public float mostRecentPingValue { get => self.mostRecentPingValue; internal set => self.mostRecentPingValue = value; }
            public double lastReceivedPingRequest { get => self.lastReceivedPingRequest; internal set => self.lastReceivedPingRequest = value; }

            public NetworkPlayer() {
                this.lastReceivedPingRequest = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
            }

            public override bool Equals(object obj) {
                if (obj is NetworkPlayer<TSocket, TClient, TNetClient> player) {
                    return this.Equals(player);
                } else if (obj is INetworkClient<TSocket, TNetClient> client) {
                    return self.client.Equals(client);
                }
                return object.Equals(this, obj);
            }

            public bool Equals(INetworkPlayer<TSocket, TClient, TNetClient> other) {
                return self.playerId == other.playerId;
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }
    }

    namespace Client {
        public interface INetworkPlayer<TSocket, TClient, TNetClient> : Server.INetworkPlayer<TSocket, TClient, TNetClient>
                where TSocket : ISocket
                where TClient : INetworkClient<TSocket, TNetClient>
                where TNetClient : INetClient<TSocket, TNetClient> {
            bool isLocalPlayer { get; internal set; }
        }

        public class NetworkPlayer<TSocket, TClient, TNetClient> : Server.NetworkPlayer<TSocket, TClient, TNetClient>, INetworkPlayer<TSocket, TClient, TNetClient>
            where TSocket : ISocket
            where TClient : INetworkClient<TSocket, TNetClient>
            where TNetClient : INetClient<TSocket, TNetClient> {
            private INetworkPlayer<TSocket, TClient, TNetClient> self => this;

            bool INetworkPlayer<TSocket, TClient, TNetClient>.isLocalPlayer { get; set; }

            public bool isLocalPlayer { get => self.isLocalPlayer; }
        }
    }
}
