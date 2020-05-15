using System;
using System.IO;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders.Converters;

namespace GameNetworking.Messages.Coders {
    public interface IDecodable {
        void Decode(IDecoder decoder);
    }

    public interface IDecoder {
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

    struct _Decoder : IDecoder {
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

        public void SetBuffer(byte[] bytes, int offset, int length) {
            this._memoryStream.SetLength(0);
            this._memoryStream.Write(bytes, offset, length);
            this._memoryStream.Seek(0, SeekOrigin.Begin);
        }

        public int GetInt() {
            var array = this._intConverter.array;
            this._memoryStream.Read(array, 0, sizeof(int));
            this._intConverter.array = array;
            return this._intConverter.value;
        }

        public short GetShort() {
            var array = this._shortConverter.array;
            this._memoryStream.Read(array, 0, sizeof(short));
            this._shortConverter.array = array;
            return this._shortConverter.value;
        }

        public long GetLong() {
            var array = this._longConverter.array;
            this._memoryStream.Read(array, 0, sizeof(long));
            this._longConverter.array = array;
            return this._longConverter.value;
        }

        public uint GetUInt() {
            var array = this._uintConverter.array;
            this._memoryStream.Read(array, 0, sizeof(uint));
            this._uintConverter.array = array;
            return this._uintConverter.value;
        }

        public ushort GetUShort() {
            var array = this._ushortConverter.array;
            this._memoryStream.Read(array, 0, sizeof(ushort));
            this._ushortConverter.array = array;
            return this._ushortConverter.value;
        }

        public ulong GetULong() {
            var array = this._ulongConverter.array;
            this._memoryStream.Read(array, 0, sizeof(ulong));
            this._ulongConverter.array = array;
            return this._ulongConverter.value;
        }

        public float GetFloat() {
            var array = this._floatConverter.array;
            this._memoryStream.Read(array, 0, sizeof(float));
            this._floatConverter.array = array;
            return this._floatConverter.value;
        }

        public double GetDouble() {
            var array = this._doubleConverter.array;
            this._memoryStream.Read(array, 0, sizeof(double));
            this._doubleConverter.array = array;
            return this._doubleConverter.value;
        }

        public string GetString() {
            var bytes = this.GetBytes();
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public byte[] GetBytes() {
            int length = this.GetInt();
            byte[] bytes = new byte[length];
            this._memoryStream.Read(bytes, 0, length);
            return bytes;
        }

        public bool GetBool() {
            this._memoryStream.Read(this._boolBuffer, 0, 1);
            return Convert.ToBoolean(this._boolBuffer[0]);
        }

        public T? GetObject<T>() where T : struct, IDecodable {
            if (this.GetBool()) {
                T value = new T();
                value.Decode(this);
                return value;
            }
            return null;
        }

        internal void Rent() {
            this._memoryStream = _memoryStreamPool.Rent();
            this._shortConverter = _shortConverterPool.Rent();
            this._ushortConverter = _ushortConverterPool.Rent();
            this._intConverter = _intConverterPool.Rent();
            this._uintConverter = _uintConverterPool.Rent();
            this._longConverter = _longConverterPool.Rent();
            this._ulongConverter = _ulongConverterPool.Rent();
            this._floatConverter = _floatConverterPool.Rent();
            this._doubleConverter = _doubleConverterPool.Rent();
            this._boolBuffer = _boolBufferPool.Rent();
        }

        internal void Pay() {
            _memoryStreamPool.Pay(this._memoryStream);
            _shortConverterPool.Pay(this._shortConverter);
            _ushortConverterPool.Pay(this._ushortConverter);
            _intConverterPool.Pay(this._intConverter);
            _uintConverterPool.Pay(this._uintConverter);
            _longConverterPool.Pay(this._longConverter);
            _ulongConverterPool.Pay(this._ulongConverter);
            _floatConverterPool.Pay(this._floatConverter);
            _doubleConverterPool.Pay(this._doubleConverter);
            _boolBufferPool.Pay(this._boolBuffer);
        }
    }

    public static class BinaryDecoder {
        public static TMessage Decode<TMessage>(byte[] fromBuffer, int index, int length) where TMessage : struct, IDecodable {
            var decoder = new _Decoder();
            decoder.Rent();
            decoder.SetBuffer(fromBuffer, index, length);
            TMessage message = new TMessage();
            message.Decode(decoder);
            decoder.Pay();
            return message;
        }
    }
}
