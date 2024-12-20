using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WayfarerKit.Helpers
{
    public sealed class AsyncOperationHandleGroup<T>
    {
        [NonSerialized] private readonly List<AsyncOperationHandle<T>> _handles;

        public AsyncOperationHandleGroup(int capacity)
        {
            Assert.IsTrue(capacity >= 0);

            _handles = new(capacity);
        }

        public IReadOnlyList<AsyncOperationHandle<T>> Handles => _handles;

        public float Progress => _handles.Count == 0f
            ? 0f
            : _handles.Average(handle => handle.PercentComplete);

        public bool IsDone => _handles.Count == 0 || _handles.All(handle => handle.IsDone);

        public void Clear() => _handles.Clear();
        public void AddHandle(AsyncOperationHandle<T> handle) => _handles.Add(handle);
    }
}