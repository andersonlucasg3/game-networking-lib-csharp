namespace GameNetworking.Messages.Coders {
    public interface IDecoder {
        int GetInt();
        short GetShort();
        long GetLong();
        uint GetUInt();
        ushort GetUShort();
        ulong GetULong();

        float GetFloat();
        double GetDouble();

        string GetString();
        byte[] GetBytes();

        bool GetBool();

        TDecodable GetObject<TDecodable>() where TDecodable : class, IDecodable, new();
    }
}