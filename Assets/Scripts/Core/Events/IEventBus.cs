using System;

namespace Core.Events
{
    /// <summary>
    /// Contract for a lightweight event bus supporting publish-subscribe pattern
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Subscribes a callback to be invoked when events of type T are published
        /// </summary>
        /// <typeparam name="T">Type of event to subscribe to</typeparam>
        /// <param name="callback">Action to invoke when event is published</param>
        public void Subscribe<T>(Action<T> callback) where T : class;

        /// <summary>
        /// Unsubscribes a callback from events of type T
        /// </summary>
        /// <typeparam name="T">Type of event to unsubscribe from</typeparam>
        /// <param name="callback">Action to remove from subscribers</param>
        public void Unsubscribe<T>(Action<T> callback) where T : class;

        /// <summary>
        /// Publishes an event to all subscribed callbacks
        /// </summary>
        /// <typeparam name="T">Type of event being published</typeparam>
        /// <param name="eventData">Event data to publish</param>
        public void Publish<T>(T eventData) where T : class;
    }
}
