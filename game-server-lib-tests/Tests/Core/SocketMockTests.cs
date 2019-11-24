using Networking.IO;
using Networking.Models;
using NUnit.Framework;
using Tests.Core.Model;

namespace Tests.Core {
    class SocketMockTests {
        private NetEndPoint endPoint = new NetEndPoint("127.0.0.1", 30000);

        private SocketMock New() {
            return new SocketMock {
                noDelay = true,
                blocking = false
            };
        }

        private SocketMock ListeningServer() {
            SocketMock server = this.New();

            server.Bind(this.endPoint);
            server.Listen(10);

            return server;
        }

        private void AcceptedClient(SocketMock server, out SocketMock connected, out SocketMock accepted) {
            connected = this.New();
            connected.Connect(this.endPoint, null);

            SocketMock acceptedClient = null;
            server.Accept((sock) => acceptedClient = sock as SocketMock);
            accepted = acceptedClient;
        }

        [Test]
        public void TestSockServer() {
            SocketMock socket = this.ListeningServer();

            Assert.IsFalse(socket.blocking);
            Assert.IsTrue(socket.noDelay);
            Assert.IsTrue(socket.isBound);
        }

        [Test]
        public void TestSockClient() {
            SocketMock socket = this.New();

            socket.Connect(this.endPoint, null);

            Assert.IsTrue(socket.isConnected);
        }

        [Test]
        public void TestServerAcceptClient() {
            SocketMock server = this.ListeningServer();

            this.AcceptedClient(server, out SocketMock connectedSocket, out SocketMock acceptedSocket);

            Assert.IsNotNull(acceptedSocket);
            Assert.IsTrue(acceptedSocket.noDelay);
            Assert.IsFalse(acceptedSocket.blocking);
            Assert.IsTrue(connectedSocket.isConnected);
        }

        [Test]
        public void TestReadWrite() {
            SocketMock server = this.ListeningServer();

            this.AcceptedClient(server, out SocketMock connectedSocket, out SocketMock acceptedSocket);

            byte[] buffer = { 1, 2, 3, 4, 5 };
            connectedSocket.Write(buffer, null);

            byte[] readBytes = null;
            acceptedSocket.Read((bytes) => readBytes = bytes);

            Assert.AreEqual(readBytes, buffer);
        }

        [Test]
        public void TestReadWriteMulticlient() {
            SocketMock server = this.ListeningServer();
            this.AcceptedClient(server, out SocketMock connectedSocket1, out SocketMock acceptedSocket1);
            this.AcceptedClient(server, out SocketMock connectedSocket2, out SocketMock acceptedSocket2);

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

        [Test]
        public void TestDisconnect() {
            SocketMock server = this.ListeningServer();

            this.AcceptedClient(server, out SocketMock connectedSocket, out SocketMock acceptedSocket);

            Assert.IsTrue(connectedSocket.isConnected);
            Assert.IsTrue(acceptedSocket.isConnected);

            connectedSocket.Disconnect(null);

            Assert.IsFalse(connectedSocket.isConnected);
            Assert.IsFalse(acceptedSocket.isConnected);
        }
    }
}
