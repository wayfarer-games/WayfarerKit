using System.Collections.Generic;
using System.Linq;
using Unity.Logging;
using Console = System.Console;

namespace WayfarerKit.Patterns.EventBus
{
    // Usage of EventBus:
    //      =================================    
    //      Step 1: Define your event struct
    //      =================================
    //          public struct TestEvent : IBusEvent
    //          {
    //  
    //          }
    //
    //          public struct PlayerEvent : IBusEvent
    //          {
    //              public int Param;
    //          }
    //
    //      ==================================================================
    //      Step 2: Create bindings and register callbacks for them.
    //      Callback may have one or zero parameters as you wish.
    //      Also don't forget to deregister when you don't need them anymore.
    //      ==================================================================
    //          testEventBinding = new(HandleTestEvent);
    //          EventBus<TestEvent>.Register(testEventBinding);
    //
    //          playerEventBinding = new(HandlePlayerEvent);
    //          EventBus<PlayerEvent>.Register(playerEventBinding);
    //
    //          ...
    //
    //          EventBus<TestEvent>.Deregister(testEventBinding);
    //          EventBus<PlayerEvent>.Deregister(playerEventBinding);
    //
    //      =================================================================
    //      Step 3: When business logic requires, raise the events.
    //      =================================================================
    //          EventBus<TestEvent>.Raise(new());
    //          EventBus<PlayerEvent>.Raise(new()
    //          {
    //              Param = 40
    //          });
    public static class EventBus<T> where T : IBusEvent
    {
        private static readonly HashSet<IEventBinding<T>> __bindings = new();

        public static void Register(IEventBinding<T> binding)
        {
            __bindings.Add(binding);
        }
        public static void Deregister(IEventBinding<T> binding) => __bindings.Remove(binding);

        public static void Raise(T @event)
        {
            foreach (var binding in __bindings)
            {
                binding.OnEvent.Invoke(@event);
                binding.OnEventNoArgs.Invoke();
            }
        }

        // This Method would be called with reflection when the bus have to be cleared.
        // This is not code smell, as this is part of automated buses creation and destruction process.
        // Check EventBusLoader.cs to review process of lookup through active project assemblies.
        private static void Clear()
        {
            //Log.Debug($"EventBus.Clear: Clear <color=yellow>EventBus<{typeof(T).Name}></color> bindings...");
            __bindings.Clear();
        }
    }
}