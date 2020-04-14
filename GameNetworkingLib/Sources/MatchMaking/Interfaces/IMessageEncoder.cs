#if ENABLE

using Google.Protobuf;

namespace MatchMaking.Coders {
    internal interface IMessageEncoder {
        byte[] Encode<Message>(Message message) where Message : IMessage;
    }
}

#endif