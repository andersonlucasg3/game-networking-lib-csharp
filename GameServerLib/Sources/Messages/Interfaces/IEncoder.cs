namespace Messages.Coders {
    public interface IEncoder {
        void Encode(int value);
        void Encode(short value);
        void Encode(long value);
        void Encode(uint value);
        void Encode(ushort value);
        void Encode(ulong value);

        void Encode(float value);
        void Encode(double value);

        void Encode(string value);
        void Encode(byte[] value);

        void Encode(bool value);

        void Encode(IEncodable value);
    }
}