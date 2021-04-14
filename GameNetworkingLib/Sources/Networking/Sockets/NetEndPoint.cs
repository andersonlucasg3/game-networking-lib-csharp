using System;
using System.Net;

namespace GameNetworking.Networking.Sockets
{
    public readonly struct NetEndPoint : IEquatable<NetEndPoint>
    {
        public IPAddress address { get; }
        public int port { get; }

        public NetEndPoint(IPAddress address, int port)
        {
            this.address = address;
            this.port = port;
        }

        public override bool Equals(object obj)
        {
            if (obj is NetEndPoint other) return Equals(other);
            return Equals(this, obj);
        }

        public bool Equals(NetEndPoint other)
        {
            return address.Equals(other.address) && port == other.port;
        }
        
        public override int GetHashCode()
        {
#if !UNITY_64
            return HashCode.Combine(address, port);
#else
            return this.address.GetHashCode() + this.port.GetHashCode();
#endif
        }

        public override string ToString()
        {
            return $"{{ ip: {address}, port: {port} }}";
        }
    }
}
