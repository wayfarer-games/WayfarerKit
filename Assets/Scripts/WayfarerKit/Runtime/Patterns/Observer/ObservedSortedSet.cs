using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Unity.Logging;

namespace WayfarerKit.Patterns.Observer
{
    [Serializable]
    public class ObservedSortedSet<T> : SortedSet<T>
    {
        protected List<Action<T, NotifyCollectionChangedAction>> Subscribers = new();

        public ObservedSortedSet(IComparer<T> comparer) : base(comparer) {}
        public ObservedSortedSet(IEnumerable<T> collection) : base(collection) {}
        public ObservedSortedSet(IEnumerable<T> collection, IComparer<T> comparer) : base(collection, comparer) {}

        public bool HandleExceptions { get; set; } = false;

        public virtual void Subscribe(Action<T, NotifyCollectionChangedAction> callback) => Subscribers.Add(callback);
        public virtual void Unsubscribe(Action<T, NotifyCollectionChangedAction> callback) => Subscribers.Remove(callback);

        public new virtual void Clear()
        {
            var items = new List<T>(this);
            foreach (var item in items)
            {
                base.Remove(item);
                NotifySubscribers(item, NotifyCollectionChangedAction.Remove);
            }
        }

        public virtual void ClearSubscriptions() => Subscribers.Clear();

        public new virtual bool Add(T item)
        {
            var itemAdded = base.Add(item);
            if (itemAdded) NotifySubscribers(item, NotifyCollectionChangedAction.Add);

            return itemAdded;
        }

        public new virtual bool Remove(T item)
        {
            var success = base.Remove(item);
            if (success) NotifySubscribers(item, NotifyCollectionChangedAction.Remove);

            return success;
        }

        protected virtual void NotifySubscribers(T item, NotifyCollectionChangedAction changeType)
        {
            // Make a copy of the subscribers list
            var subscribersCopy = Subscribers.ToList();

            foreach (var subscriber in subscribersCopy)
            {
                try
                {
                    subscriber(item, changeType);
                }
                catch (Exception ex)
                {
                    Log.Error($"{ex.Message}\n{ex.StackTrace}");

                    if (!HandleExceptions) throw;
                }
            }
        }
    }
}