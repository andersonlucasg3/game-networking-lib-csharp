namespace Messages.Streams {
    using Models;

    public interface IStreamWriter {
        byte[] Write<Message>(Message message) where Message : ITypedMessage;
    }
}