using System.Collections.Generic;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams {
    public class MessageStreamReader : IStreamReader {
        private readonly ObjectPool<Decoder> _decoderPool = new ObjectPool<Decoder>(() => new Decoder());
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
                var packageBuffer = MessageContainer.GetBuffer();
                CoderHelper.PackageBytes(delimiterIndex, arrayBuffer, packageBuffer);

                var decoder = this._decoderPool.Rent();
                decoder.SetBuffer(packageBuffer, 0, delimiterIndex);
                var container = new MessageContainer(decoder, this._decoderPool, packageBuffer);

                CoderHelper.SliceBuffer(delimiterIndex, ref this.byteList);

                return container;
            }
            return null;
        }
    }
}