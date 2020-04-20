using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Client {
    public class UnreliableDisconnectMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.disconnect;

        void IDecodable.Decode(IDecoder decoder) { }
        void IEncodable.Encode(IEncoder encoder) { }
    }
}