using System;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;
using Logging;

namespace GameNetworking.Messages.Streams {
    public class MessageStreamReader : IStreamReader {
        private readonly object lockToken = new object();
        private readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB
        private readonly byte[] checksumBuffer = new byte[16];

        public int currentBufferLength { get; private set; }

        public MessageStreamReader() { }

        public void Add(byte[] buffer, int count) {
            lock (this.lockToken) {
                Array.Copy(buffer, 0, this.currentBuffer, this.currentBufferLength, count);
                this.currentBufferLength += count;
            }
        }

        public MessageContainer? Decode() {
            lock (this.lockToken) {
                if (this.currentBufferLength == 0) { return null; }

                int delimiterIndex = CoderHelper.CheckForDelimiter(this.currentBuffer, this.currentBufferLength);
                if (delimiterIndex != -1) {
                    if (this.IsValidChecksum(delimiterIndex)) {
                        var container = new MessageContainer(this.currentBuffer, delimiterIndex);
                        this.currentBufferLength = CoderHelper.SliceBuffer(delimiterIndex, this.currentBuffer, this.currentBufferLength);
                        return container;
                    } else {
                        this.currentBufferLength = CoderHelper.SliceBuffer(delimiterIndex, this.currentBuffer, this.currentBufferLength);
                        if (Logger.IsLoggingEnabled) { Logger.Log($"Discarded currupted message!"); }
                    }
                }
                return null;
            }
        }

        private bool IsValidChecksum(int messageEndIndex) {
            var checksum = CoderHelper.CalculateChecksum(this.currentBuffer, 0, messageEndIndex - 16);
            Array.Copy(this.currentBuffer, messageEndIndex - 16, this.checksumBuffer, 0, 16);
            unchecked {
                for (int index = 0; index < checksum.Length; index++) {
                    if (checksum[index] != this.checksumBuffer[index]) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}