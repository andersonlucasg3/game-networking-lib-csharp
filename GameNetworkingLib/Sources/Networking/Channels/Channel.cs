using System.Collections.Generic;
using System.Threading;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Sockets;

namespace GameNetworking.Channels {
    public interface IChannelListener {
        void ChannelDidReceiveMessage(MessageContainer container, NetEndPoint from);
    }

    public interface IChannel {
        IChannelListener listener { get; set; }

        void Send(ITypedMessage message);
    }

    public abstract class Channel<TSocket> : IChannel, ISocketListener<TSocket>
        where TSocket : ISocket<TSocket> {
        private readonly MessageStreamReader reader;
        private readonly MessageStreamWriter writer;

        protected readonly TSocket socket;

        public IChannelListener listener { get; set; }

        public Channel(TSocket socket) {
            this.socket = socket;
            this.socket.listener = this;

            this.reader = new MessageStreamReader();
            this.writer = new MessageStreamWriter();
        }

        internal abstract void StartIO();

        public void Send(ITypedMessage message) {
            this.writer.Write(message);
        }

        #region Read & Write

        protected virtual void Receive() {
            ThreadPool.QueueUserWorkItem(ReceiveTask);
        }

        protected virtual void Flush() {
            ThreadPool.QueueUserWorkItem(FlushTask);
        }

        private void ReceiveTask(object stateInfo) {
            this.socket.Receive();

            this.Receive();
        }

        private void FlushTask(object flushState) {
            var count = this.writer.Put(out byte[] buffer);
            this.socket.Send(buffer, count);
        }

        #endregion

        protected virtual void ChannelDidReceiveMessage(MessageContainer container, TSocket from) {
            this.listener?.ChannelDidReceiveMessage(container, from.remoteEndPoint);
        }

        void ISocketListener<TSocket>.SocketDidReceiveBytes(TSocket socket, byte[] bytes, int count) {
            this.reader.Add(bytes, count);

            MessageContainer container;
            while ((container = this.reader.Decode()) != null) { this.ChannelDidReceiveMessage(container, socket); }
        }

        void ISocketListener<TSocket>.SocketDidSendBytes(TSocket socket, int count) {
            this.writer.DidWrite(count);

            this.Flush();
        }
    }

    public enum Channel {
        reliable, unreliable
    }

    #region Reliable

    public class ReliableChannel : Channel<TcpSocket> {
        public ReliableChannel(TcpSocket socket) : base(socket) { }

        internal override void StartIO() {
            this.Receive();
            this.Flush();
        }

        public void CloseChannel() => this.socket.Disconnect();
    }

    #endregion

    #region Unreliable

    public class UnreliableChannel : Channel<UdpSocket>, IChannelListener {
        private readonly Dictionary<NetEndPoint, IChannelListener> receiverCollection
            = new Dictionary<NetEndPoint, IChannelListener>();
        internal bool isServer = false;

        public UnreliableChannel(UdpSocket socket) : base(socket) {
            this.socket.listener = this;
        }

        internal override void StartIO() {
            this.StartIO(true, true, 4);
        }

        internal void StartIO(bool input, bool output, int count) {
            for (int index = 0; index < count; index++) {
                if (input) { this.Receive(); }
                if (output) { this.Flush(); }
            }
        }

        public void Register(NetEndPoint remoteEndPoint, IChannelListener listener) {
            this.receiverCollection[remoteEndPoint] = listener;
        }

        public void Unregister(NetEndPoint endPoint) => this.receiverCollection.Remove(endPoint);

        public void SetRemote(NetEndPoint endPoint) {
            this.socket.Connect(endPoint);
        }

        protected override void ChannelDidReceiveMessage(MessageContainer container, UdpSocket from) {
            ((IChannelListener)this).ChannelDidReceiveMessage(container, from.remoteEndPoint);
        }

        void IChannelListener.ChannelDidReceiveMessage(MessageContainer container, NetEndPoint from) {
            if (!this.isServer) {
                this.listener?.ChannelDidReceiveMessage(container, from);
                return;
            }

            if (this.receiverCollection.TryGetValue(from, out IChannelListener listener)) {
                listener?.ChannelDidReceiveMessage(container, from);
            } else {
                this.listener?.ChannelDidReceiveMessage(container, from);
            }
        }
    }

    #endregion
}