#if ENABLE

using System.Collections.Generic;
using System.Collections;

namespace MatchMaking.Protobuf.Coders {
    using System;
    using MatchMaking.Coders;
    using Messages.Coders;
    using Models;

    public sealed class MessageDecoder : IMessageDecoder {
        private List<byte> buffer;

        public MessageDecoder() {
            this.buffer = new List<byte>();
        }

        public void Add(byte[] bytes) {
            this.buffer.AddRange(bytes);
        }

        public MessageContainer Decode() {
            var arrayBuffer = this.buffer.ToArray();
            int delimiterIndex = CoderHelper.CheckForDelimiter(arrayBuffer);
            if (delimiterIndex != -1) {
                byte[] bytes = CoderHelper.PackageBytes(delimiterIndex, arrayBuffer);
                var package = MessagePackage.Parser.ParseFrom(bytes);
                CoderHelper.SliceBuffer(delimiterIndex, ref this.buffer);
                return new MessageContainer(package);
            }
            return null;
        }
    }
}

#endif