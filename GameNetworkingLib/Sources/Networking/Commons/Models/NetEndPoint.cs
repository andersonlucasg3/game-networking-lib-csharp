using System;

namespace Networking.Commons.Models {
    public struct NetEndPoint : IEquatable<NetEndPoint> {
        public string host { get; }
        public int port { get; }

        public NetEndPoint(string host, int port) {
            this.host = host;
            this.port = port;
        }

        public override bool Equals(object obj) {
            if (obj is NetEndPoint other) {
                return this.Equals(other);
            }
            return base.Equals(obj);
        }

        public bool Equals(NetEndPoint other) {
            return this.host == other.host && this.port == other.port;
        }

        public override int GetHashCode() {
            return host.GetHashCode() + port.GetHashCode();
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
