using System;
using Networking;

namespace MatchMaking {
    using Connection;
    using Models;
    using Commons;

    public class MatchMakingClient<MMClient>: WeakDelegate<IMatchMakingClientDelegate<MMClient>>, IClientConnectionDelegate<MMClient> where MMClient: Client, new() {
        private ClientConnection<MMClient> connection;

        public bool IsConnecting { get { return this.connection?.IsConnecting ?? false; } }

        public bool IsConnected { get { return this.connection?.IsConnected ?? false; } }

        public void Start(string host, int port) {
            if (!(this.connection?.IsConnecting ?? false)) {
                this.connection = new ClientConnection<MMClient>(new NetSocket());
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
            MessageContainer container = this.connection?.Read();
            if (container != null) { this.RouteMessage(container); }
        }

        public void Flush() {
            this.connection?.Flush();
        }

        private void RouteMessage(MessageContainer container) {
            if (container.Is(ConnectGameInstanceResponse.Descriptor)) {
                this.Delegate?.MatchMakingClientDidRequestConnectToGameServer(this, container.Parse<ConnectGameInstanceResponse>());
            } else {
                this.Delegate?.MatchMakingClientDidReceiveUnknownMessage(this, container);
            }
        }

        #region IClientConnectionDelegate<MMClient>

        void IClientConnectionDelegate<MMClient>.ClientConnectionDidConnect() {
            this.Delegate?.MatchMakingClientDidConnect(this);
        }

        void IClientConnectionDelegate<MMClient>.ClientConnectionDidTimeout() {
            this.connection = null;
            this.Delegate?.MatchMakingClientConnectDidTimeout(this);
        }

        void IClientConnectionDelegate<MMClient>.ClientConnectionDidDisconnect() {
            this.connection = null;
            this.Delegate?.MatchMakingClientDidDisconnect(this);
        }

        #endregion
    }
}
