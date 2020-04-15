#if !UNITY_64

using NUnit.Framework;
using System;
using MatchMaking.Protobuf.Coders;
using MatchMaking.Models;
using System.Collections.Generic;
using Logging;

namespace Tests.Protobuf {
    public class CodersTests {
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
            var request = new LoginRequest();
            request.AccessToken = "asdf";
            request.Username = "name";

            var encoder = new MessageEncoder();

            this.Measure(() => {
                encoder.Encode(request);
            }, "Encoder");
        }

        [Test]
        public void TestDecoder() {
            var request = new LoginRequest();
            request.AccessToken = "asdf";
            request.Username = "name";

            var encoder = new MessageEncoder();

            byte[] bytes = encoder.Encode(request);

            var decoder = new MessageDecoder();
            decoder.Add(bytes);

            this.Measure(() => {
                decoder.Decode();
            }, "Decoder");
        }

        [Test]
        public void TestEncoderDecoder() {
            var request = new LoginRequest {
                AccessToken = "asdf",
                Username = "name"
            };

            var encoder = new MessageEncoder();
            var decoder = new MessageDecoder();

            MessageContainer container = null;

            this.Measure(() => {
                byte[] bytes = encoder.Encode(request);

                decoder.Add(bytes);

                container = decoder.Decode();
            }, "Encoder and Decoder");

            Assert.True(container.Is(LoginRequest.Descriptor));

            var decoded = container.Parse<LoginRequest>();

            Assert.AreEqual(request.AccessToken, decoded.AccessToken);
            Assert.AreEqual(request.Username, decoded.Username);
        }

        [Test]
        public void TestPartialStreamingDecoding() {
            var firstToken = "asldkfjalksdjfalkjsdf";
            var username = "andersonlucasg3";
            var secondToken = "asdlkfalksjdgklashdioohweg";
            var ip = "10.0.0.1";
            var port = 6109;


            var loginRequest = new LoginRequest();
            loginRequest.AccessToken = firstToken;
            loginRequest.Username = username;

            var matchRequest = new MatchRequest();

            var connectRequest = new ConnectGameInstanceResponse();
            connectRequest.Token = secondToken;
            connectRequest.Ip = ip;
            connectRequest.Port = port;

            var encoder = new MessageEncoder();
            List<byte> data = new List<byte>();
            data.AddRange(encoder.Encode(loginRequest));
            data.AddRange(encoder.Encode(matchRequest));
            data.AddRange(encoder.Encode(connectRequest));

            var decoder = new MessageDecoder();

            this.Measure(() => {
                var position = 0;
                do {
                    decoder.Add(data.GetRange(position, 1).ToArray());
                    var container = decoder.Decode();
                    if (container != null) {
                        if (container.Is(LoginRequest.Descriptor)) {
                            var message = container.Parse<LoginRequest>();
                            Assert.AreEqual(message.AccessToken, firstToken);
                            Assert.AreEqual(message.Username, username);
                        } else if (container.Is(MatchRequest.Descriptor)) {
                            var message = container.Parse<MatchRequest>();
                            Assert.AreNotEqual(message, null);
                        } else if (container.Is(ConnectGameInstanceResponse.Descriptor)) {
                            var message = container.Parse<ConnectGameInstanceResponse>();
                            Assert.AreEqual(message.Ip, ip);
                            Assert.AreEqual(message.Port, port);
                            Assert.AreEqual(message.Token, secondToken);
                        }
                    }
                    position += 1;
                } while (position < data.Count);
            }, "Partial Stream Decoding");
        }

        [Test]
        public void TestMessageSize() {
            var encoder = new MessageEncoder();

            LoginRequest request = new LoginRequest {
                AccessToken = "asdfasdfasdf",
                Username = "andersonlucasg3"
            };

            int size = encoder.Encode(request).Length;
            Logger.Log($"LoginRequest Message size: {size}");
        }
    }
}

#endif