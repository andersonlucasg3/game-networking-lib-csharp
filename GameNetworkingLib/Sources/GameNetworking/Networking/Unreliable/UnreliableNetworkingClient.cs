using System.Collections;
using GameNetworking.Networking.Commons;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public class UnreliableNetworkingClient :
        NetworkingClient<UnreliableSocket, IUDPSocket, UnreliableNetworkingClient, UnreliableNetClient, UnreliableNetworkingClient.IListener> {
        public interface IListener : NetworkingClient<UnreliableSocket, IUDPSocket, UnreliableNetworkingClient, UnreliableNetClient, IListener> {

        }
    }
}
