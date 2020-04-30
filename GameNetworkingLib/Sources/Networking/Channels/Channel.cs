using System.Collections.Generic;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Sockets;

namespace GameNetworking.Channels {
    public interface IChannelListener {
        void ChannelDidReceiveMessage(MessageContainer container);
    }

    public interface IChannel {
        IChannelListener listener { get; set; }

        void Send(ITypedMessage message);
        void Flush();
    }

    public abstract class Channel<TSocket> : IChannel, ISocketListener<TSocket>
        where TSocket : ISocket<TSocket> {
        private readonly MessageStreamReader reader;
        private readonly MessageStreamWriter writer;
        private bool isSending = false;

        protected readonly TSocket socket;

        public IChannelListener listener { get; set; }

        public Channel(TSocket socket) {
            this.socket = socket;
            this.socket.listener = this;

            this.reader = new MessageStreamReader();
            this.writer = new MessageStreamWriter();
        }

        public void Send(ITypedMessage message) {
            this.writer.Write(message);
            this.Flush();
        }

        public void Flush() {
            lock (this.writer) {
                if (this.isSending || !this.writer.hasBytesToWrite) { return; }
                this.isSending = true;

                var count = this.writer.Put(out byte[] buffer);
                this.socket.Send(buffer, count);
            }
        }

        protected virtual void ChannelDidReceiveMessage(MessageContainer container, TSocket from) {
            this.listener?.ChannelDidReceiveMessage(container);
        }

        private void Add(byte[] bytes, int count) { lock (this.reader) { this.reader.Add(bytes, count); } }
        private MessageContainer Decode() { lock (this.reader) { return this.reader.Decode(); } }

        void ISocketListener<TSocket>.SocketDidReceiveBytes(TSocket socket, byte[] bytes, int count) {
            this.Add(bytes, count);

            MessageContainer container;
            while ((container = this.Decode()) != null) { this.ChannelDidReceiveMessage(container, socket); }
        }

        void ISocketListener<TSocket>.SocketDidSendBytes(TSocket socket, int count) {
            this.writer.DidWrite(count);

            lock (this.writer) { this.isSending = false; }
        }
    }

    public enum Channel {
        reliable, unreliable
    }

    #region Reliable

    public class ReliableChannel : Channel<TcpSocket> {
        public ReliableChannel(TcpSocket socket) : base(socket) { }

        public void CloseChannel() => this.socket.Disconnect();
    }

    #endregion

    #region Unreliable

    public interface IUnreliableChannelIdentifiedReceiveListener {
        void ChannelDidReceiveMessage(MessageContainer container);
    }

    public class UnreliableChannel : Channel<UdpSocket>, IUnreliableChannelIdentifiedReceiveListener {
        private readonly Dictionary<NetEndPoint, IUnreliableChannelIdentifiedReceiveListener> receiverCollection
            = new Dictionary<NetEndPoint, IUnreliableChannelIdentifiedReceiveListener>();
        private bool isServer = false;

        public UnreliableChannel(UdpSocket socket) : base(socket) { }

        public void Register(int remoteRealPort, IUnreliableChannelIdentifiedReceiveListener listener) {
            this.isServer = true;
            var remoteEndPoint = new NetEndPoint(this.socket.remoteEndPoint.host, remoteRealPort);
            this.receiverCollection[remoteEndPoint] = listener;
            this.socket.Connect(remoteEndPoint);
        }

        public void Unregister(NetEndPoint endPoint) => this.receiverCollection.Remove(endPoint);

        protected override void ChannelDidReceiveMessage(MessageContainer container, UdpSocket from) {
            if (!this.isServer) {
                this.listener?.ChannelDidReceiveMessage(container);
                return;
            }

            if (this.receiverCollection.TryGetValue(from.remoteEndPoint, out IUnreliableChannelIdentifiedReceiveListener listener)) {
                listener?.ChannelDidReceiveMessage(container);
            }
        }

        void IUnreliableChannelIdentifiedReceiveListener.ChannelDidReceiveMessage(MessageContainer container) {
            this.listener?.ChannelDidReceiveMessage(container);
        }
    }

    #endregion
}