#if !UNITY_64

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using GameNetworking.Commons;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Coders.Converters;
using GameNetworking.Messages.Models;
using GameNetworking.Messages.Streams;
using Logging;
using NUnit.Framework;

namespace Tests.IO
{
    public class IOTests
    {
        private static void Measure(Action action, string name)
        {
            var times = new TimeSpan[10];
            for (var index = 0; index < times.Length; index++)
            {
                var start = DateTime.Now;
                action.Invoke();
                times[index] = DateTime.Now - start;
            }

            var timeItTook = times.Aggregate(TimeSpan.FromSeconds(0), (current, each) => current + each) / times.Length;
            Logger.Log($"{name} took (ms) {timeItTook.TotalMilliseconds}");
        }

        [Test]
        public void TestEncoder()
        {
            var value = new Value(new SubValue(new SubSubValue("")), Value.bytes);
            Measure(() =>
            {
                var buffer = new byte[8 * 1024];
                BinaryEncoder.Encode(value, buffer, 0);
            }, "Encoder");
        }

        [Test]
        public void TestDecoder()
        {
            var value = new Value(new SubValue(new SubSubValue("")), Value.bytes);
            var buffer = new byte[8 * 1024];
            var length = BinaryEncoder.Encode(value, buffer, 0);

            Measure(() => { _ = BinaryDecoder.Decode<Value>(buffer, 0, length); }, "Decoder");
        }

        [Test]
        public void TestEncoderDecoder()
        {
            var value = new Value(new SubValue(new SubSubValue("")), Value.bytes);

            var decoded = new Value();

            Measure(() =>
            {
                var buffer = new byte[8 * 1024];
                var length = BinaryEncoder.Encode(value, buffer, 0);

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
        public void TestBinaryEncoder()
        {
            var formatter = new BinaryFormatter();

            var value = new Value();

            Value? decoded = null;

            var ms = new MemoryStream();

            Measure(() =>
            {
                formatter.Serialize(ms, value);

                Logger.Log($"Encoded message size: {ms.Length}");

                ms.Seek(0, SeekOrigin.Begin);

                decoded = (Value) formatter.Deserialize(ms);
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
        public void TestPartialStreamingDecoding()
        {
            var firstToken = "asldkfjalksdjfalkjsdf";
            var username = "andersonlucasg3";
            var secondToken = "asdlkfalksjdgklashdioohweg";
            var ip = "10.0.0.1";
            var port = (short) 6109;


            var loginRequest = new LoginRequest
            {
                accessToken = firstToken,
                username = username
            };

            var matchRequest = new MatchRequest();

            var connectRequest = new ConnectGameInstanceResponse
            {
                token = secondToken,
                ip = ip,
                port = port
            };

            var encoder = new MessageStreamWriter();
            PooledList<byte> data = PooledList<byte>.Rent();
            encoder.Write(loginRequest);
            encoder.Use((buffer, count) => data.AddRange(buffer, count));
            encoder.Write(matchRequest);
            encoder.Use((buffer, count) => data.AddRange(buffer, count));
            encoder.Write(connectRequest);
            encoder.Use((buffer, count) => data.AddRange(buffer, count));

            var decoder = new MessageStreamReader();

            Measure(() =>
            {
                var position = 0;
                do
                {
                    var x = data.GetRange(position, 1).ToArray();
                    decoder.Add(x, x.Length);
                    var container = decoder.Decode();
                    if (container != null)
                    {
                        if (container.Is(200))
                        {
                            // LoginRequest
                            var message = container.Parse<LoginRequest>();
                            Assert.AreEqual(message.accessToken, firstToken);
                            Assert.AreEqual(message.username, username);
                        }
                        else if (container.Is(201))
                        {
                            // MatchRequest
                            var message = container.Parse<MatchRequest>();
                            Assert.AreNotEqual(message, null);
                        }
                        else if (container.Is(202))
                        {
                            // ConnectGameInstanceResponse
                            var message = container.Parse<ConnectGameInstanceResponse>();
                            Assert.AreEqual(message.ip, ip);
                            Assert.AreEqual(message.port, port);
                            Assert.AreEqual(message.token, secondToken);
                        }
                    }

                    position += 1;
                } while (position < data.Count);
            }, "Partial Stream Decoding");
            
            data.Dispose();
        }

        [Test]
        public void TestMessageSize()
        {
            var request = new LoginRequest
            {
                accessToken = "asdfasdfasdf",
                username = "andersonlucasg3"
            };

            var buffer = new byte[8 * 1024];
            var size = BinaryEncoder.Encode(request, buffer, 0);
            Logger.Log($"LoginRequest Message size: {size}");
        }

        [Test]
        public void TestArraySearchComplexity()
        {
            byte[] bytes =
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };

            Console.WriteLine($"Total bytes: {bytes.Length}");

            var delimiter = Encoding.ASCII.GetBytes("\r\r\r\r\r\r");

            Array.Copy(delimiter, 0, bytes, 640, delimiter.Length);

            var index = 0;
            Measure(() => { index = ArraySearch.IndexOf(delimiter, delimiter.Length, bytes, bytes.Length); }, "ArraySearch-\\r");

            Console.WriteLine($"Location index: {index}");

            Assert.AreEqual(640, index);
        }

        [Test]
        public void TestArraySearchLongComplexity()
        {
            byte[] bytes =
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };

            Console.WriteLine($"Total bytes: {bytes.Length}");

            var delimiter = Encoding.ASCII.GetBytes("942gh2hg249");

            Array.Copy(delimiter, 0, bytes, 640, delimiter.Length);

            var index = 0;
            Measure(() => { index = ArraySearch.IndexOf(delimiter, delimiter.Length, bytes, bytes.Length); }, "ArraySearchLong");

            Console.WriteLine($"Location index: {index}");

            Assert.AreEqual(640, index);
        }

        [Test]
        public void TestEndian()
        {
            Console.WriteLine($"Is Little Endian: {BitConverter.IsLittleEndian}");

            const int value = 15395;
            var bigEndianBytes = BitConverter.GetBytes(value);

            var converter = new IntByteArrayConverter
            {
                array = bigEndianBytes
            };

            Assert.AreEqual(value, converter.value);
        }

        [Test]
        public void TestMessageChecksum()
        {
            byte[] bytes =
            {
                4, 6, 4, 6, 6, 46, 6, 34, 64, 2, 64, 62, 47, 27, 247,
                4, 6, 4, 6, 6, 46, 6, 34, 64, 2, 64, 62, 47, 27, 247
            };

            var bigBytes = new byte[8 * 1024];
            Array.Copy(bytes, bigBytes, bytes.Length);

            var calculatedChecksum = CoderHelper.CalculateChecksum(bytes, 0, bytes.Length);

            var newLength = CoderHelper.AddChecksum(bigBytes, 0, bytes.Length) + bytes.Length;

            var checksumInBigBytes = bigBytes[bytes.Length];

            Assert.AreEqual(checksumInBigBytes, calculatedChecksum);

            var writer = new MessageStreamWriter();
            var reader = new MessageStreamReader();

            var loginRequest = new LoginRequest {accessToken = "alsdjflakjsdf", username = "meu username"};
            var matchRequest = new MatchRequest {value1 = 1, value2 = 2, value3 = 3, value4 = 4};

            writer.Write(loginRequest);
            writer.Write(matchRequest);
            writer.Use((buffer, len) =>
            {
                reader.Add(buffer, len);
                var message = reader.Decode();
                Assert.IsTrue(message.Is(200));
                Assert.AreEqual(loginRequest, message.Parse<LoginRequest>());
                message = reader.Decode();
                Assert.IsTrue(message.Is(201));
                Assert.AreEqual(matchRequest, message.Parse<MatchRequest>());
                writer.DidWrite(len);
            });

            writer.Write(loginRequest);
            writer.Write(matchRequest);
            writer.currentBuffer[0] = 23;
            writer.Use((buffer, len) =>
            {
                reader.Add(buffer, len);
                var message = reader.Decode();
                Assert.IsFalse(message != null);
                Assert.IsTrue(message == null);
                message = reader.Decode();
                Assert.IsTrue(message.Is(201));
                Assert.AreEqual(matchRequest, message.Parse<MatchRequest>());
                writer.DidWrite(len);
            });
        }

        [Test]
        public void TestMultiThreadReadAndWrite()
        {
            var loginRequest = new LoginRequest {accessToken = "askldfaljksdf", username = "anderson"};

            var reader = new MessageStreamReader();
            var writer = new MessageStreamWriter();

            for (var reading = 0; reading < 50; reading++)
            {
                for (var write = 0; write < 2; write++)
                {
                    writer.Write(loginRequest);
                    Logger.Log($"Did Write loginRequest {write * reading}");
                }

                writer.Use((buffer, len) =>
                {
                    Logger.Log($"Using buffer with len: {len}");
                    reader.Add(buffer, len);
                    MessageContainer message = null;
                    while ((message = reader.Decode()) != null)
                    {
                        Logger.Log($"Decoded message: {message.type}");
                        Assert.AreEqual(loginRequest, message.Parse<LoginRequest>());
                    }

                    writer.DidWrite(len);
                    Logger.Log($"Did Write len: {len}");
                });
            }

            Assert.AreEqual(0, writer.currentBufferLength);
            Assert.AreEqual(0, reader.currentBufferLength);
        }
    }

    internal struct LoginRequest : ITypedMessage
    {
        int ITypedMessage.type => 200;

        public string accessToken;
        public string username;

        public void Encode(IEncoder encoder)
        {
            encoder.Encode(accessToken);
            encoder.Encode(username);
        }

        public void Decode(IDecoder decoder)
        {
            accessToken = decoder.GetString();
            username = decoder.GetString();
        }
    }

    internal struct MatchRequest : ITypedMessage
    {
        int ITypedMessage.type => 201;

        public int value1;
        public int value2;
        public int value3;
        public int value4;

        public void Encode(IEncoder encoder)
        {
            encoder.Encode(value1);
            encoder.Encode(value2);
            encoder.Encode(value3);
            encoder.Encode(value4);
        }

        public void Decode(IDecoder decoder)
        {
            value1 = decoder.GetInt();
            value2 = decoder.GetInt();
            value3 = decoder.GetInt();
            value4 = decoder.GetInt();
        }
    }

    internal struct ConnectGameInstanceResponse : ITypedMessage
    {
        int ITypedMessage.type => 202;

        public string token;
        public string ip;
        public short port;

        public void Encode(IEncoder encoder)
        {
            encoder.Encode(token);
            encoder.Encode(ip);
            encoder.Encode(port);
        }

        public void Decode(IDecoder decoder)
        {
            token = decoder.GetString();
            ip = decoder.GetString();
            port = decoder.GetShort();
        }
    }

    [Serializable]
    internal struct Value : ITypedMessage
    {
        public static readonly byte[] bytes = Encoding.ASCII.GetBytes("Minha string preferida em bytes");

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
            string stringVal = "Minha string preferida")
        {
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

        public void Encode(IEncoder encoder)
        {
            encoder.Encode(intVal);
            encoder.Encode(shortVal);
            encoder.Encode(longVal);
            encoder.Encode(uintVal);
            encoder.Encode(ushortVal);
            encoder.Encode(ulongVal);
            encoder.Encode(stringVal);
            encoder.Encode(bytesVal);
            encoder.Encode(subValue);
        }

        public void Decode(IDecoder decoder)
        {
            intVal = decoder.GetInt();
            shortVal = decoder.GetShort();
            longVal = decoder.GetLong();
            uintVal = decoder.GetUInt();
            ushortVal = decoder.GetUShort();
            ulongVal = decoder.GetULong();
            stringVal = decoder.GetString();
            bytesVal = decoder.GetBytes();
            var subValue = decoder.GetObject<SubValue>();
            if (subValue.HasValue) this.subValue = subValue.Value;
        }
    }

    [Serializable]
    internal struct SubValue : ITypedMessage
    {
        int ITypedMessage.type => 101;

        public string name;
        public int age;
        public float height;
        public double weight;

        public SubSubValue subSubValue;

        public SubValue(SubSubValue subSubValue, string name = "Meu nome", int age = 30, float height = 1.95F,
            double weight = 110F)
        {
            this.name = name;
            this.age = age;
            this.height = height;
            this.weight = weight;
            this.subSubValue = subSubValue;
        }

        public void Encode(IEncoder encoder)
        {
            encoder.Encode(name);
            encoder.Encode(age);
            encoder.Encode(height);
            encoder.Encode(weight);
            encoder.Encode(subSubValue);
        }

        public void Decode(IDecoder decoder)
        {
            name = decoder.GetString();
            age = decoder.GetInt();
            height = decoder.GetFloat();
            weight = decoder.GetDouble();
            var subSubValue = decoder.GetObject<SubSubValue>();
            if (subSubValue.HasValue) this.subSubValue = subSubValue.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is SubValue other)
                return name == other.name &&
                       age == other.age;
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, age, height, weight);
        }
    }

    [Serializable]
    internal struct SubSubValue : ITypedMessage
    {
        int ITypedMessage.type => 102;

        public string empty;

        public SubSubValue(string empty)
        {
            this.empty = empty;
        }

        public void Encode(IEncoder encoder)
        {
            encoder.Encode(empty);
        }

        public void Decode(IDecoder decoder)
        {
            empty = decoder.GetString();
        }
    }
}

#endif
