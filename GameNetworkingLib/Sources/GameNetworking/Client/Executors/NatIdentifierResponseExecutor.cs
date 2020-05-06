using GameNetworking.Channels;
using GameNetworking.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Networking.Sockets;

namespace GameNetworking.Executors.Client {
    class NatIdentifierResponseExecutor<TPlayer> : GameNetworking.Commons.BaseExecutor<GameClient<TPlayer>, NatIdentifierResponseMessage>
        where TPlayer : GameNetworking.Client.Player, new() {
        private readonly NetEndPoint from;

        public NatIdentifierResponseExecutor(GameClient<TPlayer> instance, NetEndPoint from, NatIdentifierResponseMessage message) : base(instance, message) {
            this.from = from;
        }

        public override void Execute() => this.instance.listener.GameClientDidConnect(Channel.unreliable);
    }
}