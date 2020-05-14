using System;
using System.IO;
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

    sealed class _Decoder : IDecoder, IDisposable {
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

        public _Decoder() {
            this._memoryStream = new MemoryStream();
        }

        public void SetBuffer(byte[] bytes, int offset, int length) {
            this._memoryStream.SetLength(0);
            this._memoryStream.Write(bytes, offset, length);
            this._memoryStream.Seek(offset, SeekOrigin.Begin);
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

        public void Dispose() {
            this._memoryStream.Dispose();
        }
    }

    public static class BinaryDecoder {
        public static readonly IDecoder shared = new _Decoder();

        public static TMessage Decode<TMessage>(byte[] fromBuffer, int index, int length) where TMessage : struct, IDecodable {
            var decoder = (_Decoder)shared;
            decoder.SetBuffer(fromBuffer, index, length);
            TMessage message = new TMessage();
            message.Decode(shared);
            return message;
        }
    }
}
