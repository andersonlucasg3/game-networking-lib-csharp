using GameNetworking.Messages.Models;

namespace GameNetworking.Commons {
    public interface IExecutor<TModel, TMessage>
        where TMessage : struct, ITypedMessage {
        void Execute(TModel model, TMessage message);
    }

    public struct Executor<TExecutor, TModel, TMessage>
        where TExecutor : struct, IExecutor<TModel, TMessage>
        where TMessage : struct, ITypedMessage {
        readonly TExecutor forwarding;
        readonly TModel model;
        readonly TMessage message;

        public Executor(TModel model, TMessage message) {
            this.forwarding = new TExecutor();
            this.model = model;
            this.message = message;
        }

        public void Execute() {
            this.forwarding.Execute(this.model, this.message);
        }
    }
}