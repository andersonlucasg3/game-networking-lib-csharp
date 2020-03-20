using GameNetworking.Executors;

namespace GameNetworking.Commons {
    public abstract class BaseExecutor<TGame> : IExecutor where TGame : class {
        protected TGame instance { get; }

        protected BaseExecutor(TGame instance) {
            this.instance = instance;
        }

        public abstract void Execute();
    }
}