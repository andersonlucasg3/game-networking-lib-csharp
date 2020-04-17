using GameNetworking.Commons;
using GameNetworking.Messages.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;

namespace GameNetworking.Executors.Server {
    public class UnreliableConnectExecutor : BaseExecutor<UnreliableNetworkingServer> {
        private readonly UnreliableNetworkClient client;

        public UnreliableConnectExecutor(UnreliableNetworkingServer instance, UnreliableNetworkClient client) : base(instance) {
            this.client = client;
        }

        public override void Execute() {
            if (this.client.isConnected) { return; }

            var connect = new UnreliableConnectResponseMessage();
            this.instance.Send(connect, this.client);
            this.instance.Send(connect, this.client);

            this.client.isConnected = true;

            this.instance.listener.NetworkingServerDidAcceptClient(this.client);
        }
    }
}