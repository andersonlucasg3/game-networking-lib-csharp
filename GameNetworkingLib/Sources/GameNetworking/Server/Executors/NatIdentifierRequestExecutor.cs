using GameNetworking.Channels;
using GameNetworking.Messages.Client;
using GameNetworking.Messages.Server;
using GameNetworking.Server;
using GameNetworking.Sockets;

namespace GameNetworking.Executors.Server {
    class NatIdentifierRequestExecutor<TPlayer> : Commons.BaseExecutor<IGameServer<TPlayer>, NatIdentifierRequestMessage>
        where TPlayer : class, GameNetworking.Server.IPlayer {
        private readonly TPlayer player;

        public NatIdentifierRequestExecutor(IGameServer<TPlayer> instance, TPlayer player, NatIdentifierRequestMessage message) : base(instance, message) {
            this.player = player;
        }

        public override void Execute() {
            var channel = this.player.GetChannel<UnreliableChannel>(Channel.unreliable);
            var ep = new NetEndPoint(this.message.remoteIp, this.message.port);
            channel.SetRemote(ep);

            this.instance.networkServer.NatIdentify(channel, ep);
            this.instance.listener.GameServerPlayerDidConnect(this.player, Channel.unreliable);

            this.player.Send(new NatIdentifierResponseMessage(), Channel.reliable);
        }
    }
}