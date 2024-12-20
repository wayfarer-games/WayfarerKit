using UnityEngine;

namespace WayfarerKit.Systems.SimpleObjectPool
{
    public interface ISimplePooledItem
    {
        public void RegisterPool<T>(SimpleObjectPool<T> pool) where T : Component, ISimplePooledItem;
        public void ReleaseToPool();
        public void OnTakeFromPool();
        public void OnReturnedToPool();
        public void OnCreated();
    }
}