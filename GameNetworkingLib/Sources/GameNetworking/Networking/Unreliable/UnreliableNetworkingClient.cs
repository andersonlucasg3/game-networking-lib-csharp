using GameNetworking.Messages.Client;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Networking.Commons.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public class UnreliableNetworkingClient : NetworkingClient<UnreliableSocket, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient> {

        public UnreliableNetworkingClient(UnreliableSocket backend) : base(backend) { }

        public void Connect(string host, int port) {
            this.networking.Start(port);
            this.networking.BindToRemote(new NetEndPoint(host, port));

            this.Send(new UnreliableConnectMessage());
        }
    }
}
