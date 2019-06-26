namespace Messages.Coders {
    public interface IDecoder {
        int Int();
        short Short();
        long Long();
        uint UInt();
        ushort UShort();
        ulong ULong();

        float Float();
        double Double();

        string String();
        byte[] Bytes();

        bool Bool();

        T Object<T>() where T : class, IDecodable, new();
    }
}