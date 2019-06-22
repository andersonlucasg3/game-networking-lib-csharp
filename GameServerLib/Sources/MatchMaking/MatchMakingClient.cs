using System;
using System.Threading.Tasks;

namespace MatchMaking {
    using Connection;
    using Models;

    public class MatchMakingClient<MMClient> where MMClient: Client, new() {
        private ClientConnection<MMClient> connection;

        private WeakReference weakDelegate;

        private bool isConnecting = false;

        public bool IsConnected { get { return this.connection?.IsConnected ?? false || this.isConnecting; } }

        public IMatchMakingClientDelegate<MMClient> Delegate {
            get { return this.weakDelegate.Target as IMatchMakingClientDelegate<MMClient>; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public void Start(string host, int port) {
            this.isConnecting = true;
            this.connection = new ClientConnection<MMClient>(new Networking.Networking());
            this.connection.Connect(host, port, () => {
                this.Delegate?.MatchMakingClientDidConnect(this);
                this.isConnecting = false;
            });
        }

        public void Stop() {
            this.isConnecting = false;
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
            var container = this.connection?.Read();
            if (container != null) { this.RouteMessage(container); }
        }

        public void Flush() {
            if (!this.isConnecting &&
                !this.IsConnected &&
                this.connection != null) {

                this.connection = null;
                this.Delegate?.MatchMakingClientDidDisconnect(this);
            }
            this.connection?.Flush();
        }

        private void RouteMessage(MessageContainer container) {
            if (container.Is(ConnectGameInstanceResponse.Descriptor)) {
                this.Delegate?.MatchMakingClientDidRequestConnectToGameServer(this, container.Parse<ConnectGameInstanceResponse>());
            } else {
                this.Delegate?.MatchMakingClientDidReceiveUnknownMessage(this, container);
            }
        }
    }
}
