using GameNetworking.Models;
using Messages.Streams;
using Networking.Sockets;

namespace GameNetworking.Networking.Models {
    public class UnreliableNetworkClient : NetworkClient<ITCPSocket, UnreliableNetClient> {
        public UnreliableNetworkClient(UnreliableNetClient client, IStreamReader reader, IStreamWriter writer) : base(client, reader, writer) { }
    }
}