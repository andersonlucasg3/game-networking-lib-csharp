#if !UNITY_64

using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Logging;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Streams;
using GameNetworking.Commons;
using System.Linq;
using System.Text;
using GameNetworking.Messages.Coders.Converters;
using System.Net.NetworkInformation;
using System.Threading;

namespace Tests.IO {
    public class IOTests {
        void Measure(Action action, string name) {
            TimeSpan[] times = new TimeSpan[10];
            for (int index = 0; index < times.Length; index++) {
                DateTime start = DateTime.Now;
                action.Invoke();
                times[index] = DateTime.Now - start;
            }

            TimeSpan timeItTook = times.Aggregate(TimeSpan.FromSeconds(0), (current, each) => current + each) / times.Length;
            Logger.Log($"{name} took (ms) {timeItTook.TotalMilliseconds}");
        }

        [Test]
        public void TestEncoder() {
            Value value = new Value(new SubValue(new SubSubValue("")), Value.bytes);
            this.Measure(() => {
                byte[] buffer = new byte[8 * 1024];
                BinaryEncoder.Encode(value, buffer, 0);
            }, "Encoder");
        }

        [Test]
        public void TestDecoder() {
            Value value = new Value(new SubValue(new SubSubValue("")), Value.bytes);
            byte[] buffer = new byte[8 * 1024];
            int length = BinaryEncoder.Encode(value, buffer, 0);

            this.Measure(() => {
                _ = BinaryDecoder.Decode<Value>(buffer, 0, length);
            }, "Decoder");
        }

        [Test]
        public void TestEncoderDecoder() {
            Value value = new Value(new SubValue(new SubSubValue("")), Value.bytes);

            Value decoded = new Value();

            this.Measure(() => {
                byte[] buffer = new byte[8 * 1024];
                int length = BinaryEncoder.Encode(value, buffer, 0);

                Logger.Log($"Encoded message size: {length}");

                decoded = BinaryDecoder.Decode<Value>(buffer, 0, length);
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
            Assert.AreEqual(value.subValue?.subSubValue.empty, decoded.subValue?.subSubValue.empty);
        }

        [Test]
        public void TestBinaryEncoder() {
            BinaryFormatter formatter = new BinaryFormatter();

            Value value = new Value();

            Value? decoded = null;

            MemoryStream ms = new MemoryStream();

            this.Measure(() => {
                formatter.Serialize(ms, value);

                Logger.Log($"Encoded message size: {ms.Length}");

                ms.Seek(0, SeekOrigin.Begin);

                decoded = (Value)formatter.Deserialize(ms);
            }, "BinaryFormatter");

            Assert.AreEqual(value.intVal, decoded?.intVal);
            Assert.AreEqual(value.shortVal, decoded?.shortVal);
            Assert.AreEqual(value.longVal, decoded?.longVal);
            Assert.AreEqual(value.uintVal, decoded?.uintVal);
            Assert.AreEqual(value.ushortVal, decoded?.ushortVal);
            Assert.AreEqual(value.ulongVal, decoded?.ulongVal);
            Assert.AreEqual(value.stringVal, decoded?.stringVal);
            Assert.AreEqual(value.bytesVal, decoded?.bytesVal);
            Assert.AreEqual(value.subValue, decoded?.subValue);
        }

        [Test]
        public void TestPartialStreamingDecoding() {
            var firstToken = "asldkfjalksdjfalkjsdf";
            var username = "andersonlucasg3";
            var secondToken = "asdlkfalksjdgklashdioohweg";
            var ip = "10.0.0.1";
            var port = (short)6109;


            var loginRequest = new LoginRequest {
                accessToken = firstToken,
                username = username
            };

            var matchRequest = new MatchRequest();

            var connectRequest = new ConnectGameInstanceResponse {
                token = secondToken,
                ip = ip,
                port = port
            };

            var encoder = new MessageStreamWriter();
            List<byte> data = new List<byte>();
            encoder.Write(loginRequest);
            encoder.Use((buffer, count) => data.AddRange(buffer, count));
            encoder.Write(matchRequest);
            encoder.Use((buffer, count) => data.AddRange(buffer, count));
            encoder.Write(connectRequest);
            encoder.Use((buffer, count) => data.AddRange(buffer, count));

            var decoder = new MessageStreamReader();

            this.Measure(() => {
                var position = 0;
                do {
                    var x = data.GetRange(position, 1).ToArray();
                    decoder.Add(x, x.Length);
                    var container = decoder.Decode();
                    if (container.HasValue) {
                        if (container.Value.Is(200)) { // LoginRequest
                            var message = container.Value.Parse<LoginRequest>();
                            Assert.AreEqual(message.accessToken, firstToken);
                            Assert.AreEqual(message.username, username);
                        } else if (container.Value.Is(201)) { // MatchRequest
                            var message = container.Value.Parse<MatchRequest>();
                            Assert.AreNotEqual(message, null);
                        } else if (container.Value.Is(202)) { // ConnectGameInstanceResponse
                            var message = container.Value.Parse<ConnectGameInstanceResponse>();
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
            LoginRequest request = new LoginRequest {
                accessToken = "asdfasdfasdf",
                username = "andersonlucasg3"
            };

            byte[] buffer = new byte[8 * 1024];
            int size = BinaryEncoder.Encode(request, buffer, 0);
            Logger.Log($"LoginRequest Message size: {size}");
        }

        [Test]
        public void TestArraySearchComplexity() {
            byte[] bytes = new byte[] {
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            };

            Console.WriteLine($"Total bytes: {bytes.Length}");

            var delimiter = Encoding.ASCII.GetBytes("\r\r\r\r\r\r");

            Array.Copy(delimiter, 0, bytes, 640, delimiter.Length);

            int index = 0;
            this.Measure(() => {
                index = ArraySearch.IndexOf(bytes, delimiter, bytes.Length);
            }, "ArraySearch-\\r");

            Console.WriteLine($"Location index: {index}");

            Assert.AreEqual(640, index);
        }

        [Test]
        public void TestArraySearchLongComplexity() {
            byte[] bytes = new byte[] {
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            };

            Console.WriteLine($"Total bytes: {bytes.Length}");

            var delimiter = Encoding.ASCII.GetBytes("942gh2hg249");

            Array.Copy(delimiter, 0, bytes, 640, delimiter.Length);

            int index = 0;
            this.Measure(() => {
                index = ArraySearch.IndexOf(bytes, delimiter, bytes.Length);
            }, "ArraySearchLong");

            Console.WriteLine($"Location index: {index}");

            Assert.AreEqual(640, index);
        }

        [Test]
        public void TestEndianess() {
            Console.WriteLine($"Is Little Endian: {BitConverter.IsLittleEndian}");

            int value = 15395;
            byte[] bigEndianBytes = BitConverter.GetBytes(value);
            Array.Reverse(bigEndianBytes);

            IntByteArrayConverter converter = new IntByteArrayConverter();
            converter.array = bigEndianBytes;

            Assert.AreEqual(value, converter.value);
        }

        [Test]
        public void TestMessageChecksum() {
            byte[] bytes = new byte[] {
                4,6,4,6,6,46,6,34,64,2,64,62,47,27,247
            };

            var calculatedChecksum = CoderHelper.ComputeAdditionChecksum(bytes, 0, bytes.Length);

            var checksum = 0;
            for (int index = 0; index < bytes.Length; index++) {
                checksum += bytes[index];
            }

            Assert.AreEqual(checksum, calculatedChecksum);

            var writer = new MessageStreamWriter();
            var reader = new MessageStreamReader();

            var loginRequest = new LoginRequest() { accessToken = "alsdjflakjsdf", username = "meu username" };
            var matchRequest = new MatchRequest() { value1 = 1, value2 = 2, value3 = 3, value4 = 4 };

            writer.Write(loginRequest);
            writer.Write(matchRequest);
            writer.Use((buffer, len) => {
                reader.Add(buffer, len);
                var message = reader.Decode();
                Assert.IsTrue(message.Value.Is(200));
                Assert.AreEqual(loginRequest, message.Value.Parse<LoginRequest>());
                message = reader.Decode();
                Assert.IsTrue(message.Value.Is(201));
                Assert.AreEqual(matchRequest, message.Value.Parse<MatchRequest>());
                writer.DidWrite(len);
            });

            writer.Write(loginRequest);
            writer.Write(matchRequest);
            writer.currentBuffer[0] = 23;
            writer.Use((buffer, len) => {
                reader.Add(buffer, len);
                var message = reader.Decode();
                Assert.IsFalse(message.HasValue);
                Assert.IsTrue(message == null);
                message = reader.Decode();
                Assert.IsTrue(message.Value.Is(201));
                Assert.AreEqual(matchRequest, message.Value.Parse<MatchRequest>());
                writer.DidWrite(len);
            });
        }

        [Test]
        public void TestMultithreadReadAndWrite() {
            var loginRequest = new LoginRequest() { accessToken = "askldfaljksdf", username = "anderson" };

            var reader = new MessageStreamReader();
            var writer = new MessageStreamWriter();

            bool isReading = true;

            ThreadPool.QueueUserWorkItem(_ => {
                Thread.CurrentThread.Name = "Writer";
                for (int index = 0; index < 100; index++) {
                    writer.Write(loginRequest);
                }
            });

            ThreadPool.QueueUserWorkItem(_ => {
                Thread.CurrentThread.Name = "Reader";
                do {
                    writer.Use((buffer, len) => {
                        reader.Add(buffer, len);
                        var message = reader.Decode();
                        if (message.HasValue) {
                            Assert.AreEqual(loginRequest, message.Value.Parse<LoginRequest>());
                        }
                        writer.DidWrite(len);
                    });
                } while (isReading);
            });

            Thread.Sleep(5000);

            isReading = false;

            Assert.AreEqual(0, writer.currentBufferLength);
            Assert.AreEqual(0, reader.currentBufferLength);
        }
    }

    struct LoginRequest : ITypedMessage {
        int ITypedMessage.type => 200;

        public string accessToken;
        public string username;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.accessToken);
            encoder.Encode(this.username);
        }

        public void Decode(IDecoder decoder) {
            this.accessToken = decoder.GetString();
            this.username = decoder.GetString();
        }
    }

    struct MatchRequest : ITypedMessage {
        int ITypedMessage.type => 201;

        public int value1;
        public int value2;
        public int value3;
        public int value4;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.value1);
            encoder.Encode(this.value2);
            encoder.Encode(this.value3);
            encoder.Encode(this.value4);
        }

        public void Decode(IDecoder decoder) {
            this.value1 = decoder.GetInt();
            this.value2 = decoder.GetInt();
            this.value3 = decoder.GetInt();
            this.value4 = decoder.GetInt();
        }
    }

    struct ConnectGameInstanceResponse : ITypedMessage {
        int ITypedMessage.type => 202;

        public string token;
        public string ip;
        public short port;

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.token);
            encoder.Encode(this.ip);
            encoder.Encode(this.port);
        }

        public void Decode(IDecoder decoder) {
            this.token = decoder.GetString();
            this.ip = decoder.GetString();
            this.port = decoder.GetShort();
        }
    }

    [Serializable]
    struct Value : ITypedMessage {
        public static readonly byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Minha string preferida em bytes");

        int ITypedMessage.type => 100;

        public int intVal;
        public short shortVal;
        public long longVal;
        public uint uintVal;
        public ushort ushortVal;
        public ulong ulongVal;

        public string stringVal;
        public byte[] bytesVal;

        public SubValue? subValue;

        public Value(SubValue subValue, byte[] bytesVal, int intVal = 1, short shortVal = 2, long longVal = 3,
            uint uintVal = 4, ushort ushortVal = 5, ulong ulongVal = 6,
            string stringVal = "Minha string preferida") {
            this.bytesVal = bytesVal;
            this.intVal = intVal;
            this.shortVal = shortVal;
            this.longVal = longVal;
            this.uintVal = uintVal;
            this.ushortVal = ushortVal;
            this.ulongVal = ulongVal;
            this.stringVal = stringVal;
            this.subValue = subValue;
        }

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
            this.intVal = decoder.GetInt();
            this.shortVal = decoder.GetShort();
            this.longVal = decoder.GetLong();
            this.uintVal = decoder.GetUInt();
            this.ushortVal = decoder.GetUShort();
            this.ulongVal = decoder.GetULong();
            this.stringVal = decoder.GetString();
            this.bytesVal = decoder.GetBytes();
            var subValue = decoder.GetObject<SubValue>();
            if (subValue.HasValue) {
                this.subValue = subValue.Value;
            }
        }
    }

    [Serializable]
    struct SubValue : ITypedMessage {
        int ITypedMessage.type => 101;

        public string name;
        public int age;
        public float height;
        public double weight;

        public SubSubValue subSubValue;

        public SubValue(SubSubValue subSubValue, string name = "Meu nome", int age = 30, float height = 1.95F,
            double weight = 110F) {
            this.name = name;
            this.age = age;
            this.height = height;
            this.weight = weight;
            this.subSubValue = subSubValue;
        }

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.name);
            encoder.Encode(this.age);
            encoder.Encode(this.height);
            encoder.Encode(this.weight);
            encoder.Encode(this.subSubValue);
        }

        public void Decode(IDecoder decoder) {
            this.name = decoder.GetString();
            this.age = decoder.GetInt();
            this.height = decoder.GetFloat();
            this.weight = decoder.GetDouble();
            var subSubValue = decoder.GetObject<SubSubValue>();
            if (subSubValue.HasValue) {
                this.subSubValue = subSubValue.Value;
            }
        }

        public override bool Equals(object obj) {
            if (obj is SubValue other) {
                return this.name == other.name &&
                    this.age == other.age;
            }
            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(this.name, this.age, this.height, this.weight);
        }
    }

    [Serializable]
    struct SubSubValue : ITypedMessage {
        int ITypedMessage.type => 102;

        public string empty;

        public SubSubValue(string empty) {
            this.empty = empty;
        }

        public void Encode(IEncoder encoder) {
            encoder.Encode(this.empty);
        }

        public void Decode(IDecoder decoder) {
            this.empty = decoder.GetString();
        }
    }
}

#endif