using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server {
    class NatIdentifierResponseMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.natIdentifier;

        void IDecodable.Decode(IDecoder decoder) { }
        void IEncodable.Encode(IEncoder encoder) { }
    }
}