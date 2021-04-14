using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams
{
    public interface IStreamReader
    {
        void Add(byte[] buffer, int count);

        MessageContainer? Decode();
    }
}