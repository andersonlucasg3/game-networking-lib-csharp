using System;
namespace Networking.Models {
    public struct NetEndPoint {
        public readonly string host;
        public readonly int port;

        public NetEndPoint(string host, int port) {
            this.host = host;
            this.port = port;
        }
    }
}
