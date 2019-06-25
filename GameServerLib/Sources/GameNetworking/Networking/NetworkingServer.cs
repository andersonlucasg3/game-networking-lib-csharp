using Networking;
using Networking.Models;
using Messages.Streams;
using Messages.Models;
using Messages.Coders;
using System;

namespace GameNetworking.Networking {
    using Models;

    internal class NetworkingServer {
        private readonly INetworking networking;

        private WeakReference weakDelegate;

        public INetworkingServerDelegate Delegate {
            get { return this.weakDelegate?.Target as INetworkingServerDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public NetworkingServer() {
            this.networking = new NetSocket();
        }

        public void Listen(int port) {
            this.networking.Start(port);
        }

        public void AcceptClient() {
            Client client = this.networking.Accept();
            if (client != null) {
                NetworkClient networkClient = new NetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
                this.Delegate?.NetworkingServerDidAcceptClient(networkClient);
            }
        }

        public void Read(NetworkClient client) {
            byte[] bytes = this.networking.Read(client.Client);
            client.Reader.Add(bytes);
            var container = client.Reader.Decode();
            if (container != null) { this.Delegate?.NetworkingServerDidReadMessage(container, client); }
        }

        public void Send(IEncodable encodable, NetworkClient client) {
            client.Writer.Write(encodable);
        }

        public void Flush(NetworkClient client) {
            this.networking.Flush(client.Client);
        }
    }
}
