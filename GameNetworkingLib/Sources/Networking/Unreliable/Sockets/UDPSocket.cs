using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace Networking.Sockets {
    public interface IUDPSocket : ISocket {
        void BindToRemote(NetEndPoint endPoint);

        void Close();

        void Read(Action<byte[], IUDPSocket> callback);
    }

    public sealed class UDPSocket : IUDPSocket, IDisposable {
        private readonly Dictionary<EndPoint, UDPSocket> instantiatedEndPointSockets;
        private UdpClient client;
        private IPEndPoint remoteEndPoint;

        public bool isBound => this.client.Client.IsBound;
        public bool isCommunicable { get; private set; }

        public UDPSocket() {
            this.instantiatedEndPointSockets = new Dictionary<EndPoint, UDPSocket>();
        }

        private UDPSocket(UdpClient client, IPEndPoint remoteEndPoint) : this() {
            this.client = client;

            this.remoteEndPoint = remoteEndPoint;
            this.isCommunicable = true;
        }

        public void Dispose() {
            this.client.Dispose();
        }

        public void Close() {
            this.client.Close();
        }

        public void Bind(NetEndPoint endPoint) {
            var ipep = this.From(endPoint);
            this.client = new UdpClient() { ExclusiveAddressUse = false };
            this.client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this.client.Client.Bind(ipep);
            this.isCommunicable = true;
        }

        public void BindToRemote(NetEndPoint endPoint) {
            this.remoteEndPoint = this.From(endPoint);
            this.isCommunicable = true;
        }

        public void Read(Action<byte[], IUDPSocket> callback) {
            this.client.BeginReceive(ar => {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                var receivedBytes = this.client.EndReceive(ar, ref endPoint);
                
                if (this.instantiatedEndPointSockets.TryGetValue(endPoint, out UDPSocket value)) {
                    callback.Invoke(receivedBytes, value);
                } else {
                    var socket = new UDPSocket(this.client, endPoint);
                    this.instantiatedEndPointSockets[endPoint] = socket;
                    callback.Invoke(receivedBytes, socket);
                }
            }, null);
        }

        public void Write(byte[] bytes, Action<int> callback) {
            this.client.BeginSend(bytes, bytes.Length, this.remoteEndPoint, ar => {
                var writtenCount = this.client.EndSend(ar);
                callback.Invoke(writtenCount);
            }, null);
        }

        public override string ToString() {
            return $"{{EndPoint-{this.remoteEndPoint}}}";
        }

        #region Private Methods

        private IPEndPoint From(NetEndPoint ep) {
            return new IPEndPoint(IPAddress.Parse(ep.host), ep.port);
        }

        #endregion
    }
}