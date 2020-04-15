using System;
using System.IO;

namespace Messages.Coders {
    internal sealed class Encoder : IEncoder, IDisposable {
        internal BinaryWriter writer;

        internal Encoder() {
            this.writer = new BinaryWriter(new MemoryStream());
        }

        internal Encoder(Stream stream) {
            this.writer = new BinaryWriter(stream);
        }

        public void Dispose() {
            this.writer.Dispose();
        }

        public void Encode(int value) {
            this.writer.Write(value);
        }

        public void Encode(short value) {
            this.writer.Write(value);
        }

        public void Encode(long value) {
            this.writer.Write(value);
        }

        public void Encode(uint value) {
            this.writer.Write(value);
        }

        public void Encode(ushort value) {
            this.writer.Write(value);
        }

        public void Encode(ulong value) {
            this.writer.Write(value);
        }

        public void Encode(float value) {
            this.writer.Write(value);
        }

        public void Encode(double value) {
            this.writer.Write(value);
        }

        public void Encode(string value) {
            this.writer.Write(value);
        }

        public void Encode(byte[] value) {
            int length = value.Length;
            this.writer.Write(length);
            this.writer.Write(value);
        }

        public void Encode(bool value) {
            this.writer.Write(value);
        }

        public void Encode(IEncodable value) {
            bool hasValue = value != null;
            this.writer.Write(hasValue);

            if (hasValue) {
                Encoder encoder = new Encoder(this.writer.BaseStream);
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

                MemoryStream memoryStream = (MemoryStream)encoder.writer.BaseStream;
                encoder.Dispose();
                return memoryStream.ToArray();
            }
        }
    }
}
