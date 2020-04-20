namespace GameNetworking.Messages.Streams {
    using Models;

    public interface IStreamReader {
        void Add(byte[] buffer, int count);

        MessageContainer Decode();
    }
}