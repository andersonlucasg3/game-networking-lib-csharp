using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server {
    class NatIdentifierResponseMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.natIdentifier;

        public string remoteIp = "";
        public int port = 0;

        public NatIdentifierResponseMessage() { }

        public NatIdentifierResponseMessage(string remoteIp, int port) {
            this.remoteIp = remoteIp;
            this.port = port;
        }

        void IDecodable.Decode(IDecoder decoder) {
            this.remoteIp = decoder.GetString();
            this.port = 0;
        }
        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.remoteIp);
            encoder.Encode(this.port);
        }
    }
}