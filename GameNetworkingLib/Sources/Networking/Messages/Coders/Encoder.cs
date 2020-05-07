using System;
using System.IO;
using System.Text;
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

    sealed class _Encoder : IEncoder, IDisposable {
        private ShortByteArrayConverter _shortConverter = new ShortByteArrayConverter(0);
        private UShortByteArrayConverter _ushortConverter = new UShortByteArrayConverter(0);
        private IntByteArrayConverter _intConverter = new IntByteArrayConverter(0);
        private UIntByteArrayConverter _uintConverter = new UIntByteArrayConverter(0);
        private LongByteArrayConverter _longConverter = new LongByteArrayConverter(0L);
        private ULongByteArrayConverter _ulongConverter = new ULongByteArrayConverter(0L);
        private FloatByteArrayConverter _floatConverter = new FloatByteArrayConverter(0F);
        private DoubleByteArrayConverter _doubleConverter = new DoubleByteArrayConverter(0F);
        private readonly byte[] _boolBuffer = new byte[1];
        private readonly MemoryStream _memoryStream;

        public _Encoder() {
            this._memoryStream = new MemoryStream();
        }

        public int PutBytes(byte[] buffer, int index) {
            var memBuff = this._memoryStream.GetBuffer();
            var length = (int)this._memoryStream.Length;
            Array.Copy(memBuff, 0, buffer, index, length);
            this._memoryStream.SetLength(0);
            return length;
        }

        public void Dispose() {
            this._memoryStream.Dispose();
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
            var bytes = Encoding.UTF8.GetBytes(value);
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
    }

    public static class BinaryEncoder {
        public static readonly IEncoder shared = new _Encoder();

        public static int Encode<TEncodable>(TEncodable encodable, byte[] intoBuffer, int index) where TEncodable : IEncodable {
            _Encoder encoder = (_Encoder)shared;
            encodable.Encode(encoder);
            return encoder.PutBytes(intoBuffer, index);
        }
    }
}
