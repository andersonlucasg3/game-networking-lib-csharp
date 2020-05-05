using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Sockets;
using Logging;

namespace GameNetworking.Channels {
    public enum Channel {
        reliable, unreliable
    }

    #region Reliable

    public interface IReliableChannelListener {
        void ChannelDidReceiveMessage(ReliableChannel channel, MessageContainer container);
    }

    public class ReliableChannel : ITcpSocketIOListener<TcpSocket> {
        private readonly MessageStreamReader reader;
        private readonly MessageStreamWriter writer;

        private readonly TcpSocket socket;

        public IReliableChannelListener listener { get; set; }

        public ReliableChannel(TcpSocket socket) {
            this.socket = socket;
            this.socket.ioListener = this;

            this.reader = new MessageStreamReader();
            this.writer = new MessageStreamWriter();
        }

        public void CloseChannel() => this.socket.Disconnect();

        internal void StartIO() {
            this.Receive();
            this.Flush();
        }

        public void Send(ITypedMessage message) {
            this.writer.Write(message);
        }

        #region Read & Write

        private void Receive() {
            ThreadPool.QueueUserWorkItem(ReceiveTask);
        }

        private void Flush() {
            ThreadPool.QueueUserWorkItem(FlushTask);
        }

        private void ReceiveTask(object stateInfo) {
            while (this.socket.isConnected) { this.socket.Receive(); }
        }

        private void FlushTask(object flushState) {
            while (this.socket.isConnected) {
                var count = this.writer.Put(out byte[] buffer);
                this.socket.Send(buffer, count);
            }
        }

        #endregion

        void ITcpSocketIOListener<TcpSocket>.SocketDidReceiveBytes(TcpSocket socket, byte[] bytes, int count) {
            this.reader.Add(bytes, count);

            MessageContainer container;
            while ((container = this.reader.Decode()) != null) {
                this.listener?.ChannelDidReceiveMessage(this, container);
            }
        }

        void ITcpSocketIOListener<TcpSocket>.SocketDidSendBytes(TcpSocket socket, int count) {
            this.writer.DidWrite(count);
        }
    }

    #endregion

    #region Unreliable

    public interface IUnreliableChannelListener {
        void ChannelDidReceiveMessage(UnreliableChannel channel, MessageContainer container, NetEndPoint from);
    }

    public class UnreliableChannel : IUdpSocketIOListener {
        private readonly ConcurrentDictionary<NetEndPoint, MessageStreamReader> readerCollection;
        private readonly ConcurrentDictionary<NetEndPoint, MessageStreamWriter> writerCollection;
        private readonly List<KeyValuePair<NetEndPoint, MessageStreamWriter>> toWriterCollection;

        private UdpSocket socket;

        public IUnreliableChannelListener listener { get; set; }

        public UnreliableChannel(UdpSocket socket) {
            this.socket = socket;
            this.socket.listener = this;

            this.readerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamReader>();
            this.writerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamWriter>();
            this.toWriterCollection = new List<KeyValuePair<NetEndPoint, MessageStreamWriter>>();
        }

        internal void StartIO() {
            this.Receive();
            this.Flush();
        }

        internal void StopIO() {
            this.socket.Close();
            this.socket = null;
        }

        public void Send(ITypedMessage message, NetEndPoint to) {
            if (!this.writerCollection.TryGetValue(to, out MessageStreamWriter writer)) {
                writer = new MessageStreamWriter();
                this.writerCollection.TryAdd(to, writer);
                lock (this.toWriterCollection) {
                    this.toWriterCollection.Add(new KeyValuePair<NetEndPoint, MessageStreamWriter>(to, writer));
                }
            }
            lock (writer) {
                writer.Write(message);
            }
        }

        #region Read & Write

        private void Receive() {
            ThreadPool.QueueUserWorkItem(ReceiveTask);
        }

        private void Flush() {
            ThreadPool.QueueUserWorkItem(FlushTask);
        }

        private void ReceiveTask(object stateInfo) {
            while (this.socket != null) { this.socket.Receive(); }
        }

        private void FlushTask(object flushState) {
            while (this.socket != null) {
                lock (this.toWriterCollection) {
                    for (int index = 0; index < this.toWriterCollection.Count; index++) {
                        var keyValue = this.toWriterCollection[index];
                        var to = keyValue.Key;
                        var writer = keyValue.Value;
                        lock (writer) {
                            var count = writer.Put(out byte[] buffer);
                            this.socket.Send(buffer, count, to);
                        }
                    }
                }
            }
        }

        #endregion

        void IUdpSocketIOListener.SocketDidReceiveBytes(UdpSocket socket, byte[] bytes, int count, NetEndPoint from) {
            if (!this.readerCollection.TryGetValue(from, out MessageStreamReader reader)) {
                reader = new MessageStreamReader();
                this.readerCollection.TryAdd(from, reader);
            }
            reader.Add(bytes, count);

            MessageContainer container;
            while ((container = reader.Decode()) != null) {
                this.listener?.ChannelDidReceiveMessage(this, container, from);
            }
        }

        void IUdpSocketIOListener.SocketDidWriteBytes(UdpSocket socket, int count, NetEndPoint to) {
            if (this.writerCollection.TryGetValue(to, out MessageStreamWriter writer)) {
                lock (writer) {
                    writer.DidWrite(count);
                }
            } else {
                if (Logger.IsLoggingEnabled) { Logger.Log($"SocketDidWriteBytes did not find writer for endPoint-{to}"); }
            }
        }
    }

    #endregion
}