namespace Networking.Commons {
    using Models;
    using Sockets;

    public interface INetworking<TSocket, TClient> where TSocket : ISocket where TClient : INetClient<TSocket, TClient> {
        int port { get; }

        void Start(string host, int port);
        void Stop();

        void Read(TClient client);
        void Send(TClient client, byte[] message);
        void Flush(TClient client);
    }
}