#if ENABLE

using Networking;
using Networking.Sockets;

namespace MatchMaking {
    using Connection;
    using Models;

    public class MatchMakingClient<TClient> : IClientConnectionDelegate<TClient> where TClient : MatchMakingClient, new() {
        private ClientConnection<TClient> connection;

        public bool IsConnecting { get { return this.connection?.IsConnecting ?? false; } }

        public bool IsConnected { get { return this.connection?.IsConnected ?? false; } }

        public IMatchMakingClientDelegate<TClient> listener { get; set; }

        public void Start(string host, int port, ITCPSocket socket) {
            if (!(this.connection?.IsConnecting ?? false)) {
                this.connection = new ClientConnection<TClient>(new ReliableSocket(socket));
                this.connection.Connect(host, port);
            }
        }

        public void Stop() {
            this.connection?.Disconnect();
        }

        public void Login(string accessToken, string username) {
            LoginRequest request = new LoginRequest {
                AccessToken = accessToken,
                Username = username
            };

            this.connection.Send(request);
        }

        public void RequestMatch() {
            MatchRequest request = new MatchRequest();

            this.connection.Send(request);
        }

        public void Ready() {
            ReadyStatusRequest request = new ReadyStatusRequest();

            this.connection.Send(request);
        }

        public void Read() {
            this.connection?.Read();
        }

        public void Flush() {
            this.connection?.Flush();
        }

        private void RouteMessage(MessageContainer container) {
            if (container.Is(ConnectGameInstanceResponse.Descriptor)) {
                this.listener?.MatchMakingClientDidRequestConnectToGameServer(this, container.Parse<ConnectGameInstanceResponse>());
            } else {
                this.listener?.MatchMakingClientDidReceiveUnknownMessage(this, container);
            }
        }

        #region IClientConnectionDelegate<MMClient>

        void IClientConnectionDelegate<TClient>.ClientConnectionDidConnect() {
            this.listener?.MatchMakingClientDidConnect(this);
        }

        void IClientConnectionDelegate<TClient>.ClientConnectionDidTimeout() {
            this.connection = null;
            this.listener?.MatchMakingClientConnectDidTimeout(this);
        }

        void IClientConnectionDelegate<TClient>.ClientConnectionDidDisconnect() {
            this.connection = null;
            this.listener?.MatchMakingClientDidDisconnect(this);
        }

        void IClientConnectionDelegate<TClient>.ClientDidReadMessage(MessageContainer container) {
            if (container != null) { this.RouteMessage(container); }
        }

        #endregion
    }
}

#endif