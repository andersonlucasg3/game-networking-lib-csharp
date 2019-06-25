namespace Messages.Streams {
    using Models;

    public interface IStreamReader {
        void Add(byte[] buffer);

        MessageContainer Decode();
    }
}