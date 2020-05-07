using GameNetworking.Channels;
using GameNetworking.Client;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    class NatIdentifierResponseExecutor<TPlayer> : GameNetworking.Commons.BaseExecutor<GameClient<TPlayer>, NatIdentifierResponseMessage>
        where TPlayer : GameNetworking.Client.Player, new() {
        public NatIdentifierResponseExecutor(GameClient<TPlayer> instance, NatIdentifierResponseMessage message) : base(instance, message) { }

        public override void Execute() => this.instance.listener.GameClientDidConnect(Channel.unreliable);
    }
}