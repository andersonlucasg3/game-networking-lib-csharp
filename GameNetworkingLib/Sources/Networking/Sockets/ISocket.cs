namespace GameNetworking.Networking.Sockets
{
    public interface ISocket<TDerived>
        where TDerived : ISocket<TDerived>
    {
        NetEndPoint localEndPoint { get; }
        NetEndPoint remoteEndPoint { get; }

        bool isConnected { get; }

        void Bind(NetEndPoint endPoint);

        void Connect(NetEndPoint endPoint);

        void Close();
    }
}
