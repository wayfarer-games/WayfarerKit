using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace WayfarerKit.Helpers
{
    public sealed class AsyncOperationGroup
    {
        [NonSerialized] private readonly List<AsyncOperation> _operations;

        public AsyncOperationGroup(int capacity)
        {
            Assert.IsTrue(capacity >= 0);

            _operations = new(capacity);
        }

        public IReadOnlyList<AsyncOperation> Operations => _operations;

        public float Progress => _operations.Count == 0f
            ? 0f
            : _operations.Average(operation => operation.progress);

        public bool IsDone => _operations.All(operation => operation.isDone);

        public void AddOperation(AsyncOperation operation)
        {
            if (operation == null) return;

            Assert.IsTrue(_operations.Count < _operations.Capacity);

            _operations.Add(operation);
        }

        public static AsyncOperationGroup FromOperations(params AsyncOperation[] operations)
        {
            var group = new AsyncOperationGroup(operations.Length);
            foreach (var operation in operations) group.AddOperation(operation);

            return group;
        }
    }
}