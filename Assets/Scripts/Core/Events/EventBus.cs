using System;
using System.Collections.Generic;

namespace Core.Events
{
    /// <summary>
    /// Implementation of a lightweight event bus with type-safe event publishing
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
        private readonly object _lockObject = new();

        /// <summary>
        /// Subscribes a callback to be invoked when events of type T are published
        /// </summary>
        public void Subscribe<T>(Action<T> callback) where T : class
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var eventType = typeof(T);
            lock (_lockObject)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<Delegate>();
                }

                _subscribers[eventType].Add(callback);
            }
        }

        /// <summary>
        /// Unsubscribes a callback from events of type T
        /// </summary>
        public void Unsubscribe<T>(Action<T> callback) where T : class
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var eventType = typeof(T);
            lock (_lockObject)
            {
                if (_subscribers.ContainsKey(eventType))
                {
                    _ = _subscribers[eventType].Remove(callback);

                    if (_subscribers[eventType].Count == 0)
                    {
                        _ = _subscribers.Remove(eventType);
                    }
                }
            }
        }

        /// <summary>
        /// Publishes an event to all subscribed callbacks
        /// </summary>
        public void Publish<T>(T eventData) where T : class
        {
            if (eventData == null)
            {
                throw new ArgumentNullException(nameof(eventData));
            }

            var eventType = typeof(T);
            List<Delegate> callbacks;

            lock (_lockObject)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    return;
                }

                // Create a copy to avoid issues if callbacks modify subscriptions during iteration
                callbacks = new List<Delegate>(_subscribers[eventType]);
            }

            // Invoke callbacks outside the lock to prevent deadlocks
            foreach (var callback in callbacks)
            {
                try
                {
                    (callback as Action<T>)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[EventBus] Error invoking callback for event {eventType.Name}: {ex.Message}");
                }
            }
        }
    }
}
