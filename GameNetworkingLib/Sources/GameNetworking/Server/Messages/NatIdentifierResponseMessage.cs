using System.Net;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server
{
    internal struct NatIdentifierResponseMessage : ITypedMessage
    {
        int ITypedMessage.type => (int) MessageType.natIdentifier;

        public string remoteIp { get; private set; }
        public int port { get; private set; }

        public NatIdentifierResponseMessage(IPAddress remoteIp, int port)
        {
            this.remoteIp = remoteIp.ToString();
            this.port = port;
        }

        void IDecodable.Decode(IDecoder decoder)
        {
            remoteIp = decoder.GetString();
            port = decoder.GetInt();
        }

        void IEncodable.Encode(IEncoder encoder)
        {
            encoder.Encode(remoteIp);
            encoder.Encode(port);
        }
    }
}