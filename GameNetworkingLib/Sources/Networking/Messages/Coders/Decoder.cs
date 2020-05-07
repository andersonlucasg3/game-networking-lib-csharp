using System;
using System.IO;
using GameNetworking.Messages.Coders.Converters;

namespace GameNetworking.Messages.Coders {
    public sealed class Decoder : IDecoder, System.IDisposable {
        private ShortByteArrayConverter _shortConverter = new ShortByteArrayConverter(0);
        private UShortByteArrayConverter _ushortConverter = new UShortByteArrayConverter(0);
        private IntByteArrayConverter _intConverter = new IntByteArrayConverter(0);
        private UIntByteArrayConverter _uintConverter = new UIntByteArrayConverter(0);
        private LongByteArrayConverter _longConverter = new LongByteArrayConverter(0L);
        private ULongByteArrayConverter _ulongConverter = new ULongByteArrayConverter(0L);
        private FloatByteArrayConverter _floatConverter = new FloatByteArrayConverter(0F);
        private DoubleByteArrayConverter _doubleConverter = new DoubleByteArrayConverter(0F);
        private byte[] _boolBuffer = new byte[1];
        private MemoryStream _memoryStream;

        public Decoder() { }

        public Decoder(MemoryStream stream) {
            this._memoryStream = stream;
        }

        public void SetBuffer(byte[] bytes, int offset, int length) {
            this._memoryStream?.Dispose();
            this._memoryStream = new MemoryStream(bytes, offset, length);
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
            return System.Text.Encoding.UTF8.GetString(bytes);
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

        public T GetObject<T>() where T : class, IDecodable, new() {
            if (this.GetBool()) {
                T value = new T();
                value.Decode(this);
                return value;
            }
            return null;
        }

        public void Dispose() {
            this._memoryStream.Dispose();
        }
    }
}
