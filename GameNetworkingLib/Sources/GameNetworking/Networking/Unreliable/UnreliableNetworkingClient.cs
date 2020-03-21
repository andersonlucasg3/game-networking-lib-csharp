using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public class UnreliableNetworkingClient : NetworkingClient<UnreliableSocket, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient> {
        public UnreliableNetworkingClient(UnreliableSocket backend) : base(backend) { }
    }
}
