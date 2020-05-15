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
                        var packageBuffer = MessageContainer.GetBuffer();
                        CoderHelper.PackageBytes(delimiterIndex, this.currentBuffer, packageBuffer);
                        var container = new MessageContainer(packageBuffer, delimiterIndex);
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
            var index = ArraySearch.IndexOf(this.currentBuffer, checksum, messageEndIndex);
            return index != -1;
        }
    }
}