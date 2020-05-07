using GameNetworking.Messages.Models;

namespace GameNetworking.Commons {
    public interface IExecutor<TModel, TMessage>
        where TMessage : struct, ITypedMessage {
        void Execute(TModel model, TMessage message);
    }

    public struct Executor<TExecutor, TModel, TMessage>
        where TExecutor : IExecutor<TModel, TMessage>
        where TMessage : struct, ITypedMessage {
        readonly TExecutor forwarding;
        readonly TModel model;
        readonly TMessage message;

        public Executor(TExecutor executor, TModel model, TMessage message) {
            this.forwarding = executor;
            this.model = model;
            this.message = message;
        }

        public void Execute() {
            this.forwarding.Execute(this.model, this.message);
        }
    }
}