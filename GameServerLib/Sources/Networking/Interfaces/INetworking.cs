namespace Networking {
    using Models;

    public interface INetworking {
        int Port { get; }

        void Start(int port);
        Client Connect(string host, int port);

        Client Accept();
        void Disconnect(Client client);

        byte[] Read(Client client);
        void Send(Client client, byte[] message);
        void Flush(Client client);
    }
}