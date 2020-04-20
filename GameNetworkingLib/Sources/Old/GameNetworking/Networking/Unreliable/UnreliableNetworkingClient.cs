using GameNetworking.Messages.Client;
using GameNetworking.Messages.Streams;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using GameNetworking.Sockets;
using Networking.Commons.Models;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public interface IUnreliableNetworkingClientListener : INetworkingClientListener {
        void UnreliableNetworkingClientDidConnect();
        void UnreliableNetworkingClientConnectDidTimeout();
        void UnreliableNetworkingClientDidDisconnect();
    }

    public class UnreliableNetworkingClient : NetworkingClient<UnreliableSocket, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, IUnreliableClientMessageSender, UnreliableSocket.IListener {
        internal readonly UnreliableClientConnectionController clientConnectionController;

        public new IUnreliableNetworkingClientListener listener { get => base.listener as IUnreliableNetworkingClientListener; set => base.listener = value; }

        public UnreliableNetworkingClient(UnreliableSocket backend) : base(backend) {
            backend.listener = this;

            this.clientConnectionController = new UnreliableClientConnectionController(this, this.ConnectionDidTimeOut);

            this.client = new UnreliableNetworkClient(new UnreliableNetClient(this.networking.socket), new MessageStreamReader(), new MessageStreamWriter());
        }

        public void Start(string host, int port) {
            this.networking.Start(host, port);
        }

        public void Connect(string host, int port) {
            this.networking.BindToRemote(new NetEndPoint(host, port));

            this.clientConnectionController.Connect();
        }

        public void Disconnect() {
            var disconnect = new UnreliableDisconnectMessage();
            this.Send(disconnect);
            this.Send(disconnect);
        }

        public override void Update() {
            this.clientConnectionController.Update();

            this.networking.Read();
            this.networking.Flush(this.client.client);
        }

        void UnreliableSocket.IListener.SocketDidRead(byte[] bytes, int count, UnreliableNetClient client) {
            var listener = this as INetClientListener<IUDPSocket, UnreliableNetClient>;
            listener.ClientDidReadBytes(client, bytes, count);
        }

        private void ConnectionDidTimeOut() {
            this.Close();
            this.listener?.UnreliableNetworkingClientConnectDidTimeout();
        }

        internal void DidConnect() {
            if (this.clientConnectionController.isConnecting) {
                this.clientConnectionController.Stop();
            }
            this.listener?.UnreliableNetworkingClientDidConnect();
        }

        internal void DidDisconnect() {
            this.listener?.UnreliableNetworkingClientDidDisconnect();
        }
    }
}
