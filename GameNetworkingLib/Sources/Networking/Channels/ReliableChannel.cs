using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Networking.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace GameNetworking.Channels {
    public interface IReliableChannelListener {
        void ChannelDidReceiveMessage(ReliableChannel channel, MessageContainer container);
    }

    public class ReliableChannel : ITcpSocketIOListener<TcpSocket> {
        private static bool ioRunning = false;
        private static readonly List<ReliableChannel> aliveSockets = new List<ReliableChannel>();
        private static readonly object socketLock = new object();

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

        public static void StartIO() {
            ioRunning = true;
            ThreadPool.QueueUserWorkItem(_ => {
                ReliableChannel[] channels;
                do {
                    lock (socketLock) {
                        channels = aliveSockets.ToArray();
                    }

                    for (int index = 0; index < channels.Length; index++) {
                        var channel = channels[index];
                        channel.socket.Receive();
                        channel.writer.Use(channel.socket.Send);
                    }
                } while (ioRunning);
            });
        }

        public static void StopIO() {
            ioRunning = false;
        }

        public static void Add(ReliableChannel channel) {
            lock (socketLock) { aliveSockets.Add(channel); }
        }

        public static void Remove(ReliableChannel channel) {
            lock (socketLock) { aliveSockets.Remove(channel); }
        }

        public void CloseChannel() {
            this.socket.Disconnect();
        }

        public void Send(ITypedMessage message) {
            this.writer.Write(message);
        }

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
}