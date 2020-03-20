using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Networking.IO;
using Networking.Models;

namespace Networking.Sockets {
    public sealed class UDPSocket : ISocket, IDisposable {
        private BaseUDPSocket socket;

        private NetEndPoint bindEndPoint;
        
        public bool isConnected => this.socket.isConnected;
        public bool isBound => this.socket.isBound;
        public bool isAcceptClientSupported => false;

        public UDPSocket() { }

        private UDPSocket(BaseUDPSocket socket) {
            this.socket = socket;
        }

        public void Dispose() {
            this.socket.Dispose();
        }

        #region Server

        public void Accept(Action<ISocket> callback) {
            this.socket.Accept(socket => callback.Invoke(new UDPSocket(this.socket)));
        }

        public void Bind(NetEndPoint endPoint) {
            this.bindEndPoint = endPoint;
        }

        public void Listen(int backlog) {
            this.socket = new UDPServer();
            this.socket.Bind(this.From(this.bindEndPoint));
        }

        #endregion

        public void Close() {
            this.socket.Close();
        }

        public void Connect(NetEndPoint endPoint, Action callback) {
            this.socket = new UDPClient();
            this.socket.Connect(this.From(endPoint));
            callback.Invoke();
        }

        public void Disconnect(Action callback) {
            this.socket.Disconnect(callback);
        }

        public void Read(Action<byte[]> callback) {
            this.socket.Read(callback);
        }

        public void Write(byte[] bytes, Action<int> callback) {
            this.socket.Write(bytes, callback);
        }

        #region Private Methods

        private IPEndPoint From(NetEndPoint ep) {
            return new IPEndPoint(IPAddress.Parse(ep.host), ep.port);
        }

        #endregion
    }

    abstract class BaseUDPSocket : IDisposable {
        class State {
            public byte[] buffer = new byte[bufferSize];
            public EndPoint endPointFrom = new IPEndPoint(IPAddress.Any, 0);
        }

        private readonly Socket socket;
        private const int bufferSize = 8 * 1024;
        private EndPoint remoteEndPoint;

        public readonly bool isConnected = true;
        public bool isBound => this.socket.IsBound;

        protected BaseUDPSocket() {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {
                SendTimeout = 2000,
                ReceiveTimeout = 2000
            };
            this.Configure();
        }

        protected BaseUDPSocket(BaseUDPSocket socket) {
            if (socket.socket.SocketType != SocketType.Dgram) { throw new ArgumentException("The socket param MUST be a Dgram socket"); }
            this.socket = socket.socket;
            this.Configure();
        }

        public void Dispose() {
            if (socket == null) { return; }
            this.socket.Dispose();
        }

        #region Public methods

        public void Bind(EndPoint endPoint) {
            this.socket.Bind(endPoint);
        }

        public abstract void Accept(Action<BaseUDPSocket> callback);

        public virtual void Connect(EndPoint endPoint) {
            this.remoteEndPoint = endPoint;
        }

        public virtual void Disconnect(Action callback) {
            callback.Invoke();
        }

        public void Close() {
            this.socket.Close();
        }

        public virtual void Read(Action<byte[]> callback) {
            var state = new State();
            byte[] buffer = new byte[bufferSize];
            this.socket.BeginReceiveFrom(state.buffer, 0, bufferSize, SocketFlags.None, ref state.endPointFrom, asyncResult => {
                var count = this.socket.EndReceiveFrom(asyncResult, ref state.endPointFrom);
                byte[] shrinked = new byte[count];
                Copy(buffer, ref shrinked);
                this.Read(shrinked, state.endPointFrom as IPEndPoint, callback);
            }, state);
        }

        public virtual void Write(byte[] bytes, Action<int> callback) {
            this.socket.BeginSendTo(bytes, 0, bytes.Length, SocketFlags.None, this.remoteEndPoint, (ar) => {
                int written = this.socket.EndSend(ar);
                callback.Invoke(written);
            }, this);
        }

        #endregion

        #region Private methods

        private void Configure() {
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        }

        private static void Copy(byte[] source, ref byte[] destination) {
            for (var i = 0; i < destination.Length; i++) {
                destination[i] = source[i];
            }
        }

        #endregion

        #region Protected methods 

        protected virtual void Read(byte[] bytes, IPEndPoint ep, Action<byte[]> callback) {
            callback.Invoke(bytes);
        }

        #endregion
    }

    class UDPServer : BaseUDPSocket {
        private readonly Dictionary<string, UDPClient> ipClientAssociation = new Dictionary<string, UDPClient>();
        private readonly Dictionary<UDPClient, string> clientIpAssociation = new Dictionary<UDPClient, string>();

        private Action<BaseUDPSocket> acceptCallback;

        public UDPServer() : base() { }

        public override void Accept(Action<BaseUDPSocket> callback) {
            this.acceptCallback = callback;
        }

        internal void Disassociate(UDPClient client) {
            if (this.clientIpAssociation.TryGetValue(client, out string ip)) {
                this.clientIpAssociation.Remove(client);
                this.ipClientAssociation.Remove(ip);
            }
        }

        protected override void Read(byte[] bytes, IPEndPoint ep, Action<byte[]> callback) {
            if (!this.ipClientAssociation.ContainsKey(ep.Address.ToString())) {
                var client = new UDPClient(this, this);
                string ip = ep.Address.ToString();

                this.ipClientAssociation[ip] = client;
                this.clientIpAssociation[client] = ip;

                this.acceptCallback.Invoke(client);
            }

            base.Read(bytes, ep, callback);
        }
    }

    class UDPClient : BaseUDPSocket {
        private readonly UDPServer associatedServer = null;

        public UDPClient() : base() { }

        public UDPClient(BaseUDPSocket socket, UDPServer associatedServer) : base(socket) {
            this.associatedServer = associatedServer;
        }

        public override void Connect(EndPoint endPoint) {
            base.Connect(endPoint);
            this.Bind(endPoint);
        }

        public override void Disconnect(Action callback) {
            this.associatedServer.Disassociate(this);

            base.Disconnect(callback);
        }

        public override void Accept(Action<BaseUDPSocket> callback) {
            throw new NotImplementedException();
        }
    }
}