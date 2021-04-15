using System;
using System.Collections.Generic;

namespace GameNetworking.Commons
{
    public class ObjectPool<TObject>
    {
        private readonly object _lockToken = new object();
        private readonly List<TObject> _bag;
        private readonly Func<TObject> _factory;

        public ObjectPool(Func<TObject> factory)
        {
            _bag = new List<TObject>();
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Pay(Rent());
        }

        public TObject Rent()
        {
            return TryRemove(out TObject item) ? item : _factory.Invoke();
        }

        public void Pay(TObject item)
        {
            lock (_lockToken) _bag.Add(item);
        }

        private bool TryRemove(out TObject obj)
        {
            lock (_lockToken)
            {
                if (_bag.Count == 0)
                {
                    obj = default;
                    return false;
                }
                obj = _bag[0];
                _bag.RemoveAt(0);
                return true;
            }
        }
    }
}
