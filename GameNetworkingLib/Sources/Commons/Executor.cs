using System;
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
        readonly MessageContainer message;
        readonly TMessage unpackedMessage;

        public Executor(TModel model, MessageContainer message) {
            ThreadChecker.AssertMainThread(false);
            this.forwarding = new TExecutor();
            this.model = model;
            this.message = message;
            this.unpackedMessage = message.Parse<TMessage>();
        }

        public void Execute() {
            ThreadChecker.AssertMainThread();

            this.forwarding.Execute(this.model, this.unpackedMessage);

            if (this.unpackedMessage is IDisposable disposable) {
                disposable.Dispose();
            }
            this.message.ReturnBuffer();
        }
    }
}