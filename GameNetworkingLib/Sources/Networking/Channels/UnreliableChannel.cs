using System.Collections.Concurrent;
using System.Threading;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Networking.Sockets;
using Logging;

namespace GameNetworking.Channels {
    public interface IUnreliableChannelListener {
        void ChannelDidReceiveMessage(UnreliableChannel channel, MessageContainer container, NetEndPoint from);
    }

    public class UnreliableChannel : IUdpSocketIOListener {
        private readonly ConcurrentDictionary<NetEndPoint, MessageStreamReader> readerCollection;
        private readonly ConcurrentDictionary<NetEndPoint, MessageStreamWriter> writerCollection;
        private readonly ConcurrentQueue<SendInfo> sendInfoCollection;

        private UdpSocket socket;

        public IUnreliableChannelListener listener { get; set; }

        public UnreliableChannel(UdpSocket socket) {
            this.socket = socket;
            this.socket.listener = this;

            this.readerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamReader>();
            this.writerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamWriter>();
            this.sendInfoCollection = new ConcurrentQueue<SendInfo>();
        }

        internal void StartIO() {
            ThreadPool.QueueUserWorkItem(_ => {
                bool shouldRun = true;

                NetEndPoint to = new NetEndPoint();
                void SendTo(byte[] bytes, int count) {
                    this.socket.Send(bytes, count, to);
                }

                do {
                    lock (this) { shouldRun = this.socket != null; }

                    this.socket.Receive();

                    if (this.sendInfoCollection.TryDequeue(out SendInfo info)) {
                        var message = info.message;
                        to = info.to;

                        if (!this.writerCollection.TryGetValue(to, out MessageStreamWriter writer)) {
                            writer = new MessageStreamWriter();
                            this.writerCollection.TryAdd(to, writer);
                        }
                        writer.Write(message);
                    }

                    var values = this.writerCollection.GetEnumerator();
                    while (values.MoveNext()) {
                        var keyValue = values.Current;
                        var writer = keyValue.Value;
                        to = keyValue.Key;
                        writer.Use(SendTo);
                    }

                } while (shouldRun);
            });
        }

        internal void StopIO() {
            lock (this) {
                this.socket.Close();
                this.socket = null;
            }
        }

        public void Send(ITypedMessage message, NetEndPoint to) {
            this.sendInfoCollection.Enqueue(new SendInfo { message = message, to = to });
        }

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
                writer.DidWrite(count);
            } else {
                if (Logger.IsLoggingEnabled) { Logger.Log($"SocketDidWriteBytes did not find writer for endPoint-{to}"); }
            }
        }

        private struct SendInfo {
            public ITypedMessage message;
            public NetEndPoint to;
        }
    }
}