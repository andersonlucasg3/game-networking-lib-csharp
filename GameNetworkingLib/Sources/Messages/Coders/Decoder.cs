using System.IO;

namespace Messages.Coders {
    internal sealed class Decoder : IDecoder {
        internal BinaryReader reader;

        internal Decoder(byte[] buffer) {
            this.reader = new BinaryReader(new MemoryStream(buffer));
        }

        internal Decoder(Stream stream) {
            this.reader = new BinaryReader(stream);
        }

        ~Decoder() {
            this.reader.Dispose();
        }

        public int Int() {
            return this.reader.ReadInt32();
        }

        public short Short() {
            return this.reader.ReadInt16();
        }

        public long Long() {
            return this.reader.ReadInt64();
        }

        public uint UInt() {
            return this.reader.ReadUInt32();
        }

        public ushort UShort() {
            return this.reader.ReadUInt16();
        }

        public ulong ULong() {
            return this.reader.ReadUInt64();
        }

        public float Float() {
            return this.reader.ReadSingle();
        }

        public double Double() {
            return this.reader.ReadDouble();
        }

        public string String() {
            return this.reader.ReadString();
        }

        public byte[] Bytes() {
            int length = this.reader.ReadInt32();
            return this.reader.ReadBytes(length);
        }

        public bool Bool() {
            return this.reader.ReadBoolean();
        }

        public T Object<T>() where T : class, IDecodable, new() {
            if (this.reader.ReadBoolean()) {
                T value = new T();
                value.Decode(this);
                return value;
            }
            return null;
        }
    }

    namespace Binary {
        public sealed class Decoder {
            public T Decode<T>(byte[] buffer) where T : IDecodable, new() {
                Coders.Decoder decoder = new Coders.Decoder(buffer);
                T value = new T();
                value.Decode(decoder);
                return value;
            }
        }
    }
}
