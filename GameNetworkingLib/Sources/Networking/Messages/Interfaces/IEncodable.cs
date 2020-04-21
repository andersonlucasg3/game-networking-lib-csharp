namespace GameNetworking.Messages.Coders {
    public interface IEncodable {
        void Encode(IEncoder encoder);
    }
}
