using System;
using System.IO;
using System.Text;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders.Converters;

namespace GameNetworking.Messages.Coders {
    public interface IEncodable {
        void Encode(IEncoder encoder);
    }

    public interface IEncoder {
        void Encode(int value);
        void Encode(short value);
        void Encode(long value);
        void Encode(uint value);
        void Encode(ushort value);
        void Encode(ulong value);

        void Encode(float value);
        void Encode(double value);

        void Encode(string value);
        void Encode(byte[] value);

        void Encode(bool value);

        void Encode(IEncodable value);
    }

    struct _Encoder : IEncoder {
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

        public int PutBytes(byte[] buffer, int index) {
            var memBuff = this._memoryStream.GetBuffer();
            var length = (int)this._memoryStream.Length;
            Array.Copy(memBuff, 0, buffer, index, length);
            this._memoryStream.SetLength(0);
            return length;
        }

        public void Encode(int value) {
            this._intConverter.value = value;
            this._memoryStream.Write(this._intConverter.array, 0, sizeof(int));
        }

        public void Encode(short value) {
            this._shortConverter.value = value;
            this._memoryStream.Write(this._shortConverter.array, 0, sizeof(short));
        }

        public void Encode(long value) {
            this._longConverter.value = value;
            this._memoryStream.Write(this._longConverter.array, 0, sizeof(long));
        }

        public void Encode(uint value) {
            this._uintConverter.value = value;
            this._memoryStream.Write(this._uintConverter.array, 0, sizeof(uint));
        }

        public void Encode(ushort value) {
            this._ushortConverter.value = value;
            this._memoryStream.Write(this._ushortConverter.array, 0, sizeof(ushort));
        }

        public void Encode(ulong value) {
            this._ulongConverter.value = value;
            this._memoryStream.Write(this._ulongConverter.array, 0, sizeof(ulong));
        }

        public void Encode(float value) {
            this._floatConverter.value = value;
            this._memoryStream.Write(this._floatConverter.array, 0, sizeof(float));
        }

        public void Encode(double value) {
            this._doubleConverter.value = value;
            this._memoryStream.Write(this._doubleConverter.array, 0, sizeof(double));
        }

        public void Encode(string value) {
            var bytes = Encoding.ASCII.GetBytes(value);
            this.Encode(bytes);
        }

        public void Encode(byte[] value) {
            this._intConverter.value = value.Length;
            this._memoryStream.Write(this._intConverter.array, 0, sizeof(int));

            this._memoryStream.Write(value, 0, value.Length);
        }

        public void Encode(bool value) {
            this._boolBuffer[0] = Convert.ToByte(value);
            this._memoryStream.Write(this._boolBuffer, 0, 1);
        }

        public void Encode(IEncodable value) {
            bool hasValue = value != null;
            this.Encode(hasValue);

            if (hasValue) {
                value.Encode(this);
            }
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

    public static class BinaryEncoder {
        public static int Encode<TEncodable>(TEncodable encodable, byte[] intoBuffer, int index) where TEncodable : IEncodable {
            _Encoder encoder = new _Encoder();
            encoder.Rent();
            encodable.Encode(encoder);
            var len = encoder.PutBytes(intoBuffer, index);
            encoder.Pay();
            return len;
        }
    }
}
