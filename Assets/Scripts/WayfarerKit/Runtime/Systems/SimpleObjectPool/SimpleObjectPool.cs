using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace WayfarerKit.Systems.SimpleObjectPool
{
    public class SimpleObjectPool<T> where T : Component, ISimplePooledItem
    {
        // ReSharper disable once StaticMemberInGenericType
        private static int _counter;

        private IObjectPool<T> _pool;
        private GameObject _source;
        private Transform _storageTransform;

        public SimpleObjectPool(GameObject source, Transform parent, int capacity, int maxSize = 100)
        {
            Assert.IsTrue(capacity > 0, "Capacity must be greater than 0");
            Assert.IsTrue(maxSize >= capacity, $"Max size must be greater or equal to capacity, but was {maxSize} and capacity was {capacity}");
            Assert.IsNotNull(source, "source != null");
            Assert.IsTrue(source.GetComponent<T>() != null, $"No component of type {typeof(T)} found on source object");

            _source = source;

            var objectPool = new ObjectPool<T>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject,
                true, capacity, maxSize);

            _pool = objectPool;
        }

        public void Take(T obj) => _pool.Release(obj);
        public T Get() => _pool.Get();
        public int CountInactive() => _pool.CountInactive;

        private static void OnDestroyPoolObject(T obj) => Object.Destroy(obj.gameObject);
        private static void OnTakeFromPool(T obj) => obj.OnTakeFromPool();

        private void OnReturnedToPool(T obj)
        {
            obj.OnReturnedToPool();
            obj.transform.parent = _storageTransform;
            obj.transform.localPosition = Vector3.zero;
        }

        private T CreatePooledItem()
        {
            var go = Object.Instantiate(_source);
            go.name = $"{_source.name} (Pooled {_counter++})";

            var component = go.GetComponent<T>();
            Assert.IsNotNull(component, $"Component of type {typeof(T)} not found on source object {_source.name}");

            component.OnCreated();

            return component;
        }

        ~SimpleObjectPool()
        {
            _pool?.Clear();
            _pool = null;

            _source = null;
            _storageTransform = null;
        }
    }
}