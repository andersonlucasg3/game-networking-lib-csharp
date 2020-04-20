#if !UNITY_64

using GameNetworking.Sockets;
using NUnit.Framework;
using Test.Core.Model;
using Tests.Core.Model;

namespace Tests.Core {
    class SocketMockTests {
        [SetUp] public void SetUp() {
            UnreliableSocketMock.Setup();
        }

        private NetEndPoint NewEndPoint(string host) => new NetEndPoint(host, 1);

        private ReliableSocketMock NewReliable() => new ReliableSocketMock();

        private UnreliableSocketMock NewUnreliable() => new UnreliableSocketMock();

        private ReliableSocketMock ListeningReliableServer(NetEndPoint endPoint) {
            ReliableSocketMock server = this.NewReliable();

            server.Bind(endPoint);
            server.Listen(10);

            return server;
        }

        private UnreliableSocketMock BoundUnreliableServer(NetEndPoint endPoint) {
            UnreliableSocketMock server = this.NewUnreliable();

            server.Bind(endPoint);

            return server;
        }

        private void AcceptedReliableClient(ReliableSocketMock server, NetEndPoint connectingEndPoint, out ReliableSocketMock connected, out ReliableSocketMock accepted) {
            connected = this.NewReliable();
            connected.Connect(connectingEndPoint, null);

            ReliableSocketMock acceptedClient = null;
            server.Accept((sock) => acceptedClient = sock as ReliableSocketMock);
            accepted = acceptedClient;
        }

        private void AcceptedUnreliableClient(UnreliableSocketMock server, NetEndPoint remoteEndPoint, NetEndPoint bindEndPoint, out UnreliableSocketMock connected, out UnreliableSocketMock accepted) {
            connected = this.NewUnreliable();
            connected.Bind(bindEndPoint);
            connected.BindToRemote(remoteEndPoint);
            connected.Write(new byte[1] { 0 }, null);

            UnreliableSocketMock acceptedClient = null;
            server.Read((bytes, _, socket) => acceptedClient = socket as UnreliableSocketMock);
            accepted = acceptedClient;
        }

        [Test]
        public void TestReliableSockServer() {
            ReliableSocketMock socket = this.ListeningReliableServer(this.NewEndPoint("127.0.0.1"));

            Assert.IsTrue(socket.isBound);
        }

        [Test]
        public void TestUnreliableSockServer() {
            UnreliableSocketMock socket = this.BoundUnreliableServer(this.NewEndPoint("127.0.0.1"));

            Assert.IsTrue(socket.isBound);
        }

        [Test]
        public void TestReliableSockClient() {
            ReliableSocketMock socket = this.NewReliable();

            socket.Connect(this.NewEndPoint("127.0.0.1"), null);

            Assert.IsTrue(socket.isConnected);
        }

        [Test]
        public void TestUnreliableSockClient() {
            UnreliableSocketMock socket = this.NewUnreliable();

            socket.Bind(this.NewEndPoint("192.168.0.1"));
            socket.BindToRemote(this.NewEndPoint("127.0.0.1"));

            Assert.IsTrue(socket.isCommunicable);
        }

        [Test]
        public void TestReliableServerAcceptClient() {
            ReliableSocketMock server = this.ListeningReliableServer(this.NewEndPoint("127.0.0.1"));

            this.AcceptedReliableClient(server, this.NewEndPoint("127.0.0.1"), out ReliableSocketMock connectedSocket, out ReliableSocketMock acceptedSocket);

            Assert.IsNotNull(acceptedSocket);
            Assert.IsTrue(connectedSocket.isConnected);
        }

        [Test] public void TestUnreliableServerAcceptClient() {
            UnreliableSocketMock server = this.BoundUnreliableServer(this.NewEndPoint("127.0.0.1"));

            this.AcceptedUnreliableClient(server, this.NewEndPoint("127.0.0.1"), this.NewEndPoint("192.168.0.1"), out UnreliableSocketMock connectedSocket, out UnreliableSocketMock acceptedSocket);

            Assert.IsNotNull(acceptedSocket);
            Assert.IsTrue(connectedSocket.isCommunicable);
            Assert.AreEqual(connectedSocket, acceptedSocket);
        }

        [Test]
        public void TestReliableReadWrite() {
            ReliableSocketMock server = this.ListeningReliableServer(this.NewEndPoint("127.0.0.1"));

            this.AcceptedReliableClient(server, this.NewEndPoint("127.0.0.1"), out ReliableSocketMock connectedSocket, out ReliableSocketMock acceptedSocket);

            byte[] buffer = { 1, 2, 3, 4, 5 };
            connectedSocket.Write(buffer, null);

            byte[] readBytes = null;
            acceptedSocket.Read((bytes, count) => readBytes = bytes);

            Assert.AreEqual(readBytes, buffer);
        }

        [Test] public void TestUnreliableReadWrite() {
            UnreliableSocketMock server = this.BoundUnreliableServer(this.NewEndPoint("127.0.0.1"));

            this.AcceptedUnreliableClient(server, this.NewEndPoint("127.0.0.1"), this.NewEndPoint("192.168.0.1"), out UnreliableSocketMock connectedSocket, out UnreliableSocketMock acceptedSocket);

            byte[] buffer = { 1, 2, 3, 4, 5 };
            connectedSocket.Write(buffer, null);

            byte[] readBytes = null;
            acceptedSocket.Read((bytes, _, socket) => readBytes = bytes);

            Assert.AreEqual(buffer, readBytes);
            Assert.AreEqual(connectedSocket, acceptedSocket);
        }

        [Test]
        public void TestReliableReadWriteMulticlient() {
            ReliableSocketMock server = this.ListeningReliableServer(this.NewEndPoint("127.0.0.1"));
            this.AcceptedReliableClient(server, this.NewEndPoint("127.0.0.1"), out ReliableSocketMock connectedSocket1, out ReliableSocketMock acceptedSocket1);
            this.AcceptedReliableClient(server, this.NewEndPoint("127.0.0.1"), out ReliableSocketMock connectedSocket2, out ReliableSocketMock acceptedSocket2);

            byte[] buffer1 = { 10, 9, 8, 7, 6, 5 };
            byte[] buffer2 = { 1, 2, 3, 4, 5 };

            connectedSocket1.Write(buffer1, null);
            connectedSocket2.Write(buffer2, null);

            byte[] readBytes1 = null;
            byte[] readBytes2 = null;
            acceptedSocket1.Read((buffer, _) => readBytes1 = buffer);
            acceptedSocket2.Read((buffer, _) => readBytes2 = buffer);

            Assert.AreEqual(readBytes1, buffer1);
            Assert.AreEqual(readBytes2, buffer2);
            Assert.AreNotEqual(readBytes2, readBytes1);
        }

        [Test] public void TestUnreliableReadWriteMulticlient() {
            UnreliableSocketMock server = this.BoundUnreliableServer(this.NewEndPoint("127.0.0.1"));
            this.AcceptedUnreliableClient(server, this.NewEndPoint("127.0.0.1"), this.NewEndPoint("192.168.0.1"), out UnreliableSocketMock connectedSocket1, out UnreliableSocketMock acceptedSocket1);
            this.AcceptedUnreliableClient(server, this.NewEndPoint("127.0.0.1"), this.NewEndPoint("192.168.0.2"), out UnreliableSocketMock connectedSocket2, out UnreliableSocketMock acceptedSocket2);

            byte[] buffer1 = { 10, 9, 8, 7, 6, 5 };
            byte[] buffer2 = { 1, 2, 3, 4, 5 };

            connectedSocket1.Write(buffer1, null);
            connectedSocket2.Write(buffer2, null);

            byte[] readBytes1 = null;
            byte[] readBytes2 = null;
            acceptedSocket1.Read((buffer, _, socket) => readBytes1 = buffer);
            acceptedSocket2.Read((buffer, _, socket) => readBytes2 = buffer);

            Assert.AreEqual(buffer1, readBytes1);
            Assert.AreEqual(buffer2, readBytes2);
            Assert.AreEqual(connectedSocket1, acceptedSocket1);
            Assert.AreEqual(connectedSocket2, acceptedSocket2);
        }

        [Test]
        public void TestReliableDisconnect() {
            ReliableSocketMock server = this.ListeningReliableServer(this.NewEndPoint("127.0.0.1"));

            this.AcceptedReliableClient(server, this.NewEndPoint("127.0.0.1"), out ReliableSocketMock connectedSocket, out ReliableSocketMock acceptedSocket);

            Assert.IsTrue(connectedSocket.isConnected);
            Assert.IsTrue(acceptedSocket.isConnected);

            connectedSocket.Disconnect(null);

            Assert.IsFalse(connectedSocket.isConnected);
            Assert.IsFalse(acceptedSocket.isConnected);
        }
    }
}

#endif