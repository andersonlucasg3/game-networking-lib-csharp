using System.IO;

namespace Messages.Coders {
    internal sealed class Decoder : IDecoder, System.IDisposable {
        internal BinaryReader reader;

        internal Decoder(byte[] buffer) {
            this.reader = new BinaryReader(new MemoryStream(buffer));
        }

        internal Decoder(Stream stream) {
            this.reader = new BinaryReader(stream);
        }

        public int GetInt() {
            return this.reader.ReadInt32();
        }

        public short GetShort() {
            return this.reader.ReadInt16();
        }

        public long GetLong() {
            return this.reader.ReadInt64();
        }

        public uint GetUInt() {
            return this.reader.ReadUInt32();
        }

        public ushort GetUShort() {
            return this.reader.ReadUInt16();
        }

        public ulong GetULong() {
            return this.reader.ReadUInt64();
        }

        public float GetFloat() {
            return this.reader.ReadSingle();
        }

        public double GetDouble() {
            return this.reader.ReadDouble();
        }

        public string GetString() {
            return this.reader.ReadString();
        }

        public byte[] GetBytes() {
            int length = this.reader.ReadInt32();
            return this.reader.ReadBytes(length);
        }

        public bool GetBool() {
            return this.reader.ReadBoolean();
        }

        public T GetObject<T>() where T : class, IDecodable, new() {
            if (this.reader.ReadBoolean()) {
                T value = new T();
                value.Decode(this);
                return value;
            }
            return null;
        }

        public void Dispose() {
            this.reader.Dispose();
        }
    }

    namespace Binary {
        public sealed class Decoder {
            public static TDecodable Decode<TDecodable>(byte[] buffer) where TDecodable : IDecodable, new() {
                Coders.Decoder decoder = new Coders.Decoder(buffer);
                TDecodable value = new TDecodable();
                value.Decode(decoder);
                decoder.Dispose();
                return value;
            }
        }
    }
}
