using GameNetworking.Commons.Models;
using GameNetworking.Messages.Streams;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking.Models {
    public class ReliableNetworkClient : NetworkClient<ITCPSocket, ReliableNetClient> {
        public ReliableNetworkClient(ReliableNetClient client, IStreamReader reader, IStreamWriter writer) : base(client, reader, writer) { }
    }
}