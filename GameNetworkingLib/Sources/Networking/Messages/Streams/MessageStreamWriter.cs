﻿using System;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams {
    public interface IStreamWriter {
        void Write<TMessage>(TMessage message) where TMessage : ITypedMessage;
    }

    public class MessageStreamWriter : IStreamWriter {
        private readonly byte[] currentBuffer = new byte[Consts.bufferSize];
        private int currentBufferLength;

        public bool hasBytesToWrite => this.currentBufferLength > 0;

        public MessageStreamWriter() { }

        public void Write<TMessage>(TMessage message) where TMessage : ITypedMessage {
            this.currentBufferLength += CoderHelper.WriteHeader(message.type, this.currentBuffer, this.currentBufferLength);

            var messageBytes = Coders.Binary.Encoder.Encode(message);
            Array.Copy(messageBytes, 0, this.currentBuffer, this.currentBufferLength, messageBytes.Length);
            this.currentBufferLength += messageBytes.Length;

            this.currentBufferLength += CoderHelper.InsertDelimiter(this.currentBuffer, this.currentBufferLength);
        }

        public int Put(out byte[] buffer) {
            buffer = this.currentBuffer;
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