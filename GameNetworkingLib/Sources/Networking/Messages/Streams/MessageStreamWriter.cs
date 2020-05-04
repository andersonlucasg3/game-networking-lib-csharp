using System;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams {
    public interface IStreamWriter {
        void Write<TMessage>(TMessage message) where TMessage : ITypedMessage;
    }

    public class MessageStreamWriter : IStreamWriter {
        private readonly Encoder encoder = new Encoder();
        private readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB
        private int currentBufferLength;

        public bool hasBytesToWrite {
            get {
                lock (this) {
                    return this.currentBufferLength > 0;
                }
            }
        }

        public MessageStreamWriter() { }

        public void Write<TMessage>(TMessage message) where TMessage : ITypedMessage {
            lock (this) {
                this.currentBufferLength += CoderHelper.WriteHeader(message.type, this.currentBuffer, this.currentBufferLength);

                message.Encode(this.encoder);
                var messageBytes = this.encoder.encodedBytes;

                Array.Copy(messageBytes, 0, this.currentBuffer, this.currentBufferLength, messageBytes.Length);
                this.currentBufferLength += messageBytes.Length;

                this.currentBufferLength += CoderHelper.InsertDelimiter(this.currentBuffer, this.currentBufferLength);
            }
        }

        public int Put(out byte[] buffer) {
            lock (this) {
                buffer = this.currentBuffer;
                return this.currentBufferLength;
            }
        }

        public void DidWrite(int count) {
            if (count == 0) { return; }
            lock (this) {
                var newLength = this.currentBufferLength - count;
                Array.Copy(this.currentBuffer, count, this.currentBuffer, 0, newLength);
                this.currentBufferLength = newLength;
            }
        }
    }
}