using System.Net;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Client
{
    internal struct NatIdentifierRequestMessage : ITypedMessage
    {
        int ITypedMessage.type => (int) MessageType.natIdentifier;

        public int playerId;
        public string remoteIp;
        public int port;

        public NatIdentifierRequestMessage(int playerId, IPAddress remoteIp, int port)
        {
            this.playerId = playerId;
            this.remoteIp = remoteIp.ToString();
            this.port = port;
        }

        void IDecodable.Decode(IDecoder decoder)
        {
            playerId = decoder.GetInt();
            remoteIp = decoder.GetString();
            port = decoder.GetInt();
        }

        void IEncodable.Encode(IEncoder encoder)
        {
            encoder.Encode(playerId);
            encoder.Encode(remoteIp);
            encoder.Encode(port);
        }
    }
}