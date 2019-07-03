using System;

namespace Commons {
    public abstract class WeakDelegate<DelegateType> where DelegateType : class {
        private WeakReference weakDelegate;

        public DelegateType Delegate {
            get { return this.weakDelegate?.Target as DelegateType; }
            set { this.weakDelegate = new WeakReference(value); }
        }
    }
}