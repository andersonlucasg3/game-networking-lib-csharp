using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using GameNetworking.Commons;
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
        private readonly List<NetEndPoint> netEndPointWriters;
        private readonly ConcurrentQueue<SendInfo> sendInfoCollection;
        private readonly object lockToken = new object();

        private UdpSocket socket;

        public IUnreliableChannelListener listener { get; set; }

        public UnreliableChannel(UdpSocket socket) {
            this.socket = socket;
            this.socket.listener = this;

            this.readerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamReader>();
            this.writerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamWriter>();
            this.netEndPointWriters = new List<NetEndPoint>();
            this.sendInfoCollection = new ConcurrentQueue<SendInfo>();
        }

        internal void StartIO() {
            ThreadPool.QueueUserWorkItem(ThreadPoolWorker);
        }

        internal void StopIO() {
            lock (this) {
                this.socket.Close();
                this.socket = null;
            }
        }

        public void CloseChannel(NetEndPoint endPoint) {
            this.writerCollection.TryRemove(endPoint, out _);
            lock (this.lockToken) {
                this.netEndPointWriters.Remove(endPoint);
            }
        }

        public void Send(ITypedMessage message, NetEndPoint to) {
            ThreadChecker.AssertMainThread();

            this.sendInfoCollection.Enqueue(new SendInfo { message = message, to = to });
        }

        private void ThreadPoolWorker(object state) {
            Thread.CurrentThread.Name = "UnreliableChannel Thread";
            ThreadChecker.ConfigureUnreliable(Thread.CurrentThread);
            bool shouldRun = true;

            NetEndPoint to = new NetEndPoint();
            void SendTo(byte[] bytes, int count) {
                this.socket.Send(bytes, count, to);
            }

            NetEndPoint[] endPoints = new NetEndPoint[100];
            int endPointCount = 0;

            do {
                lock (this) { shouldRun = this.socket != null; }

                try {
                    this.socket.Receive();

                    if (this.sendInfoCollection.TryDequeue(out SendInfo info)) {
                        var message = info.message;
                        to = info.to;

                        if (!this.writerCollection.TryGetValue(to, out MessageStreamWriter writer)) {
                            writer = new MessageStreamWriter();
                            this.writerCollection.TryAdd(to, writer);
                            lock (this.lockToken) {
                                this.netEndPointWriters.Add(to);
                            }
                        }
                        writer.Write(message);
                    }

                    lock (this.lockToken) {
                        this.netEndPointWriters.CopyTo(endPoints);
                        endPointCount = this.netEndPointWriters.Count;
                    }

                    for (int index = 0; index < endPointCount; index++) {
                        to = endPoints[index];
                        var writer = this.writerCollection[to];
                        writer.Use(SendTo);
                    }
                } catch (ObjectDisposedException) {
                    shouldRun = false;
                } catch (Exception ex) {
                    Logger.Log($"Exception thrown in ThreadPool\n{ex}");
                }
            } while (shouldRun);

            Logger.Log("UnreliableChannel ThreadPool EXITING");
            ThreadChecker.ConfigureUnreliable(null);
        }

        void IUdpSocketIOListener.SocketDidReceiveBytes(UdpSocket socket, byte[] bytes, int count, NetEndPoint from) {
            ThreadChecker.AssertUnreliableChannel();

            if (!this.readerCollection.TryGetValue(from, out MessageStreamReader reader)) {
                reader = new MessageStreamReader();
                this.readerCollection.TryAdd(from, reader);
            }
            reader.Add(bytes, count);

            MessageContainer? container;
            while ((container = reader.Decode()) != null) {
                this.listener?.ChannelDidReceiveMessage(this, container.Value, from);
            }
        }

        void IUdpSocketIOListener.SocketDidWriteBytes(UdpSocket socket, int count, NetEndPoint to) {
            ThreadChecker.AssertUnreliableChannel();

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