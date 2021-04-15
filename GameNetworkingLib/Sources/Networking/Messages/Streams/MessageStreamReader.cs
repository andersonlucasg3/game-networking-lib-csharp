using System;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;
using Logging;

namespace GameNetworking.Messages.Streams
{
    public class MessageStreamReader : IStreamReader
    {
        private readonly object _lockToken = new object();
        private readonly byte[] _currentBuffer = new byte[1024 * 1024]; // 1MB

        public int currentBufferLength { get; private set; }

        public void Add(byte[] buffer, int count)
        {
            lock (_lockToken)
            {
                Array.Copy(buffer, 0, _currentBuffer, currentBufferLength, count);
                currentBufferLength += count;
            }
        }

        public MessageContainer Decode()
        {
            lock (_lockToken)
            {
                if (currentBufferLength == 0) return null;

                var delimiterIndex = CoderHelper.CheckForDelimiter(_currentBuffer, currentBufferLength);

                if (delimiterIndex == -1) return null;

                if (IsValidChecksum(delimiterIndex))
                {
                    MessageContainer container = MessageContainer.Rent().WithBuffer(_currentBuffer, delimiterIndex - 1);
                    currentBufferLength = CoderHelper.SliceBuffer(delimiterIndex, _currentBuffer, currentBufferLength);
                    return container;
                }

                currentBufferLength = CoderHelper.SliceBuffer(delimiterIndex, _currentBuffer, currentBufferLength);
                if (Logger.IsLoggingEnabled) Logger.Log("Discarded corrupted message!");

                return null;
            }
        }

        private bool IsValidChecksum(int messageEndIndex)
        {
            var checksum = CoderHelper.CalculateChecksum(_currentBuffer, 0, messageEndIndex - 1);
            var messageChecksum = _currentBuffer[messageEndIndex - 1];
            return checksum == messageChecksum;
        }
    }
}
