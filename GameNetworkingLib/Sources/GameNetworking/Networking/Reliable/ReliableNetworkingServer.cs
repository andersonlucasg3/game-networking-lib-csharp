using Networking.Models;
using Messages.Streams;
using Networking.Sockets;
using GameNetworking.Networking.Commons;
using Networking.Reliable;

namespace GameNetworking.Networking {
    using NetworkClient = Models.NetworkClient<ITCPSocket, ReliableNetClient>;

    public class ReliableNetworkingServer : NetworkingServer<IReliableSocket, ITCPSocket, NetworkClient, ReliableNetClient> {
        public ReliableNetworkingServer(ReliableSocket backend) : base(backend) { }

        public void Disconnect(NetworkClient client) {
            this.networking.Disconnect(client.client);
        }

        public override void Stop() {
            for (int i = 0; i < this.clientsStorage.Count; i++) {
                this.Disconnect(this.clientsStorage[i]);
            }
            base.Stop();
        }

        public override void Update() {
            this.AcceptClient();
            base.Update();
        }

        #region Private Methods

        private void AcceptClient() {
            ReliableNetClient client = this.networking?.Accept();
            if (client != null) {
                client.listener = this;
                NetworkClient networkClient = new NetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
                clientsStorage.Add(networkClient);
                this.listener?.NetworkingServerDidAcceptClient(networkClient);
            }
        }

        #endregion
    }
}
