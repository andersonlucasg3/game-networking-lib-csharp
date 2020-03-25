namespace Networking.Commons {
    using Sockets;
    using Models;

    public interface INetworking<TSocket, TClient> where TSocket : ISocket where TClient : INetClient<TSocket, TClient> {
        int port { get; }

        void Start(string host, int port);
        void Stop();

        void Read(TClient client);
        void Send(TClient client, byte[] message);
        void Flush(TClient client);
    }
}