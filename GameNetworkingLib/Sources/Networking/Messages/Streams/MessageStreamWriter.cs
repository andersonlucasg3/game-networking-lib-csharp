using System;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams {
    public interface IStreamWriter {
        void Write<TMessage>(TMessage message) where TMessage : ITypedMessage;
    }

    public class MessageStreamWriter : IStreamWriter {
        private readonly object lockToken = new object();
#if !UNITY_64
        public readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB
#else
        private readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB
#endif

        public int currentBufferLength { get; private set; } = 0;

        public MessageStreamWriter() { }

        public void Write<TMessage>(TMessage message) where TMessage : ITypedMessage {
            lock (this.lockToken) {
                var startIndex = this.currentBufferLength;
                this.currentBufferLength += CoderHelper.WriteInt(message.type, this.currentBuffer, this.currentBufferLength);
                this.currentBufferLength += BinaryEncoder.Encode(message, this.currentBuffer, this.currentBufferLength);
                var checksum = CoderHelper.ComputeAdditionChecksum(this.currentBuffer, startIndex, this.currentBufferLength);
                this.currentBufferLength += CoderHelper.WriteInt(checksum, this.currentBuffer, this.currentBufferLength);
                this.currentBufferLength += CoderHelper.InsertDelimiter(this.currentBuffer, this.currentBufferLength);
            }
        }

        public void Use(Action<byte[], int> action) {
            lock (this.lockToken) {
                if (this.currentBufferLength == 0) { return; }
                action?.Invoke(this.currentBuffer, this.currentBufferLength);
            }
        }

        public void DidWrite(int count) {
            lock (this.lockToken) {
                if (count <= 0) { return; }
                if (count == this.currentBufferLength) {
                    this.currentBufferLength = 0;
                    return;
                }
                var newLength = this.currentBufferLength - count;
                Array.Copy(this.currentBuffer, count, this.currentBuffer, 0, newLength);
                this.currentBufferLength = newLength;
            }
        }
    }
}