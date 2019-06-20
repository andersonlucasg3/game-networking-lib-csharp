namespace Messages.Coders {
    public interface IDecoder {
        int DecodeInt();
        short DecodeShort();
        long DecodeLong();
        uint DecodeUInt();
        ushort DecodeUShort();
        ulong DecodeULong();

        float DecodeFloat();
        double DecodeDouble();

        string DecodeString();
        byte[] DecodeBytes();

        T Decode<T>() where T : IDecodable, new();
    }
}