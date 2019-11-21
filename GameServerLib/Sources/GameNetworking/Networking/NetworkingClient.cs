using Networking;
using Networking.Models;
using Messages.Coders;
using Messages.Models;
using Messages.Streams;
using Commons;
using System;
using System.Threading;
using System.Collections.Generic;

namespace GameNetworking.Networking {
    using Models;

    internal class NetworkingClient : WeakDelegate<INetworkingClientDelegate>, INetworkingDelegate, INetClientReadDelegate {
        private INetworking networking;
        private NetworkClient client;

        public NetworkingClient() {
            this.networking = new NetSocket();
            this.networking.Delegate = this;
        }

        public void Connect(string host, int port) {
            this.networking.Connect(host, port);
        }

        public void Disconnect() {
            if (this.client?.Client != null) {
                this.networking.Disconnect(this.client.Client);
            }
        }

        public void Send(ITypedMessage message) {
            this.client?.Write(message);
        }

        public void Update() {
            this.networking.Read(this.client.Client);
            this.networking.Flush(this.client.Client);
        }

        #region INetClientReadDelegate

        void INetClientReadDelegate.ClientDidReadBytes(NetClient client, byte[] bytes) {
            this.client.Reader.Add(bytes);
            MessageContainer container = null;
            do {
                container = this.client.Reader.Decode();
                this.Delegate?.NetworkingClientDidReadMessage(container);
            } while (container != null);
        }

        #endregion

        #region INetworkingDelegate

        void INetworkingDelegate.NetworkingDidConnect(NetClient client) {
            client.Delegate = this;

            this.client = new NetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
            this.Delegate?.NetworkingClientDidConnect();
        }

        void INetworkingDelegate.NetworkingConnectDidTimeout() {
            this.client = null;
            this.Delegate?.NetworkingClientConnectDidTimeout();
        }

        void INetworkingDelegate.NetworkingDidDisconnect(NetClient client) {
            this.client = null;
            this.Delegate?.NetworkingClientDidDisconnect();
        }

        #endregion
    }
}
