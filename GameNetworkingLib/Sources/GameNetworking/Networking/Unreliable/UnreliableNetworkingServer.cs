using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public class UnreliableNetworkingServer : NetworkingServer<UnreliableSocket, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient> {
        public UnreliableNetworkingServer(UnreliableSocket backend) : base(backend) { }
    }
}