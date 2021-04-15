using System;
using System.Collections.Generic;

namespace GameNetworking.Commons
{
    public class PooledList<TItem> : List<TItem>, IDisposable
    {
        private static readonly ObjectPool<PooledList<TItem>> _objectPool = new ObjectPool<PooledList<TItem>>(() => new PooledList<TItem>());

        public static PooledList<TItem> Rent() => _objectPool.Rent();

        public static PooledList<TItem> Rent(ICollection<TItem> copy)
        {
            PooledList<TItem> rent = _objectPool.Rent();
            rent.AddRange(copy);
            return rent;
        }
        
        public void Dispose()
        {
            Clear();
            _objectPool.Pay(this);
        }
    }
}
