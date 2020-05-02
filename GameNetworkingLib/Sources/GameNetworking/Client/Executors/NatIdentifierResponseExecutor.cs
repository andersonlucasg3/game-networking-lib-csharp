using GameNetworking.Channels;
using GameNetworking.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Sockets;
using Logging;

namespace GameNetworking.Executors.Client {
    class NatIdentifierResponseExecutor<TPlayer> : GameNetworking.Commons.BaseExecutor<GameClient<TPlayer>, NatIdentifierResponseMessage>
        where TPlayer : GameNetworking.Client.Player, new() {
        private readonly NetEndPoint from;

        public NatIdentifierResponseExecutor(GameClient<TPlayer> instance, NetEndPoint from, NatIdentifierResponseMessage message) : base(instance, message) {
            this.from = from;
        }

        public override void Execute() {
            if (Logger.IsLoggingEnabled) {
                Logger.Log($"Received information from server {this.message.remoteIp}:{this.message.port}");
                Logger.Log($"But really received from server at {this.from}");
            }
            //this.instance.networkClient.ReconnectUnreliable(new NetEndPoint(this.message.))
            this.instance.listener.GameClientDidConnect(Channel.unreliable);
        }
    }
}