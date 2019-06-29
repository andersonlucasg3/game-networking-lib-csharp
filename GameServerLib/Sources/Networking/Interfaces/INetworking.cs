namespace Networking {
    using Models;

    public interface INetworking {
        int Port { get; }

        INetworkingDelegate Delegate { get; set; }

        void Start(int port);
        NetClient Accept();
        void Stop();

        void Connect(string host, int port);
        void Disconnect(NetClient client);

        byte[] Read(NetClient client);
        void Send(NetClient client, byte[] message);
        void Flush(NetClient client);
    }
}