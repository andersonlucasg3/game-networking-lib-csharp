using System;

namespace Commons {
    public abstract class WeakListener<ListenerType> where ListenerType : class {
        private WeakReference weakListener;

        public ListenerType listener {
            get { return this.weakListener?.Target as ListenerType; }
            set { this.weakListener = new WeakReference(value); }
        }
    }
}