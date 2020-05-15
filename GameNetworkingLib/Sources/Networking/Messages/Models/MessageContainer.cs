using System;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Coders.Converters;

namespace GameNetworking.Messages.Models {
    public struct MessageContainer {
        private static IntByteArrayConverter _intConverter = new IntByteArrayConverter(0);
        private readonly byte[] _buffer;
        private readonly int _length;

        public int type { get; private set; }

        public MessageContainer(byte[] buffer, int length) {
            this._buffer = buffer;
            this._length = length;

            var array = _intConverter.array;
            Array.Copy(buffer, array, sizeof(int));
            _intConverter.array = array;
            this.type = _intConverter.value;
        }

        public bool Is(int type) {
            return type == this.type;
        }

        public TMessage Parse<TMessage>() where TMessage : struct, IDecodable
            => BinaryDecoder.Decode<TMessage>(this._buffer, sizeof(int), this._length - sizeof(int));

        #region Buffers

        private static readonly ObjectPool<byte[]> bufferPool
            = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);

        internal static byte[] GetBuffer() => bufferPool.Rent();
        internal void ReturnBuffer() => bufferPool.Pay(this._buffer);

        #endregion
    }
}