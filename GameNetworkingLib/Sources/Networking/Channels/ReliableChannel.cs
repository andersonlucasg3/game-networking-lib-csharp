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

    public class ReliableChannel : ITcpSocketIOListener<TcpSocket>
    {
        private static bool ioRunning;
        private static readonly List<ReliableChannel> aliveSockets = new List<ReliableChannel>();
        private static readonly object socketLock = new object();

        private readonly MessageStreamReader reader;
        private readonly TcpSocket socket;
        private readonly MessageStreamWriter writer;

        public ReliableChannel(TcpSocket socket)
        {
            this.socket = socket;
            this.socket.ioListener = this;

            reader = new MessageStreamReader();
            writer = new MessageStreamWriter();
        }

        public IReliableChannelListener listener { get; set; }

        void ITcpSocketIOListener<TcpSocket>.SocketDidReceiveBytes(TcpSocket remoteSocket, byte[] bytes, int count)
        {
            reader.Add(bytes, count);

            MessageContainer? container;
            while ((container = reader.Decode()).HasValue) listener?.ChannelDidReceiveMessage(this, container.Value);
        }

        void ITcpSocketIOListener<TcpSocket>.SocketDidSendBytes(TcpSocket remoteSocket, int count)
        {
            writer.DidWrite(count);
        }

        public static void StartIO()
        {
            ioRunning = true;
            ThreadPool.QueueUserWorkItem(ThreadPoolWorker);
        }

        public static void StopIO()
        {
            ioRunning = false;
        }

        public static void Add(ReliableChannel channel)
        {
            ThreadChecker.AssertReliableChannel();

            lock (socketLock)
            {
                aliveSockets.Add(channel);
            }
        }

        public static void Remove(ReliableChannel channel)
        {
            ThreadChecker.AssertReliableChannel();

            lock (socketLock)
            {
                aliveSockets.Remove(channel);
            }
        }

        public void CloseChannel()
        {
            ThreadChecker.AssertMainThread();

            socket.Disconnect();
        }

        public void Send(ITypedMessage message)
        {
            ThreadChecker.AssertMainThread();

            writer.Write(message);
        }

        private static void ThreadPoolWorker(object state)
        {
            Thread.CurrentThread.Name = "ReliableChannel Thread";
            ThreadChecker.ConfigureReliable(Thread.CurrentThread);
            var channels = new ReliableChannel[100];
            do
            {
                int channelCount;
                lock (socketLock)
                {
                    aliveSockets.CopyTo(channels);
                    channelCount = aliveSockets.Count;
                }

                for (var index = 0; index < channelCount; index++)
                {
                    var channel = channels[index];
                    try
                    {
                        channel.socket.Receive();
                        channel.writer.Use(channel.socket.Send);
                    }
                    catch (ObjectDisposedException)
                    {
                        ioRunning = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Exception thrown in ThreadPool\n{ex}");
                    }
                }
            } while (ioRunning);

            Logger.Log("ReliableChannel ThreadPool EXITING");
            ThreadChecker.ConfigureReliable(null);
        }
    }
}
