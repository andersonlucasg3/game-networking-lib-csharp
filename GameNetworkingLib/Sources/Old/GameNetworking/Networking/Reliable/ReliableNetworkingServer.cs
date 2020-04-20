using GameNetworking.Messages.Streams;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Networking;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public class ReliableNetworkingServer : NetworkingServer<ReliableSocket, ITCPSocket, ReliableNetworkClient, ReliableNetClient> {
        public ReliableNetworkingServer(ReliableSocket backend) : base(backend) { }

        public override void Disconnect(ReliableNetworkClient client) {
            this.networking.Disconnect(client.client);
        }

        public override void Stop() {
            for (int i = 0; i < this.clientsList.Count; i++) {
                this.Disconnect(this.clientsList[i]);
            }
            base.Stop();
        }

        public override void Update() {
            this.AcceptClient();
            base.Update();
        }

        #region Protected methods

        protected override void Flush(ReliableNetworkClient client) {
            if (client.client.isConnected) {
                base.Flush(client);
            } else {
                this.listener?.NetworkingServerClientDidDisconnect(client);
                this.disconnectedClientsToRemove.Enqueue(client);
            }
        }

        #endregion

        #region Private Methods

        private void AcceptClient() {
            ReliableNetClient client = this.networking?.Accept();
            if (client != null) {
                client.listener = this;
                var networkClient = new ReliableNetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
                this.clientsCollection.Add(client, networkClient);
                this.clientsList.Add(networkClient);
                this.listener?.NetworkingServerDidAcceptClient(networkClient);
            }
        }

        #endregion
    }
}
