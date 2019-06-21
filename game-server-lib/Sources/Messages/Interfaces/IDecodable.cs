namespace Messages.Coders {
    public interface IDecodable {
        void Decode(IDecoder decoder);
    }
}