using NUnit.Framework;
using Messages.Coders;
using Messages.Coders.Binary;
using Messages.Models;
using Messages.Streams;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Logging;

namespace Tests.IO {
    public class IOTests {
        [SetUp]
        public void Setup() {

        }

        void Measure(Action action, string name) {
            DateTime start = DateTime.Now;
            action.Invoke();
            TimeSpan timeItTook = DateTime.Now - start;
            Logger.Log($"{name} took (ms) {timeItTook.TotalMilliseconds}");
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
            Encoder encoder = new Encoder();
            Decoder decoder = new Decoder();

            Value value = new Value();

            Value decoded = null;

            this.Measure(() => {
                byte[] encoded = encoder.Encode(value);

                Logger.Log($"Encoded message size: {encoded.Length}");

                decoded = decoder.Decode<Value>(encoded);
            }, "Encoder and Decoder");

            Assert.AreEqual(value.intVal, decoded.intVal);
            Assert.AreEqual(value.shortVal, decoded.shortVal);
            Assert.AreEqual(value.longVal, decoded.longVal);
            Assert.AreEqual(value.uintVal, decoded.uintVal);
            Assert.AreEqual(value.ushortVal, decoded.ushortVal);
            Assert.AreEqual(value.ulongVal, decoded.ulongVal);
            Assert.AreEqual(value.stringVal, decoded.stringVal);
            Assert.AreEqual(value.bytesVal, decoded.bytesVal);
            Assert.AreEqual(value.subValue, decoded.subValue);
            Assert.AreEqual(value.subValue.subSubValue.empty, decoded.subValue.subSubValue.empty);
        }

        [Test]
        public void TestBinaryEncoder() {
            BinaryFormatter formatter = new BinaryFormatter();

            Value value = new Value();

            Value decoded = null;

            MemoryStream ms = new MemoryStream();

            this.Measure(() => {
                formatter.Serialize(ms, value);

                Logger.Log($"Encoded message size: {ms.Length}");

                ms.Seek(0, SeekOrigin.Begin);

                decoded = (Value)formatter.Deserialize(ms);
            }, "BinaryFormatter");

            Assert.AreEqual(value.intVal, decoded.intVal);
            Assert.AreEqual(value.shortVal, decoded.shortVal);
            Assert.AreEqual(value.longVal, decoded.longVal);
            Assert.AreEqual(value.uintVal, decoded.uintVal);
            Assert.AreEqual(value.ushortVal, decoded.ushortVal);
            Assert.AreEqual(value.ulongVal, decoded.ulongVal);
            Assert.AreEqual(value.stringVal, decoded.stringVal);
            Assert.AreEqual(value.bytesVal, decoded.bytesVal);
            Assert.AreEqual(value.subValue, decoded.subValue);
        }

        [Test]
        public void TestPartialStreamingDecoding() {
            var firstToken = "asldkfjalksdjfalkjsdf";
            var username = "andersonlucasg3";
            var secondToken = "asdlkfalksjdgklashdioohweg";
            var ip = "10.0.0.1";
            var port = (short)6109;


            var loginRequest = new LoginRequest();
            loginRequest.accessToken = firstToken;
            loginRequest.username = username;

            var matchRequest = new MatchRequest();

            var connectRequest = new ConnectGameInstanceResponse();
            connectRequest.token = secondToken;
            connectRequest.ip = ip;
            connectRequest.port = port;

            var encoder = new MessageStreamWriter();
            List<byte> data = new List<byte>();
            data.AddRange(encoder.Write(loginRequest));
            data.AddRange(encoder.Write(matchRequest));
            data.AddRange(encoder.Write(connectRequest));

            var decoder = new MessageStreamReader();

            this.Measure(() => {
                var position = 0;
                do {
                    decoder.Add(data.GetRange(position, 1).ToArray());
                    var container = decoder.Decode();
                    if (container != null) {
                        if (container.Is(LoginRequest.Type)) {
                            var message = container.Parse<LoginRequest>();
                            Assert.AreEqual(message.accessToken, firstToken);
                            Assert.AreEqual(message.username, username);
                        } else if (container.Is(MatchRequest.Type)) {
                            var message = container.Parse<MatchRequest>();
                            Assert.AreNotEqual(message, null);
                        } else if (container.Is(ConnectGameInstanceResponse.Type)) {
                            var message = container.Parse<ConnectGameInstanceResponse>();
                            Assert.AreEqual(message.ip, ip);
                            Assert.AreEqual(message.port, port);
                            Assert.AreEqual(message.token, secondToken);
                        }
                    }
                    position += 1;
                } while (position < data.Count);
            }, "Partial Stream Decoding");
        }

        [Test]
        public void TestMessageSize() {
            var encoder = new Encoder();

            LoginRequest request = new LoginRequest {
                accessToken = "asdfasdfasdf",
                username = "andersonlucasg3"
            };

            int size = encoder.Encode(request).Length;
            Logger.Log($"LoginRequest Message size: {size}");
        }
    }

    class LoginRequest : ITypedMessage {
        public static int Type {
            get { return 200; }
        }

        int ITypedMessage.Type {
            get { return LoginRequest.Type; }
        }

        public string accessToken;
        public string username;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.accessToken);
            encoder.Encode(this.username);
        }

        public void Decode(IDecoder decoder) {
            this.accessToken = decoder.String();
            this.username = decoder.String();
        }
    }

    class MatchRequest : ITypedMessage {
        public static int Type {
            get { return 201; }
        }

        int ITypedMessage.Type {
            get { return MatchRequest.Type; }
        }

        public void Encode(IEncoder encoder) { }

        public void Decode(IDecoder decoder) { }
    }

    class ConnectGameInstanceResponse : ITypedMessage {
        public static int Type {
            get { return 202; }
        }

        int ITypedMessage.Type {
            get { return ConnectGameInstanceResponse.Type; }
        }

        public string token;
        public string ip;
        public short port;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.token);
            encoder.Encode(this.ip);
            encoder.Encode(this.port);
        }

        public void Decode(IDecoder decoder) {
            this.token = decoder.String();
            this.ip = decoder.String();
            this.port = decoder.Short();
        }
    }

    [Serializable]
    class Value : ITypedMessage {
        public static int Type {
            get { return 100; }
        }

        int ITypedMessage.Type {
            get { return Value.Type; }
        }

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
            this.intVal = decoder.Int();
            this.shortVal = decoder.Short();
            this.longVal = decoder.Long();
            this.uintVal = decoder.UInt();
            this.ushortVal = decoder.UShort();
            this.ulongVal = decoder.ULong();
            this.stringVal = decoder.String();
            this.bytesVal = decoder.Bytes();
            this.subValue = decoder.Object<SubValue>();
        }
    }

    [Serializable]
    class SubValue : ITypedMessage {
        public static int Type {
            get { return 101; }
        }

        int ITypedMessage.Type {
            get { return SubValue.Type; }
        }

        public string name = "Meu nome";
        public int age = 30;
        public float height = 1.95F;
        public float weight = 110F;

        public SubSubValue subSubValue = new SubSubValue();

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.name);
            encoder.Encode(this.age);
            encoder.Encode(this.height);
            encoder.Encode(this.weight);
        }

        public void Decode(IDecoder decoder) {
            this.name = decoder.String();
            this.age = decoder.Int();
            this.height = decoder.Float();
            this.weight = decoder.Float();
        }

        public override bool Equals(object obj) {
            if (obj is SubValue) {
                SubValue other = obj as SubValue;
                return this.name == other.name &&
                    this.age == other.age;
            }
            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    [Serializable]
    class SubSubValue : ITypedMessage {
        public static int Type {
            get { return 102; }
        }

        int ITypedMessage.Type {
            get { return SubSubValue.Type; }
        }

        public string empty;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.empty);
        }

        public void Decode(IDecoder decoder) {
            this.empty = decoder.String();
        }
    }
}
