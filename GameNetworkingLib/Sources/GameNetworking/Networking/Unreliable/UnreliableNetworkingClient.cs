using GameNetworking.Messages.Client;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Networking.Commons.Models;
using Networking.Models;
using Networking.Sockets;
using Messages.Streams;
using Networking.Commons;

namespace GameNetworking.Networking {
    public class UnreliableNetworkingClient : NetworkingClient<UnreliableSocket, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, UnreliableSocket.IListener {
        public UnreliableNetworkingClient(UnreliableSocket backend) : base(backend) {
            backend.listener = this;

            this.client = new UnreliableNetworkClient(new UnreliableNetClient(this.networking.socket), new MessageStreamReader(), new MessageStreamWriter());
        }

        public void Start(string host, int port) {
            this.networking.Start(host, port);
        }

        public void Connect(string host, int port) {
            this.networking.BindToRemote(new NetEndPoint(host, port));
        }

        public override void Update() {
            this.networking.Read();
            base.Update();
        }

        void UnreliableSocket.IListener.SocketDidRead(byte[] bytes, UnreliableNetClient client) {
            (this as INetClient<IUDPSocket, UnreliableNetClient>.IListener).ClientDidReadBytes(client, bytes);
        }
    }
}
