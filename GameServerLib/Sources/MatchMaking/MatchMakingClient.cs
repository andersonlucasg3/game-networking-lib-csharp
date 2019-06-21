using System;
using System.Threading.Tasks;

namespace MatchMaking {
    using Connection;
    using Models;

    public class MatchMakingClient<MMClient> where MMClient: Client, new() {
        private ClientConnection<MMClient> connection;

        private WeakReference weakDelegate;

        public bool IsConnected { get { return this.connection.IsConnected; } }

        public IMatchMakingClientDelegate<MMClient> Delegate {
            get { return this.weakDelegate.Target as IMatchMakingClientDelegate<MMClient>; }
            set { this.weakDelegate = new WeakReference(value); }
        }

        public MatchMakingClient() {
            this.connection = new ClientConnection<MMClient>(new Networking.Networking());
        }

        public void Start(string host, int port) {
            this.connection.Connect(host, port);

            this.Delegate?.MatchMakingClientDidConnect(this);
        }

        public void Stop() {
            this.connection.Disconnect();
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
            var container = this.connection.Read();
            if (container != null) { this.RouteMessage(container); }
        }

        public void Flush() {
            this.connection.Flush();
        }

        private void RouteMessage(MessageContainer container) {
            if (container.Is(ConnectGameInstanceResponse.Descriptor)) {
                this.Delegate?.MatchMakingClientDidRequestConnectToGameServer(this, container.Parse<ConnectGameInstanceResponse>());
            }
        }
    }
}
