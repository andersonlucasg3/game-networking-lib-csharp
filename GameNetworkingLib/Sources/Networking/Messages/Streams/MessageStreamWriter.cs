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
        private readonly object _lockToken = new object();
        public readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB

        public int currentBufferLength { get; private set; }

        public void Write<TMessage>(TMessage message) where TMessage : ITypedMessage
        {
            lock (_lockToken)
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
            lock (_lockToken)
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
            lock (_lockToken)
            {
                Array.Copy(currentBuffer, count, currentBuffer, 0, newLength);
            }
            currentBufferLength = newLength;
        }
    }
}
