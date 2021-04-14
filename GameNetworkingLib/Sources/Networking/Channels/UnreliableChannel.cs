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

            readerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamReader>();
            writerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamWriter>();
            netEndPointWriters = new List<NetEndPoint>();
            sendInfoCollection = new ConcurrentQueue<SendInfo>();
        }

        internal void StartIO() {
            ThreadPool.QueueUserWorkItem(ThreadPoolWorker);
        }

        internal void StopIO() {
            lock (this) {
                socket.Close();
                socket = null;
            }
        }

        public void CloseChannel(NetEndPoint endPoint) {
            writerCollection.TryRemove(endPoint, out _);
            lock (lockToken) {
                netEndPointWriters.Remove(endPoint);
            }
        }

        public void Send(ITypedMessage message, NetEndPoint to) {
            ThreadChecker.AssertMainThread();

            sendInfoCollection.Enqueue(new SendInfo { message = message, to = to });
        }

        private void ThreadPoolWorker(object state) {
            Thread.CurrentThread.Name = "UnreliableChannel Thread";
            ThreadChecker.ConfigureUnreliable(Thread.CurrentThread);
            bool shouldRun = true;

            NetEndPoint to = new NetEndPoint();
            void SendTo(byte[] bytes, int count) {
                socket.Send(bytes, count, to);
            }

            NetEndPoint[] endPoints = new NetEndPoint[100];
            int endPointCount = 0;

            do {
                lock (this) { shouldRun = socket != null; }

                try {
                    socket.Receive();

                    if (sendInfoCollection.TryDequeue(out SendInfo info)) {
                        var message = info.message;
                        to = info.to;

                        if (!writerCollection.TryGetValue(to, out MessageStreamWriter writer)) {
                            writer = new MessageStreamWriter();
                            writerCollection.TryAdd(to, writer);
                            lock (lockToken) {
                                netEndPointWriters.Add(to);
                            }
                        }
                        writer.Write(message);
                    }

                    lock (lockToken) {
                        netEndPointWriters.CopyTo(endPoints);
                        endPointCount = netEndPointWriters.Count;
                    }

                    for (int index = 0; index < endPointCount; index++) {
                        to = endPoints[index];
                        var writer = writerCollection[to];
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

            if (!readerCollection.TryGetValue(from, out MessageStreamReader reader)) {
                reader = new MessageStreamReader();
                readerCollection.TryAdd(from, reader);
            }
            reader.Add(bytes, count);

            MessageContainer? container;
            while ((container = reader.Decode()) != null) {
                listener?.ChannelDidReceiveMessage(this, container.Value, from);
            }
        }

        void IUdpSocketIOListener.SocketDidWriteBytes(UdpSocket socket, int count, NetEndPoint to) {
            ThreadChecker.AssertUnreliableChannel();

            if (writerCollection.TryGetValue(to, out MessageStreamWriter writer)) {
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