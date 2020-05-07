using System;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams {
    public interface IStreamWriter {
        void Write<TMessage>(TMessage message) where TMessage : ITypedMessage;
    }

    public class MessageStreamWriter : IStreamWriter {
        private readonly object lockToken = new object();
        private readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB
        private int currentBufferLength = 0;

        public MessageStreamWriter() { }

        public void Write<TMessage>(TMessage message) where TMessage : ITypedMessage {
            lock (this.lockToken) {
                this.currentBufferLength += CoderHelper.WriteHeader(message.type, this.currentBuffer, this.currentBufferLength);
                this.currentBufferLength += BinaryEncoder.Encode(message, this.currentBuffer, this.currentBufferLength);
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