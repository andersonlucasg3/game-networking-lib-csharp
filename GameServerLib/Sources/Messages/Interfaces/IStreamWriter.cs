namespace Messages.Streams {
    using Coders;

    public interface IStreamWriter {
        byte[] Write<Message>(Message message) where Message : IEncodable;
    }
}