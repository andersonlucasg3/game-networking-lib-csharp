public interface INetworking {
    void Start(int port);
    void Connect(string host, int port);

    Client Accept();
    void Disconnect(Client client);

    byte[] Read(Client client);
    void Send(Client client, byte[] message);
    void Flush(Client client);
}