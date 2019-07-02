using Networking;
using Networking.Models;
using Messages.Streams;
using Messages.Models;
using Messages.Coders;
using System;
using System.Collections.Generic;

namespace GameNetworking.Networking {
    using Models;

    internal class NetworkingServer {
        private readonly INetworking networking;

        private WeakReference weakDelegate;
        private WeakReference weakMessagesDelegate;

        public INetworkingServerDelegate Delegate {
            get { return this.weakDelegate?.Target as INetworkingServerDelegate; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public INetworkingServerMessagesDelegate MessagesDelegate {
            get { return this.weakMessagesDelegate?.Target as INetworkingServerMessagesDelegate; }
            set { this.weakMessagesDelegate = new WeakReference(value); }
        }

        public NetworkingServer() {
            this.networking = new NetSocket();
        }

        public void Listen(int port) {
            this.networking.Start(port);
        }

        public void AcceptClient() {
            NetClient client = this.networking.Accept();
            if (client != null) {
                NetworkClient networkClient = new NetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
                this.Delegate?.NetworkingServerDidAcceptClient(networkClient);
            }
        }

        public void Read(NetworkClient client) {
            byte[] bytes = this.networking.Read(client.Client);
            client.Reader.Add(bytes);
            var container = client.Reader.Decode();
            if (container != null) { this.MessagesDelegate?.NetworkingServerDidReadMessage(container, client); }
        }

        public void Send(ITypedMessage encodable, NetworkClient client) {
            client.Write(encodable);
        }

        public void SendBroadcast(ITypedMessage encodable, List<NetworkClient> clients) {
            var writer = new MessageStreamWriter();
            var buffer = writer.Write(encodable);
            clients.ForEach(c => this.networking.Send(c.Client, buffer));
        }

        public void Flush(NetworkClient client) {
            if (client.Client.IsConnected) {
                this.networking.Flush(client.Client);
            } else {
                this.Delegate?.NetworkingServerClientDidDisconnect(client);
            }
        }
    }
}
