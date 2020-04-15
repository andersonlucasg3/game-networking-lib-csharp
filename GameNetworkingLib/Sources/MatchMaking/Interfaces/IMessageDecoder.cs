#if ENABLE

namespace MatchMaking.Coders {
    using Models;

    internal interface IMessageDecoder {
        void Add(byte[] buffer);
        MessageContainer Decode();
    }
}

#endif