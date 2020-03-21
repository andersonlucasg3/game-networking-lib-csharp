using GameNetworking.Commons.Models;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public class UnreliableNetworkingClient : NetworkingClient<UnreliableSocket, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient, UnreliableNetworkingClient.IListener> {
        public interface IListener : INetworkingClient<IUDPSocket, UnreliableNetworkClient, UnreliableNetClient, IListener>.IListener { }

        public UnreliableNetworkingClient(UnreliableSocket backend) : base(backend) { }
    }
}
