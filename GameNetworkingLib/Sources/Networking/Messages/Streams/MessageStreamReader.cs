using System;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;
using Logging;

namespace GameNetworking.Messages.Streams {
    public class MessageStreamReader : IStreamReader {
        private readonly object lockToken = new object();
        private readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB

        public int currentBufferLength { get; private set; }

        public MessageStreamReader() { }

        public void Add(byte[] buffer, int count) {
            lock (lockToken) {
                Array.Copy(buffer, 0, currentBuffer, currentBufferLength, count);
                currentBufferLength += count;
            }
        }

        public MessageContainer? Decode() {
            lock (lockToken) {
                if (currentBufferLength == 0) { return null; }

                int delimiterIndex = CoderHelper.CheckForDelimiter(currentBuffer, currentBufferLength);
                if (delimiterIndex != -1) {
                    if (IsValidChecksum(delimiterIndex)) {
                        var container = new MessageContainer(currentBuffer, delimiterIndex - 1);
                        currentBufferLength = CoderHelper.SliceBuffer(delimiterIndex, currentBuffer, currentBufferLength);
                        return container;
                    } else {
                        currentBufferLength = CoderHelper.SliceBuffer(delimiterIndex, currentBuffer, currentBufferLength);
                        if (Logger.IsLoggingEnabled) { Logger.Log($"Discarded currupted message!"); }
                    }
                }
                return null;
            }
        }

        private bool IsValidChecksum(int messageEndIndex) {
            var checksum = CoderHelper.CalculateChecksum(currentBuffer, 0, messageEndIndex - 1);
            byte messageChecksum = currentBuffer[messageEndIndex - 1];
            return checksum == messageChecksum;
        }
    }
}