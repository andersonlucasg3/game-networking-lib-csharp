using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Client {
    class NatIdentifierRequestMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.natIdentifier;

        public int playerId = 0;
        public string remoteIp = "";
        public int port = 0;

        public NatIdentifierRequestMessage() { }

        public NatIdentifierRequestMessage(int playerId, string remoteIp, int port) {
            this.playerId = playerId;
            this.remoteIp = remoteIp;
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