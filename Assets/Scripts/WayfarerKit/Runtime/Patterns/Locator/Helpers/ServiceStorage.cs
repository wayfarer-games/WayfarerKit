using System;
using System.Collections.Generic;
using Unity.Logging;

namespace WayfarerKit.Patterns.Locator.Helpers
{
    public sealed class ServiceStorage
    {
        private readonly Dictionary<Type, object> _services = new();

        public IEnumerable<object> Services => _services.Values;

        ~ServiceStorage() => _services.Clear();

        public bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var @object))
            {
                service = @object as T;

                return true;
            }

            service = null;

            return false;
        }

        public T Get<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var @object)) return @object as T;

            throw new ArgumentException($"ServiceStorage.TryGet: Service of type {type.FullName} is not registered.");
        }

        public ServiceStorage Register<T>(T service)
        {
            var type = typeof(T);

            if (!_services.TryAdd(type, service)) Log.Error($"ServiceStorage.Register: Can't register service of type {type.FullName}, as it is already registered.");

            return this;
        }

        public ServiceStorage Register(Type type, object service)
        {
            if (!type.IsInstanceOfType(service))
            {
                throw new ArgumentException(
                    $"ServiceStorage.Register: Type of service {nameof(service)} doesn't match type of service interface");
            }

            if (!_services.TryAdd(type, service)) Log.Error($"ServiceStorage.Register: Can't register service of type {type.FullName}, as it is already registered.");

            return this;
        }
    }
}