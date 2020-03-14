﻿using System;
using Networking;
using Google.Protobuf;

namespace MatchMaking.Connection {
    using Models;
    using Networking.Models;
    using Protobuf.Coders;

    public sealed class ClientConnection<MMClient>: INetworkingListener, INetClientReadListener where MMClient: Client, new() {
        private readonly INetworking networking;

        private MMClient client;
        
        public bool IsConnecting { get; private set; }

        public bool IsConnected { get { return this.client?.IsConnected ?? false; } }

        public IClientConnectionDelegate<MMClient> listener { get; set; }

        public ClientConnection(INetworking networking) {
            this.networking = networking;
        }

        public void Connect(string host, int port) {
            this.IsConnecting = true;
            this.networking.Connect(host, port);
        }

        public void Read() {
            if (this.client?.client != null) {
                this.networking.Read(this.client.client);
            }
        }

        public void Send<Message>(Message message) where Message: IMessage {
            byte[] bytes = this.client?.encoder?.Encode(message);
            if (bytes != null) {
                this.networking.Send(this.client.client, bytes);
            }
        }

        public void Flush() {
            if (this.client?.client != null) {
                this.networking.Flush(this.client.client);
            }
        }

        public void Disconnect() {
            if (this.client?.client != null) {
                this.networking?.Disconnect(this.client.client);
            }
        }

        #region INetClientReadDelegate

        void INetClientReadListener.ClientDidReadBytes(NetClient client, byte[] bytes) {
            this.client.decoder.Add(bytes);
            MessageContainer message = null;
            do {
                message = this.client.decoder.Decode();
                this.listener?.ClientDidReadMessage(message);
            } while (message != null);
        }

        #endregion

        #region INetworkingDelegate

        void INetworkingListener.NetworkingDidConnect(INetClient client) {
            this.client = Client.Create<MMClient>(client, new MessageDecoder(), new MessageEncoder());
            this.IsConnecting = false;
            this.listener?.ClientConnectionDidConnect();
        }

        void INetworkingListener.NetworkingConnectDidTimeout() {
            this.client = null;
            this.IsConnecting = false;
            this.listener?.ClientConnectionDidTimeout();
        }

        void INetworkingListener.NetworkingDidDisconnect(INetClient client) {
            this.client = null;
            this.IsConnecting = false;
            this.listener?.ClientConnectionDidDisconnect();
        }

        #endregion
    }
}