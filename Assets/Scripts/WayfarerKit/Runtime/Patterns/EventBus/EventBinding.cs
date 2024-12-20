using System;

namespace WayfarerKit.Patterns.EventBus
{
    public sealed class EventBinding<T> : IEventBinding<T> where T : IBusEvent
    {
        private Action<T> onEvent = _ => {};
        private Action onEventNoArgs = () => {};

        public EventBinding(Action<T> onEvent) => this.onEvent = onEvent;
        public EventBinding(Action onEventNoArgs) => this.onEventNoArgs = onEventNoArgs;

        Action<T> IEventBinding<T>.OnEvent
        {
            get => onEvent;
            set => onEvent = value;
        }

        Action IEventBinding<T>.OnEventNoArgs
        {
            get => onEventNoArgs;
            set => onEventNoArgs = value;
        }

        public void Add(Action<T> @event) => onEvent += @event;
        public void Remove(Action<T> @event) => onEvent -= @event;

        public void Add(Action @event) => onEventNoArgs += @event;
        public void Remove(Action @event) => onEventNoArgs -= @event;
    }
}