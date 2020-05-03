using System;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;

namespace GameNetworking.Messages.Models {
    public sealed class MessageContainer {
        private readonly Decoder _decoder;
        private readonly ObjectPool<Decoder> _decoderPool;
        private readonly byte[] _buffer;

        public int type {
            get; private set;
        }

        public MessageContainer(Decoder decoder, ObjectPool<Decoder> decoderPool, byte[] buffer) {
            this._decoderPool = decoderPool;
            this._decoder = decoder;
            this._buffer = buffer;
            this.type = decoder.GetInt();
        }

        public bool Is(int type) {
            return type == this.type;
        }

        public TMessage Parse<TMessage>() where TMessage : class, IDecodable, new() {
            TMessage message = new TMessage();
            message.Decode(this._decoder);

            this._decoderPool.Pay(this._decoder);
            ReturnBuffer(this._buffer);

            return message;
        }

        #region Buffers

        private static readonly ObjectPool<byte[]> bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);
        
        public static byte[] GetBuffer() => bufferPool.Rent();
        private static void ReturnBuffer(byte[] buffer) => bufferPool.Pay(buffer);

        #endregion
    }
}