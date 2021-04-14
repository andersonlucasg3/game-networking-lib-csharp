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
            var memBuff = _memoryStream.GetBuffer();
            var length = (int)_memoryStream.Length;
            Array.Copy(memBuff, 0, buffer, index, length);
            return length;
        }

        public void Encode(int value) {
            _intConverter.value = value;
            _memoryStream.Write(_intConverter.array, 0, sizeof(int));
        }

        public void Encode(short value) {
            _shortConverter.value = value;
            _memoryStream.Write(_shortConverter.array, 0, sizeof(short));
        }

        public void Encode(long value) {
            _longConverter.value = value;
            _memoryStream.Write(_longConverter.array, 0, sizeof(long));
        }

        public void Encode(uint value) {
            _uintConverter.value = value;
            _memoryStream.Write(_uintConverter.array, 0, sizeof(uint));
        }

        public void Encode(ushort value) {
            _ushortConverter.value = value;
            _memoryStream.Write(_ushortConverter.array, 0, sizeof(ushort));
        }

        public void Encode(ulong value) {
            _ulongConverter.value = value;
            _memoryStream.Write(_ulongConverter.array, 0, sizeof(ulong));
        }

        public void Encode(float value) {
            _floatConverter.value = value;
            _memoryStream.Write(_floatConverter.array, 0, sizeof(float));
        }

        public void Encode(double value) {
            _doubleConverter.value = value;
            _memoryStream.Write(_doubleConverter.array, 0, sizeof(double));
        }

        public void Encode(string value) {
            var bytes = Encoding.ASCII.GetBytes(value);
            Encode(bytes);
        }

        public void Encode(byte[] value) {
            _intConverter.value = value.Length;
            _memoryStream.Write(_intConverter.array, 0, sizeof(int));

            _memoryStream.Write(value, 0, value.Length);
        }

        public void Encode(bool value) {
            _boolBuffer[0] = Convert.ToByte(value);
            _memoryStream.Write(_boolBuffer, 0, 1);
        }

        public void Encode(IEncodable value) {
            bool hasValue = value != null;
            Encode(hasValue);

            if (hasValue) {
                value.Encode(this);
            }
        }

        internal void Rent() {
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

            _memoryStream.SetLength(0);
        }

        internal void Pay() {
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
