using GameNetworking.Commons;
using GameNetworking.Networking.Commons;
using GameNetworking.Networking.Models;
using Messages.Models;
using Messages.Streams;
using Networking.Models;
using Networking.Sockets;

namespace GameNetworking.Networking {
    public class UnreliableNetworkingServer : NetworkingServer<UnreliableSocket, IUDPSocket, UnreliableNetworkClient, UnreliableNetClient>, UnreliableSocket.IListener {
        private readonly UnreliableServerMessageRouter router;

        public UnreliableNetworkingServer(UnreliableSocket backend, IMainThreadDispatcher dispatcher) : base(backend) {
            this.router = new UnreliableServerMessageRouter(this, dispatcher);
            backend.listener = this;
        }

        public override void Disconnect(UnreliableNetworkClient client) {
            base.Disconnect(client);

            this.listener?.NetworkingServerClientDidDisconnect(client);
        }

        public override void Update() {
            this.networking.Read();
            for (int index = 0; index < this.clientsList.Count; index++) {
                this.Flush(this.clientsList[index]);
            }
            this.RemoveDisconnected();
        }

        protected override void ProcessMessage(MessageContainer message, UnreliableNetworkClient client) {
            if (!this.router.Route(message, client)) { base.ProcessMessage(message, client); }
        }

        #region Private methods

        /// <summary>
        /// Accepts or retrieve existent client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="networkClient"></param>
        /// <returns>True if a new client was accepted</returns>
        private bool AcceptOrRetrieveClient(UnreliableNetClient client, out UnreliableNetworkClient networkClient) {
            if (this.clientsCollection.TryGetValue(client, out networkClient)) {
                return false;
            }
            networkClient = new UnreliableNetworkClient(client, new MessageStreamReader(), new MessageStreamWriter());
            this.clientsCollection.Add(client, networkClient);
            this.clientsList.Add(networkClient);
            return true;
        }

        #endregion

        #region UnreliableSocket.IListener

        void UnreliableSocket.IListener.SocketDidRead(byte[] bytes, int count, UnreliableNetClient client) {
            /*bool isNew = */
            this.AcceptOrRetrieveClient(client, out UnreliableNetworkClient networkClient);
            this.TryReadMessage(bytes, count, networkClient);
            //if (isNew) { this.listener?.NetworkingServerDidAcceptClient(networkClient); }
        }

        #endregion
    }
}