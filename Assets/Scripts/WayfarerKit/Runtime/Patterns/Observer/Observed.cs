using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Logging;

namespace WayfarerKit.Patterns.Observer
{
    [Serializable]
    public class Observed<T> : IObserverValue<T>, IDisposable, IReadOnlyObserverValue<T>
    {
        private T _storedValue;

        protected List<Action> Actions = new();
        protected List<Action<T>> Subscribers = new();

        public Observed() => _storedValue = default;
        public Observed(T initialValue) => _storedValue = initialValue;

        public bool HandleExceptions { get; set; } = false;

        public virtual void Dispose() => ClearSubscriptions();

        public T Value
        {
            get => _storedValue;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_storedValue, value)) return;

                _storedValue = value;
                NotifyChanged();
            }
        }

        public void SetSilently(T newValue) => _storedValue = newValue;

        public int SubscriberCount => Subscribers.Count;
        
        public void SubscribeAndNotify(Action<T> callback)
        {
            Subscribe(callback);
            callback(_storedValue);
        }
        
        public void Subscribe(Action<T> callback)
        {
            if (!Subscribers.Contains(callback)) Subscribers.Add(callback);
        }

        public void Subscribe(Action callback)
        {
            if (!Actions.Contains(callback)) Actions.Add(callback);
        }

        public void Unsubscribe(Action<T> callback) => Subscribers.Remove(callback);
        public void Unsubscribe(Action callback) => Actions.Remove(callback);

        public virtual void ClearSubscriptions()
        {
            Subscribers.Clear();
            Actions.Clear();
        }

        public virtual void NotifyChanged()
        {
            Notify(Actions, action => action());
            Notify(Subscribers, subscriber => subscriber(_storedValue));
        }

        private void Notify<TT>(List<TT> list, Action<TT> action)
        {
            if (list.Count <= 0) return;

            var copy = list.ToList();
            foreach (var item in copy)
            {
                try
                {
                    action(item);
                }
                catch (Exception ex)
                {
                    Log.Error($"<b>{ex.Message}</b>\n{ex.StackTrace}");

                    if (!HandleExceptions) throw;
                }
            }
        }

        public static implicit operator T(Observed<T> observable) => observable.Value;
    }
}