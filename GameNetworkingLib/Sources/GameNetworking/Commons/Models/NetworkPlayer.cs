using System;
using Boo.Lang;
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
            double lastReceivedPongRequest { get; internal set; }
        }

        public class NetworkPlayer<TSocket, TClient, TNetClient> : INetworkPlayer<TSocket, TClient, TNetClient>
            where TSocket : ISocket
            where TClient : INetworkClient<TSocket, TNetClient>
            where TNetClient : INetClient<TSocket, TNetClient> {
            private INetworkPlayer<TSocket, TClient, TNetClient> self => this;

            TClient INetworkPlayer<TSocket, TClient, TNetClient>.client { get; set; }
            int INetworkPlayer<TSocket, TClient, TNetClient>.playerId { get; set; }
            float INetworkPlayer<TSocket, TClient, TNetClient>.mostRecentPingValue { get; set; }
            double INetworkPlayer<TSocket, TClient, TNetClient>.lastReceivedPongRequest { get; set; }
        
            public TClient client { get => self.client; internal set => self.client = value; }
            public int playerId { get => self.playerId; internal set => self.playerId = value; }
            public float mostRecentPingValue { get => self.mostRecentPingValue; internal set => self.mostRecentPingValue = value; }
            public double lastReceivedPongRequest { get => self.lastReceivedPongRequest; internal set => self.lastReceivedPongRequest = value; }

            public NetworkPlayer() { this.lastReceivedPongRequest = TimeUtils.CurrentTime(); }

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
                return this.client.GetHashCode() + this.playerId.GetHashCode();
            }
        }
    }

    namespace Client {
        public interface INetworkPlayer<TSocket, TClient, TNetClient> : Server.INetworkPlayer<TSocket, TClient, TNetClient>
                where TSocket : ISocket
                where TClient : INetworkClient<TSocket, TNetClient>
                where TNetClient : INetClient<TSocket, TNetClient> {
            double lastReceivedPingRequest { get; internal set; }

            bool isLocalPlayer { get; internal set; }
        }

        public class NetworkPlayer<TSocket, TClient, TNetClient> : Server.NetworkPlayer<TSocket, TClient, TNetClient>, INetworkPlayer<TSocket, TClient, TNetClient>
            where TSocket : ISocket
            where TClient : INetworkClient<TSocket, TNetClient>
            where TNetClient : INetClient<TSocket, TNetClient> {
            private INetworkPlayer<TSocket, TClient, TNetClient> self => this;

            double INetworkPlayer<TSocket, TClient, TNetClient>.lastReceivedPingRequest { get => this.lastReceivedPongRequest; set => this.lastReceivedPongRequest = value; }

            bool INetworkPlayer<TSocket, TClient, TNetClient>.isLocalPlayer { get; set; }

            public double lastReceivedPingRequest { get => self.lastReceivedPingRequest; internal set => self.lastReceivedPingRequest = value; }

            public bool isLocalPlayer { get => self.isLocalPlayer; }

            public NetworkPlayer() : base() { }
        }
    }
}
