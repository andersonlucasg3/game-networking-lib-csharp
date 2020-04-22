using GameNetworking.Client;
using GameNetworking.Commons;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    internal class ConnectedPlayerExecutor : BaseExecutor<IRemoteClientListener, ConnectedPlayerMessage> {
        internal ConnectedPlayerExecutor(IRemoteClientListener client, ConnectedPlayerMessage message) : base(client, message) { }

        public override void Execute() => this.instance.RemoteClientDidConnect(this.message.playerId, this.message.isMe);
    }
}