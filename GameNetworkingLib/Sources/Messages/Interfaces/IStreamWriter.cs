namespace Messages.Streams {
    using Models;

    public interface IStreamWriter {
        byte[] Write<TMessage>(TMessage message) where TMessage : ITypedMessage;
    }
}