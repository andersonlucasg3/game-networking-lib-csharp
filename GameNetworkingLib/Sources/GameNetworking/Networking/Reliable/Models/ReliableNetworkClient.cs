using Messages.Streams;
using Networking.Models;
using Networking.Sockets;
using GameNetworking.Commons.Models;

namespace GameNetworking.Networking.Models {
    public class ReliableNetworkClient : NetworkClient<ITCPSocket, ReliableNetClient> {
        public ReliableNetworkClient(ReliableNetClient client, IStreamReader reader, IStreamWriter writer) : base(client, reader, writer) { }
    }
}