using System.Net;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Client {
    struct NatIdentifierRequestMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.natIdentifier;

        public int playerId;
        public string remoteIp;
        public int port;

        public NatIdentifierRequestMessage(int playerId, IPAddress remoteIp, int port) {
            this.playerId = playerId;
            this.remoteIp = remoteIp.ToString();
            this.port = port;
        }

        void IDecodable.Decode(IDecoder decoder) {
            this.playerId = decoder.GetInt();
            this.remoteIp = decoder.GetString();
            this.port = decoder.GetInt();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.playerId);
            encoder.Encode(this.remoteIp);
            encoder.Encode(this.port);
        }
    }
}