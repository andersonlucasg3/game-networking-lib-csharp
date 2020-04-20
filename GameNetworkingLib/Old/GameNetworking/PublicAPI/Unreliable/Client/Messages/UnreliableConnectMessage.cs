using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Client {
    public class UnreliableConnectMessage : ITypedMessage {
        int ITypedMessage.type => (int)MessageType.connect;

        void IDecodable.Decode(IDecoder decoder) { }

        void IEncodable.Encode(IEncoder encoder) { }
    }
}