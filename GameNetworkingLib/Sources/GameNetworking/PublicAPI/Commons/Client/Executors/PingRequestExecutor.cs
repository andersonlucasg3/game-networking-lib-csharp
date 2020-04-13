using System;
using GameNetworking.Commons;
using GameNetworking.Commons.Client;
using GameNetworking.Commons.Models;
using GameNetworking.Commons.Models.Client;
using GameNetworking.Messages.Client;
using Networking.Commons.Models;
using Networking.Commons.Sockets;

namespace GameNetworking.Executors.Client {
    internal class PingRequestExecutor<TPlayer, TSocket, TClient, TNetClient> : BaseExecutor<IGameClient<TPlayer, TSocket, TClient, TNetClient>> 
        where TPlayer : class, INetworkPlayer<TSocket, TClient, TNetClient>, new()
        where TSocket : ISocket
        where TClient : INetworkClient<TSocket, TNetClient>
        where TNetClient : INetClient<TSocket, TNetClient> {

        public PingRequestExecutor(IGameClient<TPlayer, TSocket, TClient, TNetClient> client) : base(client) { }

        public override void Execute() {
            if (this.instance.localPlayer != null) {
                this.instance.localPlayer.lastReceivedPingRequest = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
            }

            this.instance.Send(new PongRequestMessage());
        }
    }
}