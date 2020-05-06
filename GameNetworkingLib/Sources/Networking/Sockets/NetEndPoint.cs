using System;
using System.Net;

namespace GameNetworking.Networking.Sockets {
    public struct NetEndPoint : IEquatable<NetEndPoint> {
        public string host { get; private set; }
        public int port { get; private set; }

        public NetEndPoint(string host, int port) {
            this.host = host;
            this.port = port;
        }

        public void From(IPEndPoint endPoint) {
            this.host = endPoint.Address.ToString();
            this.port = endPoint.Port;
        }

        public void From(EndPoint endPoint) => this.From(endPoint as IPEndPoint);

        public override bool Equals(object obj) {
            if (obj is NetEndPoint other) {
                return this.Equals(other);
            }
            return object.Equals(this, obj);
        }

        public bool Equals(NetEndPoint other) {
            return this.host == other.host && this.port == other.port;
        }

        public override int GetHashCode() {
#if !UNITY_64
            return HashCode.Combine(host, port);
#else
            return host.GetHashCode() + port.GetHashCode();
#endif
        }

        public override string ToString() {
            return $"{{ ip: {this.host}, port: {this.port} }}";
        }
    }
}
