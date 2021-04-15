using System;
using GameNetworking.Messages.Models;

namespace GameNetworking.Commons
{
    public interface IExecutor<in TModel, in TMessage>
        where TMessage : struct, ITypedMessage
    {
        void Execute(TModel model, TMessage message);
    }

    public struct Executor<TExecutor, TModel, TMessage>
        where TExecutor : struct, IExecutor<TModel, TMessage>
        where TMessage : struct, ITypedMessage
    {
        private TExecutor forwarding;
        private readonly TModel model;
        private readonly MessageContainer message;
        private readonly TMessage unpackedMessage;

        public Executor(TModel model, MessageContainer message)
        {
            ThreadChecker.AssertMainThread(false);
            forwarding = new TExecutor();
            this.model = model;
            this.message = message;
            unpackedMessage = message.Parse<TMessage>();
        }

        public void Execute()
        {
            ThreadChecker.AssertMainThread();

            forwarding.Execute(model, unpackedMessage);

            IDisposable disposable = unpackedMessage as IDisposable;
            disposable?.Dispose();
            message.Dispose();
        }
    }
}
