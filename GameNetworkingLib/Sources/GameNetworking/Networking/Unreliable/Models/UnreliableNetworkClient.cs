using GameNetworking.Commons.Models;
using Messages.Streams;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking.Models {
    public class UnreliableNetworkClient : NetworkClient<IUDPSocket, UnreliableNetClient> {
        internal bool isConnected = false;

        public UnreliableNetworkClient(UnreliableNetClient client, IStreamReader reader, IStreamWriter writer) : base(client, reader, writer) { }
    }
}