namespace Networking {
    using Models;

    public interface INetworking {
        int Port { get; }

        INetworkingDelegate Delegate { get; set; }

        void Start(int port);
        Client Accept();
        void Stop();

        void Connect(string host, int port);
        void Disconnect(Client client);

        byte[] Read(Client client);
        void Send(Client client, byte[] message);
        void Flush(Client client);
    }
}