namespace Networking {
    using Models;

    public delegate void NetworkingConnectDelegate(Client client);

    public interface INetworking {
        int Port { get; }

        void Start(int port);
        void Stop();
        void Connect(string host, int port, NetworkingConnectDelegate connectDelegate);

        Client Accept();
        void Disconnect(Client client);

        byte[] Read(Client client);
        void Send(Client client, byte[] message);
        void Flush(Client client);
    }
}