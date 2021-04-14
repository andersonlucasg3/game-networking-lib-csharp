using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Server
{
    internal struct PingResultRequestMessage : ITypedMessage
    {
        int ITypedMessage.type => (int) MessageType.pingResult;

        public int playerId { get; private set; }
        public float pingValue { get; private set; }

        public PingResultRequestMessage(int playerId, float value)
        {
            this.playerId = playerId;
            pingValue = value;
        }

        public void Encode(IEncoder encoder)
        {
            encoder.Encode(playerId);
            encoder.Encode(pingValue);
        }

        public void Decode(IDecoder decoder)
        {
            playerId = decoder.GetInt();
            pingValue = decoder.GetFloat();
        }
    }
}