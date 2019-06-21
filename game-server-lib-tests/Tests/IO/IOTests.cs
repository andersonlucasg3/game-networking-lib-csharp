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
        public void TestEncoder() {
            Value value = new Value();
            Encoder encoder = new Encoder();
            this.Measure(() => {
                encoder.Encode(value);
            }, "Encoder");
        }

        [Test]
        public void TestDecoder() {
            Value value = new Value();

            Encoder encoder = new Encoder();
            byte[] encoded = encoder.Encode(value);

            Decoder decoder = new Decoder();
            this.Measure(() => {
                decoder.Decode<Value>(encoded);
            }, "Decoder");
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
                Assert.AreEqual(value.subValue, decoded.subValue);
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
                Assert.AreEqual(value.subValue, decoded.subValue);
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

        public SubValue subValue = new SubValue();

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.intVal);
            encoder.Encode(this.shortVal);
            encoder.Encode(this.longVal);
            encoder.Encode(this.uintVal);
            encoder.Encode(this.ushortVal);
            encoder.Encode(this.ulongVal);
            encoder.Encode(this.stringVal);
            encoder.Encode(this.bytesVal);
            encoder.Encode(this.subValue);
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
            this.subValue = decoder.Decode<SubValue>();
        }
    }

    [Serializable]
    class SubValue: ICodable {
        public string name = "Meu nome";
        public int age = 30;
        public float height = 1.95F;
        public float weight = 110F;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.name);
            encoder.Encode(this.age);
            encoder.Encode(this.height);
            encoder.Encode(this.weight);
        }

        public void Decode(IDecoder decoder) {
            this.name = decoder.DecodeString();
            this.age = decoder.DecodeInt();
            this.height = decoder.DecodeFloat();
            this.weight = decoder.DecodeFloat();
        }

        public override bool Equals(object obj) {
            if (obj is SubValue) {
                SubValue other = obj as SubValue;
                return this.name == other.name &&
                    this.age == other.age &&
                    this.height == other.height &&
                    this.weight == other.weight;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
