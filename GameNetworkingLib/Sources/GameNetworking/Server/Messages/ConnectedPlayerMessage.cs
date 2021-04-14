using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server
{
    public struct ConnectedPlayerMessage : ITypedMessage
    {
        int ITypedMessage.type => (int) MessageType.connectedPlayer;

        public int playerId { get; set; }
        public bool isMe { get; set; }

        void IDecodable.Decode(IDecoder decoder)
        {
            playerId = decoder.GetInt();
            isMe = decoder.GetBool();
        }

        void IEncodable.Encode(IEncoder encoder)
        {
            encoder.Encode(playerId);
            encoder.Encode(isMe);
        }
    }
}