namespace GameNetworking.Executors.Client
{
    using Messages.Client;
    using Models.Contract.Client;

    internal struct PingRequestExecutor<PlayerType> : IExecutor where PlayerType : INetworkPlayer, new()
    {
        private GameClient<PlayerType> client;

        public PingRequestExecutor(GameClient<PlayerType> client)
        {
            this.client = client;
        }

        public void Execute()
        {
            this.client.Send(new PongRequestMessage());
        }
    }
}