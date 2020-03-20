using System;
using Networking.Commons.Models;
using Networking.Commons.Sockets;
using GameNetworking.Commons.Models.Contract.Server;

namespace GameNetworking.Commons.Models {
    namespace Contract {
        namespace Server {
            public interface INetworkPlayer<TSocket, TClient, TNetClient> : IEquatable<INetworkPlayer<TSocket, TClient, TNetClient>> 
                where TSocket : ISocket
                where TClient : INetworkClient<TSocket, TNetClient>
                where TNetClient : INetClient<TSocket, TNetClient> {
                TClient client { get; internal set; }
                int playerId { get; internal set; }
                float mostRecentPingValue { get; internal set; }
            }
        }
        namespace Client {
            public interface INetworkPlayer<TSocket, TClient, TNetClient> : Server.INetworkPlayer<TSocket, TClient, TNetClient>
                where TSocket : ISocket
                where TClient : INetworkClient<TSocket, TNetClient>
                where TNetClient : INetClient<TSocket, TNetClient> {
                bool isLocalPlayer { get; internal set; }
            }
        }
    }

    namespace Server {
        public class NetworkPlayer<TSocket, TClient, TNetClient> : INetworkPlayer<TSocket, TClient, TNetClient>
            where TSocket : ISocket
            where TClient : INetworkClient<TSocket, TNetClient>
            where TNetClient : INetClient<TSocket, TNetClient> {
            private INetworkPlayer<TSocket, TClient, TNetClient> self => this;

            TClient INetworkPlayer<TSocket, TClient, TNetClient>.client { get; set; }
            int INetworkPlayer<TSocket, TClient, TNetClient>.playerId { get; set; }
            float INetworkPlayer<TSocket, TClient, TNetClient>.mostRecentPingValue { get; set; }

            public TClient client => self.client;
            public int playerId => self.playerId;
            public float mostRecentPingValue { get => self.mostRecentPingValue; internal set => self.mostRecentPingValue = value; }

            public NetworkPlayer() { }

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
        public class NetworkPlayer<TSocket, TClient, TNetClient> : Server.NetworkPlayer<TSocket, TClient, TNetClient>, Contract.Client.INetworkPlayer<TSocket, TClient, TNetClient>
            where TSocket : ISocket
            where TClient : INetworkClient<TSocket, TNetClient>
            where TNetClient : INetClient<TSocket, TNetClient> {
            private Contract.Client.INetworkPlayer<TSocket, TClient, TNetClient> self => this;

            bool Contract.Client.INetworkPlayer<TSocket, TClient, TNetClient>.isLocalPlayer { get; set; }

            public bool isLocalPlayer { get => self.isLocalPlayer; }
        }
    }
}
