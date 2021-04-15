using System;
using System.Collections.Concurrent;
using System.Threading;
using GameNetworking.Commons;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Networking.Sockets;
using Logging;

namespace GameNetworking.Channels
{
    public class UnreliableChannel : Channel<UnreliableChannel>, IUdpSocketIOListener
    {
        private readonly object _lockToken = new object();
        private readonly PooledList<NetEndPoint> _netEndPointWriters = PooledList<NetEndPoint>.Rent();
        private readonly ConcurrentDictionary<NetEndPoint, MessageStreamReader> _readerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamReader>();
        private readonly ConcurrentQueue<SendInfo> _sendInfoCollection = new ConcurrentQueue<SendInfo>();
        private readonly ConcurrentDictionary<NetEndPoint, MessageStreamWriter> _writerCollection = new ConcurrentDictionary<NetEndPoint, MessageStreamWriter>();
        private readonly Action<byte[], int> _socketSendTo;

        private UdpSocket _socket;
        private NetEndPoint _toNetEndPoint;

        public UnreliableChannel(UdpSocket socket)
        {
            _socket = socket;
            _socket.listener = this;

            _socketSendTo = (buffer, count) =>
            {
                lock (_lockToken) _socket.Send(buffer, count, _toNetEndPoint);
            };
        }

        ~UnreliableChannel()
        {
            lock (_lockToken) _netEndPointWriters.Dispose();
        }

        void IUdpSocketIOListener.SocketDidReceiveBytes(UdpSocket udpSocket, byte[] bytes, int count, NetEndPoint from)
        {
            ThreadChecker.AssertUnreliableChannel();

            if (!_readerCollection.TryGetValue(from, out MessageStreamReader reader))
            {
                reader = new MessageStreamReader();
                _readerCollection.TryAdd(from, reader);
            }

            reader.Add(bytes, count);
            
            ReadAllMessages(reader, from);
        }

        void IUdpSocketIOListener.SocketDidWriteBytes(UdpSocket udpSocket, int count, NetEndPoint to)
        {
            ThreadChecker.AssertUnreliableChannel();

            if (_writerCollection.TryGetValue(to, out var writer))
            {
                writer.DidWrite(count);
            }
            else
            {
                if (Logger.IsLoggingEnabled) Logger.Log($"SocketDidWriteBytes did not find writer for endPoint-{to}");
            }
        }

        internal void StartIO()
        {
            ThreadPool.QueueUserWorkItem(ThreadPoolWorker);
        }

        internal void StopIO()
        {
            lock (this)
            {
                _socket.Close();
                _socket = null;
            }
        }

        public void CloseChannel(NetEndPoint endPoint)
        {
            _writerCollection.TryRemove(endPoint, out _);
            lock (_lockToken)
            {
                _netEndPointWriters.Remove(endPoint);
            }
        }

        public void Send(ITypedMessage message, NetEndPoint to)
        {
            ThreadChecker.AssertMainThread();

            _sendInfoCollection.Enqueue(new SendInfo {message = message, to = to});
        }

        private void ThreadPoolWorker(object state)
        {
            Thread.CurrentThread.Name = "UnreliableChannel Thread";
            ThreadChecker.ConfigureUnreliable(Thread.CurrentThread);
            bool shouldRun;
            
            NetEndPoint[] endPoints = new NetEndPoint[100];

            do
            {
                lock (this) shouldRun = _socket != null;

                try
                {
                    lock(this) _socket?.Receive();

                    if (_sendInfoCollection.TryDequeue(out SendInfo info))
                    {
                        ITypedMessage message = info.message;
                        _toNetEndPoint = info.to;

                        if (!_writerCollection.TryGetValue(_toNetEndPoint, out MessageStreamWriter writer))
                        {
                            writer = new MessageStreamWriter();
                            _writerCollection.TryAdd(_toNetEndPoint, writer);
                            lock (_lockToken) _netEndPointWriters.Add(_toNetEndPoint);
                        }

                        writer.Write(message);
                    }

                    int endPointCount;
                    lock (_lockToken)
                    {
                        _netEndPointWriters.CopyTo(endPoints);
                        endPointCount = _netEndPointWriters.Count;
                    }

                    for (int index = 0; index < endPointCount; index++)
                    {
                        _toNetEndPoint = endPoints[index];
                        var writer = _writerCollection[_toNetEndPoint];
                        writer.Use(_socketSendTo);
                    }
                }
                catch (ObjectDisposedException)
                {
                    shouldRun = false;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Exception thrown in ThreadPool\n{ex}");
                }
            } while (shouldRun);

            Logger.Log("UnreliableChannel ThreadPool EXITING");
            ThreadChecker.ConfigureUnreliable(null);
        }

        private struct SendInfo
        {
            public ITypedMessage message;
            public NetEndPoint to;
        }
    }
}
