using System;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams {
    public interface IStreamWriter {
        int Write<TMessage>(TMessage message, out byte[] buffer) where TMessage : ITypedMessage;
    }

    public class MessageStreamWriter : IStreamWriter {
        private readonly byte[] currentBuffer = new byte[Consts.bufferSize];
        private int currentBufferLength;

        public MessageStreamWriter() { }

        public int Write<TMessage>(TMessage message, out byte[] buffer) where TMessage : ITypedMessage {
            buffer = this.currentBuffer;

            this.currentBufferLength += CoderHelper.WriteHeader(message.type, buffer, this.currentBufferLength);

            var messageBytes = Coders.Binary.Encoder.Encode(message);
            Array.Copy(messageBytes, 0, buffer, this.currentBufferLength, messageBytes.Length);
            this.currentBufferLength += messageBytes.Length;

            this.currentBufferLength += CoderHelper.InsertDelimiter(buffer, this.currentBufferLength);

            return this.currentBufferLength;
        }

        public void DidWrite(int count) {
            if (count == 0) { return; }

            var newLength = this.currentBufferLength - count;
            Array.Copy(this.currentBuffer, count, this.currentBuffer, 0, newLength);
            this.currentBufferLength = newLength;
        }
    }
}