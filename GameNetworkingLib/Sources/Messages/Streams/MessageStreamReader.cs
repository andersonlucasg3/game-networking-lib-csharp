using System.Collections.Generic;

namespace Messages.Streams {
    using Coders;
    using Models;

    public class MessageStreamReader : IStreamReader {
        private List<byte> buffer;

        public MessageStreamReader() {
            this.buffer = new List<byte>();
        }

        public void Add(byte[] buffer) {
            if (buffer == null || buffer.Length == 0) { return; }
            this.buffer.AddRange(buffer);
        }

        public MessageContainer Decode() {
            var arrayBuffer = this.buffer.ToArray();
            int delimiterIndex = CoderHelper.CheckForDelimiter(arrayBuffer);
            if (delimiterIndex != -1) {
                byte[] bytes = CoderHelper.PackageBytes(delimiterIndex, arrayBuffer);
                var container = new MessageContainer(bytes);
                CoderHelper.SliceBuffer(delimiterIndex, ref this.buffer);
                return container;
            }
            return null;
        }
    }
}