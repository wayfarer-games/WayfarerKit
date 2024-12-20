using System;

namespace WayfarerKit.Patterns.Observer
{
    public interface IObservedValue
    {
        int SubscriberCount { get; }

        void Subscribe(Action callback);
        void Unsubscribe(Action callback);

        void NotifyChanged();
        void ClearSubscriptions();
    }

    public interface IObserverValue<T> : IObservedValue
    {
        T Value { get; set; }

        void Subscribe(Action<T> callback);
        void Unsubscribe(Action<T> callback);

        void SetSilently(T value);
    }
    
    public interface IReadOnlyObserverValue<T> : IObservedValue
    {
        T Value { get; }

        void Subscribe(Action<T> callback);
        void Unsubscribe(Action<T> callback);
    }
}