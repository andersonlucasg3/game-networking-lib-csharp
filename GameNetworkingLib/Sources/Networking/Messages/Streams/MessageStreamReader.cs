using System;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams {
    public class MessageStreamReader : IStreamReader {
        private readonly Object lockToken = new Object();
        private readonly ObjectPool<Decoder> _decoderPool = new ObjectPool<Decoder>(() => new Decoder());
        private readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB
        private int currentBufferLength;

        public MessageStreamReader() { }

        public void Add(byte[] buffer, int count) {
            lock (this.lockToken) {
                Array.Copy(buffer, 0, this.currentBuffer, this.currentBufferLength, count);
                this.currentBufferLength += count;
            }
        }

        public MessageContainer Decode() {
            lock (this.lockToken) {
                if (this.currentBufferLength == 0) { return null; }

                int delimiterIndex = CoderHelper.CheckForDelimiter(this.currentBuffer, this.currentBufferLength);
                if (delimiterIndex != -1) {
                    var packageBuffer = MessageContainer.GetBuffer();
                    CoderHelper.PackageBytes(delimiterIndex, this.currentBuffer, packageBuffer);

                    var decoder = this._decoderPool.Rent();
                    decoder.SetBuffer(packageBuffer, 0, delimiterIndex);
                    var container = new MessageContainer(decoder, this._decoderPool, packageBuffer);

                    this.currentBufferLength = CoderHelper.SliceBuffer(delimiterIndex, this.currentBuffer, this.currentBufferLength);

                    return container;
                }
                return null;
            }
        }
    }
}