using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Networking.Sockets;
using System;
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
            ThreadPool.QueueUserWorkItem(ThreadPoolWorker);
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
            Logging.Logger.Log($"MessageStreamWriter {this.writer.currentBufferLength}");
        }

        private static void ThreadPoolWorker(object state) {
            Thread.CurrentThread.Name = "ReliableChannel Thread";
            ReliableChannel[] channels = new ReliableChannel[100];
            int channelCount = 0;
            do {
                lock (socketLock) {
                    aliveSockets.CopyTo(channels);
                    channelCount = aliveSockets.Count;
                }

                for (int index = 0; index < channelCount; index++) {
                    var channel = channels[index];
                    try {
                        channel.socket.Receive();
                        if (channel.reader.currentBufferLength > 0) {
                            Logging.Logger.Log($"MessageStreamReader {channel.reader.currentBufferLength}");
                        }
                        if (channel.writer.currentBufferLength > 0) {
                            Logging.Logger.Log($"MessageStreamWriter {channel.writer.currentBufferLength}");
                        }
                        channel.writer.Use(channel.socket.Send);
                    } catch (ObjectDisposedException) {
                        ioRunning = false;
                    } catch (Exception ex) {
                        Logging.Logger.Log($"Exception thrown in ThreadPool\n{ex}");
                    }
                }
            } while (ioRunning);
            Logging.Logger.Log("ReliableChannel ThreadPool EXITING");
        }

        void ITcpSocketIOListener<TcpSocket>.SocketDidReceiveBytes(TcpSocket socket, byte[] bytes, int count) {
            this.reader.Add(bytes, count);

            Logging.Logger.Log($"Received bytes {count}");

            MessageContainer? container;
            while ((container = this.reader.Decode()).HasValue) {
                this.listener?.ChannelDidReceiveMessage(this, container.Value);
            }
        }

        void ITcpSocketIOListener<TcpSocket>.SocketDidSendBytes(TcpSocket socket, int count) {
            this.writer.DidWrite(count);
            Logging.Logger.Log($"Written {count} bytes");
        }
    }
}