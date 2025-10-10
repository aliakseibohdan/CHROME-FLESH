namespace Core.Services
{
    /// <summary>
    /// Contract for dependency injection container managing service registration and resolution
    /// </summary>
    public interface IServiceContainer
    {
        /// <summary>
        /// Registers a service instance with a specific type
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="serviceInstance">Concrete service implementation</param>
        public void Register<T>(T serviceInstance) where T : class;

        /// <summary>
        /// Resolves and returns a service instance of the specified type
        /// </summary>
        /// <typeparam name="T">Service interface type to resolve</typeparam>
        /// <returns>Registered service instance</returns>
        public T Resolve<T>() where T : class;

        /// <summary>
        /// Checks if a service of the specified type is registered
        /// </summary>
        /// <typeparam name="T">Service interface type to check</typeparam>
        /// <returns>True if service is registered</returns>
        public bool IsRegistered<T>() where T : class;
    }
}
