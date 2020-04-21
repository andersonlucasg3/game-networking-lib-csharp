using System;
using System.Net;

namespace GameNetworking.Sockets {
    public class NetEndPoint : IEquatable<NetEndPoint> {
        public string host { get; private set; }
        public int port { get; private set; }

        public NetEndPoint() : this(IPAddress.Any.ToString(), 0) { }

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
            return base.Equals(obj);
        }

        public bool Equals(NetEndPoint other) {
            if (other == null) { return false; }
            return this.host == other.host && this.port == other.port;
        }

        public override int GetHashCode() {
#if !UNITY_64
            return host.GetHashCode(StringComparison.InvariantCulture) + port.GetHashCode();
#else
            return host.GetHashCode() + port.GetHashCode();
#endif
        }

        public static bool operator ==(NetEndPoint left, NetEndPoint right) {
            return left.Equals(right);
        }

        public static bool operator !=(NetEndPoint left, NetEndPoint right) {
            return !(left == right);
        }

        public override string ToString() {
            return $"{{ ip: {this.host}, port: {this.port} }}";
        }
    }
}
