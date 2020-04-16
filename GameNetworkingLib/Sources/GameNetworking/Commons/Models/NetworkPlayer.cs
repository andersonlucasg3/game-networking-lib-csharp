using System;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Commons.Models {
    namespace Server {
        public interface INetworkPlayer<TSocket, TClient, TNetClient> : IEquatable<INetworkPlayer<TSocket, TClient, TNetClient>>
                where TSocket : ISocket
                where TClient : INetworkClient<TSocket, TNetClient>
                where TNetClient : INetClient<TSocket, TNetClient> {
            TClient client { get; }
            int playerId { get; }
            float mostRecentPingValue { get; }
            double lastReceivedPongRequest { get; }

            void Configure(int playerId);
            void Configure(TClient client, int playerId);
        }

        public class NetworkPlayer<TSocket, TClient, TNetClient> : INetworkPlayer<TSocket, TClient, TNetClient>
            where TSocket : ISocket
            where TClient : INetworkClient<TSocket, TNetClient>
            where TNetClient : INetClient<TSocket, TNetClient> {

            public TClient client { get; internal set; }
            public int playerId { get; internal set; }
            public float mostRecentPingValue { get; internal set; }
            public double lastReceivedPongRequest { get; internal set; }

            public NetworkPlayer() { this.lastReceivedPongRequest = TimeUtils.CurrentTime(); }

            public void Configure(int playerId) {
                this.playerId = playerId;
            }

            public void Configure(TClient client, int playerId) {
                this.Configure(playerId);
                this.client = client;
            }

            public override bool Equals(object obj) {
                if (obj is NetworkPlayer<TSocket, TClient, TNetClient> player) {
                    return this.Equals(player);
                } else if (obj is INetworkClient<TSocket, TNetClient> client) {
                    return this.client.Equals(client);
                }
                return object.Equals(this, obj);
            }

            public bool Equals(INetworkPlayer<TSocket, TClient, TNetClient> other) {
                return this.playerId == other.playerId;
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
            double lastReceivedPingRequest { get; }

            bool isLocalPlayer { get; }

            void Configure(int playerId, bool isLocalPlayer);
        }

        public class NetworkPlayer<TSocket, TClient, TNetClient> : Server.NetworkPlayer<TSocket, TClient, TNetClient>, INetworkPlayer<TSocket, TClient, TNetClient>
            where TSocket : ISocket
            where TClient : INetworkClient<TSocket, TNetClient>
            where TNetClient : INetClient<TSocket, TNetClient> {

            public bool isLocalPlayer { get; internal set; }
            public double lastReceivedPingRequest { get; internal set; }

            public NetworkPlayer() : base() { this.lastReceivedPingRequest = TimeUtils.CurrentTime(); }

            public void Configure(int playerId, bool isLocalPlayer) {
                this.Configure(playerId);
                this.isLocalPlayer = isLocalPlayer;
            }
        }
    }
}
