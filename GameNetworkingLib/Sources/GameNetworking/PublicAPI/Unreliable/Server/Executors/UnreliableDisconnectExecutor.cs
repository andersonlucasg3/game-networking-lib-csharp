using GameNetworking.Commons;
using GameNetworking.Messages.Server;
using GameNetworking.Networking;
using GameNetworking.Networking.Models;

namespace GameNetworking.Executors.Server {
    public class UnreliableDisconnectExecutor : BaseExecutor<UnreliableNetworkingServer> {
        private readonly UnreliableNetworkClient client;

        public UnreliableDisconnectExecutor(UnreliableNetworkingServer instance, UnreliableNetworkClient client) : base(instance) {
            this.client = client;
        }

        public override void Execute() {
            if (this.client == null) { return; }

            var disconnect = new UnreliableDisconnectResponseMessage();
            this.instance.Send(disconnect, client);
            this.instance.Send(disconnect, client);

            this.instance.Disconnect(this.client);
        }
    }
}