using System;
using Unity.Logging;
using UnityEngine;

namespace WayfarerKit.Systems.SimpleObjectPool
{
    public class SimplePooledItem : MonoBehaviour, ISimplePooledItem
    {
        private SimpleObjectPool<SimplePooledItem> _pool;

        public virtual void RegisterPool<T>(SimpleObjectPool<T> pool) where T : Component, ISimplePooledItem
        {
            _pool = pool as SimpleObjectPool<SimplePooledItem>;

            if (_pool == null)
                throw new InvalidCastException("Pool provided is not of the correct type.");
        }

        public virtual void ReleaseToPool()
        {
            Log.Error("ReleaseToPool ");
            _pool?.Take(this);

            throw new InvalidOperationException("This item is not registered with a pool.");
        }
        public virtual void OnTakeFromPool() => Log.Error("OnTakeFromPool ");
        public virtual void OnReturnedToPool() => Log.Error("OnReturnedToPool ");
        public virtual void OnCreated() => Log.Error("OnCreated ");
    }
}