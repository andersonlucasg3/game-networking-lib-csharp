using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Messages.Coders {
    internal sealed class Encoder : IEncoder {
        internal BinaryWriter writer;

        internal Encoder() {
            this.writer = new BinaryWriter(new MemoryStream());
        }

        internal Encoder(Stream stream) {
            this.writer = new BinaryWriter(stream);
        }

        ~Encoder() {
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

        public void Encode(IEncodable value) {
            Encoder encoder = new Encoder(this.writer.BaseStream);
            value.Encode(encoder);
        }
    }

    namespace Binary {
        public sealed class Encoder {
            public byte[] Encode(IEncodable value) {
                Coders.Encoder encoder = new Coders.Encoder();

                value.Encode(encoder);

                MemoryStream memoryStream = (MemoryStream)encoder.writer.BaseStream;
                return memoryStream.ToArray();
            }
        }
    }
}