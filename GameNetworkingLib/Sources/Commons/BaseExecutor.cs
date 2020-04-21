using GameNetworking.Executors;
using GameNetworking.Messages.Models;

namespace GameNetworking.Commons {
    public abstract class BaseExecutor<TGame, TMessage> : IExecutor
        where TGame : class
        where TMessage : ITypedMessage {
        protected TGame instance { get; }
        protected TMessage message { get; }

        protected BaseExecutor(TGame instance, TMessage message) {
            this.instance = instance;
            this.message = message;
        }

        public abstract void Execute();
    }
}