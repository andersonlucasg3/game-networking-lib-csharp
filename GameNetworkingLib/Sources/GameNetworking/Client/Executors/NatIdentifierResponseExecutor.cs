using GameNetworking.Channels;
using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    struct NatIdentifierResponseExecutor<TPlayer> : IExecutor<GameClient<TPlayer>, NatIdentifierResponseMessage>
        where TPlayer : GameNetworking.Client.Player, new() {
        public void Execute(GameClient<TPlayer> model, NatIdentifierResponseMessage message)
            => model.listener.GameClientDidConnect(Channel.unreliable);
    }
}