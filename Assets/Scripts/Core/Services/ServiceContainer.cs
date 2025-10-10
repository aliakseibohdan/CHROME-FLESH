using System;
using System.Collections.Generic;

namespace Core.Services
{
    /// <summary>
    /// Simple implementation of IServiceContainer for service registration and resolution
    /// </summary>
    public class ServiceContainer : IServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new();

        /// <summary>
        /// Registers a service instance with a specific type
        /// </summary>
        public void Register<T>(T serviceInstance) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                throw new InvalidOperationException($"Service of type {type.Name} is already registered");
            }

            _services[type] = serviceInstance ?? throw new ArgumentNullException(nameof(serviceInstance));
        }

        /// <summary>
        /// Resolves and returns a service instance of the specified type
        /// </summary>
        public T Resolve<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"No service of type {type.Name} is registered");
        }

        /// <summary>
        /// Checks if a service of the specified type is registered
        /// </summary>
        public bool IsRegistered<T>() where T : class => _services.ContainsKey(typeof(T));
    }
}
