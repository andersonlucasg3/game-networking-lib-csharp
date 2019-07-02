namespace Messages.Models {
    using Coders;

    public interface ITypedMessage: ICodable {
        int Type { get; }
    }
}