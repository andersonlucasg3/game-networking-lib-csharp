using System.Collections.Generic;

namespace GameNetworking.Messages.Streams {
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
                byte[] packetBytes = MessageContainer.GetBuffer();
                CoderHelper.PackageBytes(delimiterIndex, arrayBuffer, packetBytes);
                var container = new MessageContainer(packetBytes);
                CoderHelper.SliceBuffer(delimiterIndex, ref this.byteList);
                return container;
            }
            return null;
        }
    }
}