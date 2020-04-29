using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Client {
    class NatIdentifierRequestMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.natIdentifier;

        public string remote = "";
        public int port = 0;

        void IDecodable.Decode(IDecoder decoder) {
            this.remote = decoder.GetString();
            this.port = decoder.GetInt();
        }

        void IEncodable.Encode(IEncoder encoder) {
            encoder.Encode(this.remote);
            encoder.Encode(this.port);
        }
    }
}