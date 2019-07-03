using System;
using System.Collections.Generic;

namespace Commons {
    public abstract class WeakDelegates<DelegateType> where DelegateType : class {
        private List<WeakReference> weakDelegates = new List<WeakReference>();

        public void Add(DelegateType instance) {
            this.weakDelegates.Add(new WeakReference(instance));
        }

        public void Remove(DelegateType instance) {
            this.weakDelegates.RemoveAll(each => {
                return each.Target == null || each.Target == instance;
            });
        }

        public void ForEach(Action<DelegateType> action) {
            this.weakDelegates.ConvertAll(each => each.Target as DelegateType).ForEach(each => {
                if (each != null) { action.Invoke(each); }
            });
        }
    }
}