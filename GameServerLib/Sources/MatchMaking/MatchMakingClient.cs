using System;
using System.Threading.Tasks;

namespace MatchMaking {
    using Connection;
    using Models;

    public class MatchMakingClient<MMClient>: IClientConnectionDelegate<MMClient> where MMClient: Client, new() {
        private ClientConnection<MMClient> connection;

        private WeakReference weakDelegate;

        public bool IsConnecting { get { return this.connection?.IsConnecting ?? false; } }

        public bool IsConnected { get { return this.connection?.IsConnected ?? false; } }

        public IMatchMakingClientDelegate<MMClient> Delegate {
            get { return this.weakDelegate?.Target as IMatchMakingClientDelegate<MMClient>; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public void Start(string host, int port) {
            this.connection = new ClientConnection<MMClient>(new Networking.Networking());
            this.connection.Connect(host, port);
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

        void IClientConnectionDelegate<MMClient>.ClientConnectionDidDisconnect() {
            this.connection = null;
            this.Delegate?.MatchMakingClientDidDisconnect(this);
        }

        #endregion
    }
}
