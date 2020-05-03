using System;
using System.IO;
using GameNetworking.Messages.Coders.Converters;

namespace GameNetworking.Messages.Coders {
    internal sealed class Encoder : IEncoder, IDisposable {
        private ShortByteArrayConverter _shortConverter = new ShortByteArrayConverter(0);
        private UShortByteArrayConverter _ushortConverter = new UShortByteArrayConverter(0);
        private IntByteArrayConverter _intConverter = new IntByteArrayConverter(0);
        private UIntByteArrayConverter _uintConverter = new UIntByteArrayConverter(0);
        private LongByteArrayConverter _longConverter = new LongByteArrayConverter(0L);
        private ULongByteArrayConverter _ulongConverter = new ULongByteArrayConverter(0L);
        private FloatByteArrayConverter _floatConverter = new FloatByteArrayConverter(0F);
        private DoubleByteArrayConverter _doubleConverter = new DoubleByteArrayConverter(0F);
        private byte[] _boolBuffer = new byte[1];

        internal MemoryStream memoryStream;

        internal Encoder() {
            this.memoryStream = new MemoryStream();
        }

        internal Encoder(MemoryStream stream) {
            this.memoryStream = stream;
        }

        public void Dispose() {
            this.memoryStream.Dispose();
        }

        public void Encode(int value) {
            this._intConverter.value = value;
            this.memoryStream.Write(this._intConverter.array, 0, sizeof(int));
        }

        public void Encode(short value) {
            this._shortConverter.value = value;
            this.memoryStream.Write(this._shortConverter.array, 0, sizeof(short));
        }

        public void Encode(long value) {
            this._longConverter.value = value;
            this.memoryStream.Write(this._longConverter.array, 0, sizeof(long));
        }

        public void Encode(uint value) {
            this._uintConverter.value = value;
            this.memoryStream.Write(this._uintConverter.array, 0, sizeof(uint));
        }

        public void Encode(ushort value) {
            this._ushortConverter.value = value;
            this.memoryStream.Write(this._ushortConverter.array, 0, sizeof(ushort));
        }

        public void Encode(ulong value) {
            this._ulongConverter.value = value;
            this.memoryStream.Write(this._ulongConverter.array, 0, sizeof(ulong));
        }

        public void Encode(float value) {
            this._floatConverter.value = value;
            this.memoryStream.Write(this._floatConverter.array, 0, sizeof(float));
        }

        public void Encode(double value) {
            this._doubleConverter.value = value;
            this.memoryStream.Write(this._doubleConverter.array, 0, sizeof(double));
        }

        public void Encode(string value) {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            this.memoryStream.Write(bytes, 0, bytes.Length);
        }

        public void Encode(byte[] value) {
            this._intConverter.value = value.Length;
            this.memoryStream.Write(this._intConverter.array, 0, sizeof(int));

            this.memoryStream.Write(value, 0, value.Length);
        }

        public void Encode(bool value) {
            this._boolBuffer[0] = Convert.ToByte(value);
            this.memoryStream.Write(this._boolBuffer, 0, 1);
        }

        public void Encode(IEncodable value) {
            bool hasValue = value != null;
            this.Encode(hasValue);

            if (hasValue) {
                Encoder encoder = new Encoder(this.memoryStream);
                value.Encode(encoder);
            }
        }
    }

    namespace Binary {
        public sealed class Encoder {
            private Encoder() { }

            public static byte[] Encode(IEncodable value) {
                Coders.Encoder encoder = new Coders.Encoder();

                value.Encode(encoder);

                MemoryStream memoryStream = encoder.memoryStream;
                byte[] byteArray = memoryStream.ToArray();
                encoder.Dispose();
                return byteArray;
            }
        }
    }
}
