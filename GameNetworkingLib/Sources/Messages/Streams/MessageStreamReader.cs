using System.Collections.Generic;

namespace Messages.Streams {
    using Coders;
    using Models;

    public class MessageStreamReader : IStreamReader {
        private List<byte> byteList;

        public MessageStreamReader() {
            this.byteList = new List<byte>();
        }

        public void Add(byte[] buffer, int count) {
            if (count == 0) { return; }
            for (int index = 0; index < count; index++) { this.byteList.Add(buffer[index]); }
        }

        public MessageContainer Decode() {
            var arrayBuffer = this.byteList.ToArray();
            int delimiterIndex = CoderHelper.CheckForDelimiter(arrayBuffer);
            if (delimiterIndex != -1) {
                byte[] bytes = CoderHelper.PackageBytes(delimiterIndex, arrayBuffer);
                var container = new MessageContainer(bytes);
                CoderHelper.SliceBuffer(delimiterIndex, ref this.byteList);
                return container;
            }
            return null;
        }
    }
}