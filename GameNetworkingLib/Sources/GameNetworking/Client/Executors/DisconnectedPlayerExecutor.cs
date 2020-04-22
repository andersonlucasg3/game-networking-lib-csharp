﻿using GameNetworking.Client;
using GameNetworking.Messages.Server;

namespace GameNetworking.Executors.Client {
    internal class DisconnectedPlayerExecutor : Commons.BaseExecutor<IRemoteClientListener, DisconnectedPlayerMessage> {
        internal DisconnectedPlayerExecutor(IRemoteClientListener client, DisconnectedPlayerMessage message) : base(client, message) { }

        public override void Execute() => this.instance.RemoteClientDidDisconnect(this.message.playerId);
    }
}