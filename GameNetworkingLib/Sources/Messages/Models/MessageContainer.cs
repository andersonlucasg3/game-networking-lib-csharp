using System;
using System.Collections.Generic;

namespace Messages.Models {
    using Coders;

    public sealed class MessageContainer {
        private readonly byte[] messageBytes;

        public int Type {
            get; private set;
        }

        public MessageContainer(byte[] messageBytes) {
            this.messageBytes = messageBytes;
            this.Type = CoderHelper.ReadHeader(messageBytes);
        }

        public bool Is(int type) {
            return type == this.Type;
        }

        public Message Parse<Message>() where Message : class, IDecodable, new() {
            var headerSize = sizeof(int);
            var count = this.messageBytes.Length - headerSize;
            byte[] message = new byte[count];
            Array.Copy(this.messageBytes, headerSize, message, 0, count);

            return new Coders.Binary.Decoder().Decode<Message>(message);
        }
    }
}