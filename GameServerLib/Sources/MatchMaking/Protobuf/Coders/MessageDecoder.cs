using System.Collections.Generic;
using System.Collections;

namespace MatchMaking.Protobuf.Coders {
    using MatchMaking.Coders;
    using Messages.Coders;
    using Models;

    public sealed class MessageDecoder: IMessageDecoder {
        private List<byte> buffer;

        public MessageDecoder() {
            this.buffer = new List<byte>();
        }

        public void Add(byte[] bytes) {
            this.buffer.AddRange(bytes);
        }

        public MessageContainer Decode() {
            int delimiterIndex = CoderHelper.CheckForDelimiter(this.buffer.ToArray());
            if (delimiterIndex != -1) {
                byte[] bytes = CoderHelper.PackageBytes(delimiterIndex, this.buffer);
                var package = MessagePackage.Parser.ParseFrom(bytes);
                CoderHelper.SliceBuffer(delimiterIndex, ref this.buffer);
                return new MessageContainer(package);
            }
            return null;
        }
    }
}
