namespace Networking {
    using Models;

    public interface INetworking {
        int port { get; }

        INetworkingListener listener { get; set; }

        void StartServer(int port);
        INetClient Accept();
        void Stop();

        void Connect(string host, int port);
        void Disconnect(INetClient client);

        void Read(INetClient client);
        void Send(INetClient client, byte[] message);
        void Flush(INetClient client);
    }
}