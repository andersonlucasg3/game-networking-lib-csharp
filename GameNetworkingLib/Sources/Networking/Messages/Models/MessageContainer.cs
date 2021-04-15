using System;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Coders.Converters;

namespace GameNetworking.Messages.Models
{
    public class MessageContainer : IDisposable
    {
        private static readonly ObjectPool<MessageContainer> _messageContainerPool = new ObjectPool<MessageContainer>(() => new MessageContainer());
        private static readonly ObjectPool<IntByteArrayConverter> _intConverterPool = new ObjectPool<IntByteArrayConverter>(() => new IntByteArrayConverter(0));
        private static readonly ObjectPool<byte[]> _bufferPool = new ObjectPool<byte[]>(() => new byte[Consts.bufferSize]);

        private byte[] _buffer;
        private int _length;

        public int type { get; private set; }

        private MessageContainer() { }

        public static MessageContainer Rent() => _messageContainerPool.Rent();
        
        public void Dispose()
        {
            _bufferPool.Pay(_buffer);
            _messageContainerPool.Pay(this);
        }

        public MessageContainer WithBuffer(byte[] buffer, int length)
        {
            _buffer = _bufferPool.Rent();
            CoderHelper.PackageBytes(length, buffer, _buffer);
            _length = length;

            var converter = _intConverterPool.Rent();
            byte[] array = converter.array;
            Array.Copy(buffer, array, sizeof(int));
            converter.array = array;
            type = converter.value;
            _intConverterPool.Pay(converter);
            return this;
        }

        public bool Is(int typeId)
        {
            return typeId == type;
        }

        public TMessage Parse<TMessage>() where TMessage : struct, IDecodable
        {
            return BinaryDecoder.Decode<TMessage>(_buffer, sizeof(int), _length - sizeof(int));
        }
    }
}
