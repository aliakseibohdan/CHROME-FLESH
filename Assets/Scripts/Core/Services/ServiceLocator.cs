using System;

namespace Core.Services
{
    /// <summary>
    /// Global access point for resolving services throughout the application
    /// Provides static methods to resolve services without direct container access
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceContainer _container;

        /// <summary>
        /// Initializes the service locator with a container instance
        /// Must be called during application startup before any service resolution
        /// </summary>
        /// <param name="container">Service container implementation</param>
        /// <exception cref="ArgumentNullException">Thrown if container is null</exception>
        public static void Initialize(IServiceContainer container) => _container = container ?? throw new ArgumentNullException(nameof(container));

        /// <summary>
        /// Resolves a service instance of the specified type
        /// </summary>
        /// <typeparam name="T">Service interface type to resolve</typeparam>
        /// <returns>Registered service instance</returns>
        /// <exception cref="InvalidOperationException">Thrown if ServiceLocator is not initialized or service not found</exception>
        public static T Resolve<T>() where T : class
        {
            if (_container == null)
            {
                throw new InvalidOperationException("ServiceLocator has not been initialized. Call Initialize() first.");
            }

            return _container.Resolve<T>();
        }

        /// <summary>
        /// Checks if a service of the specified type is registered
        /// </summary>
        /// <typeparam name="T">Service interface type to check</typeparam>
        /// <returns>True if service is registered, false otherwise</returns>
        public static bool IsRegistered<T>() where T : class => _container?.IsRegistered<T>() ?? false;

        /// <summary>
        /// Provides safe service resolution that returns null instead of throwing exceptions
        /// </summary>
        /// <typeparam name="T">Service interface type to resolve</typeparam>
        /// <returns>Service instance if found and registered, null otherwise</returns>
        public static T TryResolve<T>() where T : class
        {
            if (_container == null || !_container.IsRegistered<T>())
            {
                return null;
            }

            return _container.Resolve<T>();
        }
    }
}
