using System;
using System.Collections.Generic;
using System.Threading;
using GameNetworking.Commons;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using GameNetworking.Networking.Sockets;
using Logging;

namespace GameNetworking.Channels
{
    public interface IReliableChannelListener
    {
        void ChannelDidReceiveMessage(ReliableChannel channel, MessageContainer container);
    }

    public class ReliableChannel : ITcpSocketIOListener
    {
        private static bool _ioRunning;
        private static readonly PooledList<ReliableChannel> _aliveSockets = PooledList<ReliableChannel>.Rent();
        private static readonly object _socketLock = new object();

        private readonly MessageStreamReader _reader;
        private readonly ITcpSocket _socket;
        private readonly MessageStreamWriter _writer;

        public ReliableChannel(ITcpSocket socket)
        {
            _socket = socket;
            _socket.ioListener = this;

            _reader = new MessageStreamReader();
            _writer = new MessageStreamWriter();
        }

        ~ReliableChannel()
        {
            lock(_socketLock) _aliveSockets.Dispose();
        }

        public IReliableChannelListener listener { get; set; }

        void ITcpSocketIOListener.SocketDidReceiveBytes(ITcpSocket remoteSocket, byte[] bytes, int count)
        {
            _reader.Add(bytes, count);

            MessageContainer container;
            while ((container = _reader.Decode()) != null) listener?.ChannelDidReceiveMessage(this, container);
        }

        void ITcpSocketIOListener.SocketDidSendBytes(ITcpSocket remoteSocket, int count)
        {
            _writer.DidWrite(count);
        }

        public static void StartIO()
        {
            _ioRunning = true;
            ThreadPool.QueueUserWorkItem(ThreadPoolWorker);
        }

        public static void StopIO()
        {
            _ioRunning = false;
        }

        public static void Add(ReliableChannel channel)
        {
            lock (_socketLock) _aliveSockets.Add(channel);
        }

        public static void Remove(ReliableChannel channel)
        {
            lock (_socketLock) _aliveSockets.Remove(channel);
        }

        public void CloseChannel()
        {
            ThreadChecker.AssertMainThread();

            _socket.Disconnect();
        }

        public void Send(ITypedMessage message)
        {
            _writer.Write(message);
        }

        private static void ThreadPoolWorker(object state)
        {
            Thread.CurrentThread.Name = "ReliableChannel Thread";
            ThreadChecker.ConfigureReliable(Thread.CurrentThread);
            var channels = new ReliableChannel[100];
            do
            {
                int channelCount;
                lock (_socketLock)
                {
                    _aliveSockets.CopyTo(channels);
                    channelCount = _aliveSockets.Count;
                }

                for (var index = 0; index < channelCount; index++)
                {
                    var channel = channels[index];
                    try
                    {
                        channel._socket.Receive();
                        channel._writer.Use(channel._socket.Send);
                    }
                    catch (ObjectDisposedException)
                    {
                        _ioRunning = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Exception thrown in ThreadPool\n{ex}");
                    }
                }
            } while (_ioRunning);

            Logger.Log("ReliableChannel ThreadPool EXITING");
            ThreadChecker.ConfigureReliable(null);
        }
    }
}
