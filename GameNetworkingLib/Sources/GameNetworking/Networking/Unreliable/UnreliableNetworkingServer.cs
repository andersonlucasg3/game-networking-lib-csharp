using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Messages.Streams;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public class UnreliableNetworkingServer : NetworkingServer<UnreliableSocket, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, UnreliableSocket.IListener {
        public UnreliableNetworkingServer(UnreliableSocket backend) : base(backend) {
            backend.listener = this;
        }

        public override void Update() {
            this.networking.Read();
            for (int index = 0; index < this.clientsList.Count; index++) {
                this.Flush(this.clientsList[index]);
            }
            this.RemoveDisconnected();
        }

        #region Private methods

        private UnreliableNetworkClient AcceptOrRetrieveClient(UnreliableNetClient client) {
            if (this.clientsCollection.TryGetValue(client, out UnreliableNetworkClient value)) {
                return value;
            }
            UnreliableNetworkClient n_client = new UnreliableNetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
            this.clientsCollection.Add(client, n_client);
            this.clientsList.Add(n_client);
            this.listener?.NetworkingServerDidAcceptClient(n_client);
            return n_client;
        }

        #endregion

        #region UnreliableSocket.IListener

        void UnreliableSocket.IListener.SocketDidRead(byte[] bytes, UnreliableNetClient client) {
            var n_client = this.AcceptOrRetrieveClient(client);
            this.TryReadMessage(bytes, n_client);
        }

        #endregion
    }
}