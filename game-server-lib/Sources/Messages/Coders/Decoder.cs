using System.IO;

namespace Messages.Coders {
    internal sealed class Decoder: IDecoder {
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

        public int DecodeInt() {
            return this.reader.ReadInt32();
        }

        public short DecodeShort() {
            return this.reader.ReadInt16();
        }

        public long DecodeLong() {
            return this.reader.ReadInt64();
        }

        public uint DecodeUInt() {
            return this.reader.ReadUInt32();
        }

        public ushort DecodeUShort() {
            return this.reader.ReadUInt16();
        }

        public ulong DecodeULong() {
            return this.reader.ReadUInt64();
        }

        public float DecodeFloat() {
            return this.reader.ReadSingle();
        }

        public double DecodeDouble() {
            return this.reader.ReadDouble();
        }

        public string DecodeString() {
            return this.reader.ReadString();
        }

        public byte[] DecodeBytes() {
            int length = this.reader.ReadInt32();
            return this.reader.ReadBytes(length);
        }

        public T Decode<T>() where T : class, IDecodable, new() {
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
            public T Decode<T>(byte[] buffer) where T: IDecodable, new() {
                Coders.Decoder decoder = new Coders.Decoder(buffer);
                T value = new T();
                value.Decode(decoder);
                return value;
            }
        }
    }
}
