using GameNetworking.Executors;

namespace GameNetworking.Executors.Client {
    using Messages.Server;

    internal struct PingResultRequestExecutor: IExecutor {
        private GameClient client;
        private readonly PingResultRequestMessage message;

        public PingResultRequestExecutor(GameClient client, PingResultRequestMessage message) {
            this.client = client;
            this.message = message;
        }

        void IExecutor.Execute() {
            client.MostRecentPingValue = message.pingValue;

            Logging.Logger.Log(this.GetType(), $"Setting MostRecentPingValue {message.pingValue}");
        }
    }
}