using System.Threading;
using Networking.Commons.Models;
using Networking.Sockets;
using NUnit.Framework;
using Test.Core.Model;
using Tests.Core.Model;

namespace Tests.Core {
    class SocketMockTests {
        private readonly NetEndPoint endPoint = new NetEndPoint("127.0.0.1", 30000);

        private ReliableSocketMock NewReliable() => new ReliableSocketMock();

        private UnreliableSocketMock NewUnreliable() => new UnreliableSocketMock();

        private ReliableSocketMock ListeningReliableServer() {
            ReliableSocketMock server = this.NewReliable();

            server.Bind(this.endPoint);
            server.Listen(10);

            return server;
        }

        private UnreliableSocketMock BoundUnreliableServer() {
            UnreliableSocketMock server = this.NewUnreliable();

            server.Bind(this.endPoint);

            return server;
        }

        private void AcceptedReliableClient(ReliableSocketMock server, out ReliableSocketMock connected, out ReliableSocketMock accepted) {
            connected = this.NewReliable();
            connected.Connect(this.endPoint, null);

            ReliableSocketMock acceptedClient = null;
            server.Accept((sock) => acceptedClient = sock as ReliableSocketMock);
            accepted = acceptedClient;
        }

        private void AcceptedUnreliableClient(UnreliableSocketMock server, out UnreliableSocketMock connected, out UnreliableSocketMock accepted) {
            connected = this.NewUnreliable();
            connected.BindToRemote(this.endPoint);
            connected.Write(new byte[1] { 0 }, null);

            UnreliableSocketMock acceptedClient = null;
            server.Read((bytes, socket) => acceptedClient = socket as UnreliableSocketMock);
            accepted = acceptedClient;
        }

        [Test]
        public void TestReliableSockServer() {
            ReliableSocketMock socket = this.ListeningReliableServer();

            Assert.IsTrue(socket.isBound);
        }

        [Test]
        public void TestUnreliableSockServer() {
            UnreliableSocketMock socket = this.BoundUnreliableServer();

            Assert.IsTrue(socket.isBound);
        }

        [Test]
        public void TestReliableSockClient() {
            ReliableSocketMock socket = this.NewReliable();

            socket.Connect(this.endPoint, null);

            Assert.IsTrue(socket.isConnected);
        }

        [Test]
        public void TestUnreliableSockClient() {
            UnreliableSocketMock socket = this.NewUnreliable();

            socket.BindToRemote(this.endPoint);

            Assert.IsTrue(socket.isCommunicable);
        }

        [Test]
        public void TestReliableServerAcceptClient() {
            ReliableSocketMock server = this.ListeningReliableServer();

            this.AcceptedReliableClient(server, out ReliableSocketMock connectedSocket, out ReliableSocketMock acceptedSocket);

            Assert.IsNotNull(acceptedSocket);
            Assert.IsTrue(connectedSocket.isConnected);
        }

        [Test] public void TestUnreliableServerAcceptClient() {
            UnreliableSocketMock server = this.BoundUnreliableServer();

            this.AcceptedUnreliableClient(server, out UnreliableSocketMock connectedSocket, out UnreliableSocketMock acceptedSocket);

            Assert.IsNotNull(acceptedSocket);
            Assert.IsTrue(connectedSocket.isCommunicable);
            Assert.AreEqual(connectedSocket, acceptedSocket);
        }

        [Test]
        public void TestReliableReadWrite() {
            ReliableSocketMock server = this.ListeningReliableServer();

            this.AcceptedReliableClient(server, out ReliableSocketMock connectedSocket, out ReliableSocketMock acceptedSocket);

            byte[] buffer = { 1, 2, 3, 4, 5 };
            connectedSocket.Write(buffer, null);

            byte[] readBytes = null;
            acceptedSocket.Read((bytes) => readBytes = bytes);

            Assert.AreEqual(readBytes, buffer);
        }

        [Test] public void TestUnreliableReadWrite() {
            UnreliableSocketMock server = this.BoundUnreliableServer();

            this.AcceptedUnreliableClient(server, out UnreliableSocketMock connectedSocket, out UnreliableSocketMock acceptedSocket);

            byte[] buffer = { 1, 2, 3, 4, 5 };
            connectedSocket.Write(buffer, null);

            byte[] readBytes = null;
            acceptedSocket.Read((bytes, _) => readBytes = bytes);

            Assert.AreEqual(buffer, readBytes);
            Assert.AreEqual(connectedSocket, acceptedSocket);
        }

        [Test]
        public void TestReliableReadWriteMulticlient() {
            ReliableSocketMock server = this.ListeningReliableServer();
            this.AcceptedReliableClient(server, out ReliableSocketMock connectedSocket1, out ReliableSocketMock acceptedSocket1);
            this.AcceptedReliableClient(server, out ReliableSocketMock connectedSocket2, out ReliableSocketMock acceptedSocket2);

            byte[] buffer1 = { 10, 9, 8, 7, 6, 5 };
            byte[] buffer2 = { 1, 2, 3, 4, 5 };

            connectedSocket1.Write(buffer1, null);
            connectedSocket2.Write(buffer2, null);

            byte[] readBytes1 = null;
            byte[] readBytes2 = null;
            acceptedSocket1.Read((buffer) => readBytes1 = buffer);
            acceptedSocket2.Read((buffer) => readBytes2 = buffer);

            Assert.AreEqual(readBytes1, buffer1);
            Assert.AreEqual(readBytes2, buffer2);
            Assert.AreNotEqual(readBytes2, readBytes1);
        }

        [Test] public void TestUnreliableReadWriteMulticlient() {
            UnreliableSocketMock server = this.BoundUnreliableServer();
            this.AcceptedUnreliableClient(server, out UnreliableSocketMock connectedSocket1, out UnreliableSocketMock acceptedSocket1);
            this.AcceptedUnreliableClient(server, out UnreliableSocketMock connectedSocket2, out UnreliableSocketMock acceptedSocket2);

            byte[] buffer1 = { 10, 9, 8, 7, 6, 5 };
            byte[] buffer2 = { 1, 2, 3, 4, 5 };

            connectedSocket1.Write(buffer1, null);
            connectedSocket2.Write(buffer2, null);

            byte[] readBytes1 = null;
            byte[] readBytes2 = null;
            acceptedSocket1.Read((buffer, _) => readBytes1 = buffer);
            acceptedSocket2.Read((buffer, _) => readBytes2 = buffer);

            Assert.AreEqual(buffer1, readBytes1);
            Assert.AreEqual(buffer2, readBytes2);
            Assert.AreEqual(connectedSocket1, acceptedSocket1);
            Assert.AreEqual(connectedSocket2, acceptedSocket2);
        }

        [Test]
        public void TestReliableDisconnect() {
            ReliableSocketMock server = this.ListeningReliableServer();

            this.AcceptedReliableClient(server, out ReliableSocketMock connectedSocket, out ReliableSocketMock acceptedSocket);

            Assert.IsTrue(connectedSocket.isConnected);
            Assert.IsTrue(acceptedSocket.isConnected);

            connectedSocket.Disconnect(null);

            Assert.IsFalse(connectedSocket.isConnected);
            Assert.IsFalse(acceptedSocket.isConnected);
        }
    }
}
