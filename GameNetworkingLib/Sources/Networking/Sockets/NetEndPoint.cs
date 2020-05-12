using System;
using System.Net;

namespace GameNetworking.Networking.Sockets {
    public struct NetEndPoint : IEquatable<NetEndPoint> {
        public IPAddress address { get; private set; }
        public int port { get; private set; }

        public NetEndPoint(IPAddress address, int port) {
            this.address = address;
            this.port = port;
        }

        public override bool Equals(object obj) {
            if (obj is NetEndPoint other) {
                return this.Equals(other);
            }
            return object.Equals(this, obj);
        }

        public bool Equals(NetEndPoint other) {
            return this.address.Equals(other.address) && this.port == other.port;
        }

        public override int GetHashCode() {
#if !UNITY_64
            return HashCode.Combine(this.address, this.port);
#else
            return this.address.GetHashCode() + this.port.GetHashCode();
#endif
        }

        public override string ToString() {
            return $"{{ ip: {this.address}, port: {this.port} }}";
        }
    }
}
