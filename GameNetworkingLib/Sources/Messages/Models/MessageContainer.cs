using System;

namespace Messages.Models {
    using Coders;

    public sealed class MessageContainer {
        private readonly byte[] messageBytes;

        public int type {
            get; private set;
        }

        public MessageContainer(byte[] messageBytes) {
            this.messageBytes = messageBytes;
            this.type = CoderHelper.ReadHeader(messageBytes);
        }

        public bool Is(int type) {
            return type == this.type;
        }

        public TMessage Parse<TMessage>() where TMessage : class, IDecodable, new() {
            var headerSize = sizeof(int);
            var count = this.messageBytes.Length - headerSize;
            byte[] message = new byte[count];
            Array.Copy(this.messageBytes, headerSize, message, 0, count);

            return Coders.Binary.Decoder.Decode<TMessage>(message);
        }
    }
}