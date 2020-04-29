using GameNetworking.Channels;
using GameNetworking.Client;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    class NatIdentifierResponseExecutor<TPlayer> : GameNetworking.Commons.BaseExecutor<IGameClient<TPlayer>, NatIdentifierResponseMessage>
        where TPlayer : GameNetworking.Client.Player {
        public NatIdentifierResponseExecutor(IGameClient<TPlayer> instance, NatIdentifierResponseMessage message) : base(instance, message) { }

        public override void Execute() {
            this.instance.listener.GameClientDidConnect(Channel.unreliable);
        }
    }
}