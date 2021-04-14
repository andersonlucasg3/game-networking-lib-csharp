using System;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams
{
    public interface IStreamWriter
    {
        void Write<TMessage>(TMessage message) where TMessage : ITypedMessage;
    }

    public class MessageStreamWriter : IStreamWriter
    {
        private readonly object lockToken = new object();
#if !UNITY_64
        public readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB
#else
        private readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB
#endif

        public int currentBufferLength { get; private set; }

        public void Write<TMessage>(TMessage message) where TMessage : ITypedMessage
        {
            lock (lockToken)
            {
                var startIndex = currentBufferLength;
                currentBufferLength += CoderHelper.WriteInt(message.type, currentBuffer, currentBufferLength);
                currentBufferLength += BinaryEncoder.Encode(message, currentBuffer, currentBufferLength);
                currentBufferLength += CoderHelper.AddChecksum(currentBuffer, startIndex, currentBufferLength);
                currentBufferLength += CoderHelper.InsertDelimiter(currentBuffer, currentBufferLength);
            }
        }

        public void Use(Action<byte[], int> action)
        {
            lock (lockToken)
            {
                if (currentBufferLength == 0) return;
                action?.Invoke(currentBuffer, currentBufferLength);
            }
        }

        public void DidWrite(int count)
        {
            if (count <= 0) return;
            if (count == currentBufferLength)
            {
                currentBufferLength = 0;
                return;
            }

            var newLength = currentBufferLength - count;
            Array.Copy(currentBuffer, count, currentBuffer, 0, newLength);
            currentBufferLength = newLength;
        }
    }
}