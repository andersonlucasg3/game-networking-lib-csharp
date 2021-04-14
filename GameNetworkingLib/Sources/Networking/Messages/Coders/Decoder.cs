using System;
using System.IO;
using System.Text;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders.Converters;

namespace GameNetworking.Messages.Coders
{
    public interface IDecodable
    {
        void Decode(IDecoder decoder);
    }

    public interface IDecoder
    {
        int GetInt();
        short GetShort();
        long GetLong();
        uint GetUInt();
        ushort GetUShort();
        ulong GetULong();

        float GetFloat();
        double GetDouble();

        string GetString();
        byte[] GetBytes();

        bool GetBool();

        TDecodable? GetObject<TDecodable>() where TDecodable : struct, IDecodable;
    }

    internal struct _Decoder : IDecoder
    {
        private static readonly ObjectPool<MemoryStream> _memoryStreamPool
            = new ObjectPool<MemoryStream>(() => new MemoryStream());

        private static readonly ObjectPool<ShortByteArrayConverter> _shortConverterPool
            = new ObjectPool<ShortByteArrayConverter>(() => new ShortByteArrayConverter(0));

        private static readonly ObjectPool<UShortByteArrayConverter> _ushortConverterPool
            = new ObjectPool<UShortByteArrayConverter>(() => new UShortByteArrayConverter(0));

        private static readonly ObjectPool<IntByteArrayConverter> _intConverterPool
            = new ObjectPool<IntByteArrayConverter>(() => new IntByteArrayConverter(0));

        private static readonly ObjectPool<UIntByteArrayConverter> _uintConverterPool
            = new ObjectPool<UIntByteArrayConverter>(() => new UIntByteArrayConverter(0U));

        private static readonly ObjectPool<LongByteArrayConverter> _longConverterPool
            = new ObjectPool<LongByteArrayConverter>(() => new LongByteArrayConverter(0L));

        private static readonly ObjectPool<ULongByteArrayConverter> _ulongConverterPool
            = new ObjectPool<ULongByteArrayConverter>(() => new ULongByteArrayConverter(0UL));

        private static readonly ObjectPool<FloatByteArrayConverter> _floatConverterPool
            = new ObjectPool<FloatByteArrayConverter>(() => new FloatByteArrayConverter(0F));

        private static readonly ObjectPool<DoubleByteArrayConverter> _doubleConverterPool
            = new ObjectPool<DoubleByteArrayConverter>(() => new DoubleByteArrayConverter(0F));

        private static readonly ObjectPool<byte[]> _boolBufferPool
            = new ObjectPool<byte[]>(() => new byte[1]);

        private MemoryStream _memoryStream;
        private ShortByteArrayConverter _shortConverter;
        private UShortByteArrayConverter _ushortConverter;
        private IntByteArrayConverter _intConverter;
        private UIntByteArrayConverter _uintConverter;
        private LongByteArrayConverter _longConverter;
        private ULongByteArrayConverter _ulongConverter;
        private FloatByteArrayConverter _floatConverter;
        private DoubleByteArrayConverter _doubleConverter;
        private byte[] _boolBuffer;

        public void SetBuffer(byte[] bytes, int offset, int length)
        {
            _memoryStream.SetLength(0);
            _memoryStream.Write(bytes, offset, length);
            _memoryStream.Seek(0, SeekOrigin.Begin);
        }

        public int GetInt()
        {
            var array = _intConverter.array;
            _memoryStream.Read(array, 0, sizeof(int));
            _intConverter.array = array;
            return _intConverter.value;
        }

        public short GetShort()
        {
            var array = _shortConverter.array;
            _memoryStream.Read(array, 0, sizeof(short));
            _shortConverter.array = array;
            return _shortConverter.value;
        }

        public long GetLong()
        {
            var array = _longConverter.array;
            _memoryStream.Read(array, 0, sizeof(long));
            _longConverter.array = array;
            return _longConverter.value;
        }

        public uint GetUInt()
        {
            var array = _uintConverter.array;
            _memoryStream.Read(array, 0, sizeof(uint));
            _uintConverter.array = array;
            return _uintConverter.value;
        }

        public ushort GetUShort()
        {
            var array = _ushortConverter.array;
            _memoryStream.Read(array, 0, sizeof(ushort));
            _ushortConverter.array = array;
            return _ushortConverter.value;
        }

        public ulong GetULong()
        {
            var array = _ulongConverter.array;
            _memoryStream.Read(array, 0, sizeof(ulong));
            _ulongConverter.array = array;
            return _ulongConverter.value;
        }

        public float GetFloat()
        {
            var array = _floatConverter.array;
            _memoryStream.Read(array, 0, sizeof(float));
            _floatConverter.array = array;
            return _floatConverter.value;
        }

        public double GetDouble()
        {
            var array = _doubleConverter.array;
            _memoryStream.Read(array, 0, sizeof(double));
            _doubleConverter.array = array;
            return _doubleConverter.value;
        }

        public string GetString()
        {
            var bytes = GetBytes();
            return Encoding.ASCII.GetString(bytes);
        }

        public byte[] GetBytes()
        {
            var length = GetInt();
            var bytes = new byte[length];
            _memoryStream.Read(bytes, 0, length);
            return bytes;
        }

        public bool GetBool()
        {
            _memoryStream.Read(_boolBuffer, 0, 1);
            return Convert.ToBoolean(_boolBuffer[0]);
        }

        public T? GetObject<T>() where T : struct, IDecodable
        {
            if (GetBool())
            {
                var value = new T();
                value.Decode(this);
                return value;
            }

            return null;
        }

        internal void Rent()
        {
            _memoryStream = _memoryStreamPool.Rent();
            _shortConverter = _shortConverterPool.Rent();
            _ushortConverter = _ushortConverterPool.Rent();
            _intConverter = _intConverterPool.Rent();
            _uintConverter = _uintConverterPool.Rent();
            _longConverter = _longConverterPool.Rent();
            _ulongConverter = _ulongConverterPool.Rent();
            _floatConverter = _floatConverterPool.Rent();
            _doubleConverter = _doubleConverterPool.Rent();
            _boolBuffer = _boolBufferPool.Rent();
        }

        internal void Pay()
        {
            _memoryStreamPool.Pay(_memoryStream);
            _shortConverterPool.Pay(_shortConverter);
            _ushortConverterPool.Pay(_ushortConverter);
            _intConverterPool.Pay(_intConverter);
            _uintConverterPool.Pay(_uintConverter);
            _longConverterPool.Pay(_longConverter);
            _ulongConverterPool.Pay(_ulongConverter);
            _floatConverterPool.Pay(_floatConverter);
            _doubleConverterPool.Pay(_doubleConverter);
            _boolBufferPool.Pay(_boolBuffer);
        }
    }

    public static class BinaryDecoder
    {
        public static TMessage Decode<TMessage>(byte[] fromBuffer, int index, int length) where TMessage : struct, IDecodable
        {
            var decoder = new _Decoder();
            decoder.Rent();
            decoder.SetBuffer(fromBuffer, index, length);
            var message = new TMessage();
            message.Decode(decoder);
            decoder.Pay();
            return message;
        }
    }
}