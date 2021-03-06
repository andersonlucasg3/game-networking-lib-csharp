﻿using System;
using System.Collections.Concurrent;

namespace GameNetworking.Commons {
    public class ObjectPool<T> {
        private readonly ConcurrentBag<T> bag;
        private readonly Func<T> factory;

        public ObjectPool(Func<T> factory) {
            bag = new ConcurrentBag<T>();
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.Pay(this.Rent());
        }

        public T Rent() {
            if (bag.TryTake(out T item)) return item;
            return factory();
        }

        public void Pay(T item) {
            bag.Add(item);
        }
    }
}