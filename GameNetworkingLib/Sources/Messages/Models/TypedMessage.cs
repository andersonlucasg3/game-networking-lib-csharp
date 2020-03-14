namespace Messages.Models {
    using Coders;

    public interface ITypedMessage : ICodable {
        int type { get; }
    }
}