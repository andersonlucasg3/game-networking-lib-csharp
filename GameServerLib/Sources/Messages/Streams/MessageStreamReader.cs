using System.Collections.Generic;

namespace Messages.Streams {
    using Coders;
    using Models;

    public class MessageStreamReader: IStreamReader {
        private List<byte> buffer;

        public MessageStreamReader() {
            this.buffer = new List<byte>();
        }

        public void Add(byte[] buffer) {
            lock (this.buffer) {
                this.buffer.AddRange(buffer);
            }
        }

        public MessageContainer Decode() {
            lock (this.buffer) {
                int delimiterIndex = CoderHelper.CheckForDelimiter(this.buffer.ToArray());
                if (delimiterIndex != -1) {
                    byte[] bytes = CoderHelper.PackageBytes(delimiterIndex, this.buffer);
                    var container = new MessageContainer(new List<byte>(bytes));
                    CoderHelper.SliceBuffer(delimiterIndex, ref this.buffer);
                    return container;
                }
            }
            return null;
        }
    }
}