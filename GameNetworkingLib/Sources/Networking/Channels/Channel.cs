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

    public interface IUnreliableChannelListener {
        void UnreliableChannelDidReceiveMessage(MessageContainer container, NetEndPoint from);
    }

    public interface IUnreliableChannelIdentifiedReceiveListener {
        void ChannelDidReceiveMessage(MessageContainer container);
    }

    public class UnreliableChannel : Channel<UdpSocket>, IUnreliableChannelIdentifiedReceiveListener {
        private readonly Dictionary<NetEndPoint, IUnreliableChannelIdentifiedReceiveListener> receiverCollection
            = new Dictionary<NetEndPoint, IUnreliableChannelIdentifiedReceiveListener>();
        public IUnreliableChannelListener serverListener { get; set; }

        public UnreliableChannel(UdpSocket socket) : base(socket) { }

        public void Register(NetEndPoint remoteEndPoint, IUnreliableChannelIdentifiedReceiveListener listener) {
            this.receiverCollection[remoteEndPoint] = listener;
        }

        public void Unregister(NetEndPoint endPoint) => this.receiverCollection.Remove(endPoint);

        public void SetRemote(NetEndPoint endPoint) {
            this.socket.Connect(endPoint);
        }

        protected override void ChannelDidReceiveMessage(MessageContainer container, UdpSocket from) {
            if (this.receiverCollection.TryGetValue(from.remoteEndPoint, out IUnreliableChannelIdentifiedReceiveListener listener)) {
                listener?.ChannelDidReceiveMessage(container);
            } else {
                this.serverListener?.UnreliableChannelDidReceiveMessage(container, from.remoteEndPoint);
            }
        }

        void IUnreliableChannelIdentifiedReceiveListener.ChannelDidReceiveMessage(MessageContainer container) {
            this.listener?.ChannelDidReceiveMessage(container);
        }
    }

    #endregion
}