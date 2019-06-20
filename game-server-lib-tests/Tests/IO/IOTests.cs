using NUnit.Framework;
using Messages.Coders;
using Messages.Coders.Binary;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tests.IO {
    public class IOTests {
        [SetUp]
        public void Setup() {

        }

        void Measure(Action action, string name) {
            DateTime start = DateTime.Now;
            action.Invoke();
            TimeSpan timeItTook = DateTime.Now - start;
            Logging.Logger.Log(typeof(IOTests), name + " took (ms) " + timeItTook.TotalMilliseconds);
        }

        [Test]
        public void TestEncoderDecoder() {
            this.Measure(() => {
                Value value = new Value();

                Encoder encoder = new Encoder();
                byte[] encoded = encoder.Encode(value);


                Decoder decoder = new Decoder();
                Value decoded = decoder.Decode<Value>(encoded);

                Assert.AreEqual(value.intVal, decoded.intVal);
                Assert.AreEqual(value.shortVal, decoded.shortVal);
                Assert.AreEqual(value.longVal, decoded.longVal);
                Assert.AreEqual(value.uintVal, decoded.uintVal);
                Assert.AreEqual(value.ushortVal, decoded.ushortVal);
                Assert.AreEqual(value.ulongVal, decoded.ulongVal);
                Assert.AreEqual(value.stringVal, decoded.stringVal);
                Assert.AreEqual(value.bytesVal, decoded.bytesVal);
            }, "Encoder and Decoder");
        }

        [Test]
        public void TestBinaryEncoder() {
            this.Measure(() => {
                Value value = new Value();

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                formatter.Serialize(ms, value);

                ms.Seek(0, SeekOrigin.Begin);

                Value decoded = (Value)formatter.Deserialize(ms);

                Assert.AreEqual(value.intVal, decoded.intVal);
                Assert.AreEqual(value.shortVal, decoded.shortVal);
                Assert.AreEqual(value.longVal, decoded.longVal);
                Assert.AreEqual(value.uintVal, decoded.uintVal);
                Assert.AreEqual(value.ushortVal, decoded.ushortVal);
                Assert.AreEqual(value.ulongVal, decoded.ulongVal);
                Assert.AreEqual(value.stringVal, decoded.stringVal);
                Assert.AreEqual(value.bytesVal, decoded.bytesVal);
            }, "BinaryFormatter");
        }
    }

    [Serializable]
    class Value: ICodable {
        public int intVal = 1;
        public short shortVal = 2;
        public long longVal = 3;
        public uint uintVal = 4;
        public ushort ushortVal = 5;
        public ulong ulongVal = 6;

        public string stringVal = "Minha string preferida";
        public byte[] bytesVal = System.Text.Encoding.UTF8.GetBytes("Minha string preferida em bytes");

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.intVal);
            encoder.Encode(this.shortVal);
            encoder.Encode(this.longVal);
            encoder.Encode(this.uintVal);
            encoder.Encode(this.ushortVal);
            encoder.Encode(this.ulongVal);
            encoder.Encode(this.stringVal);
            encoder.Encode(this.bytesVal);
        }

        public void Decode(IDecoder decoder) {
            this.intVal = decoder.DecodeInt();
            this.shortVal = decoder.DecodeShort();
            this.longVal = decoder.DecodeLong();
            this.uintVal = decoder.DecodeUInt();
            this.ushortVal = decoder.DecodeUShort();
            this.ulongVal = decoder.DecodeULong();
            this.stringVal = decoder.DecodeString();
            this.bytesVal = decoder.DecodeBytes();
        }
    }
}
