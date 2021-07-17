using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapps.CqrsCore.Event
{
    /// <summary>
    /// Queue of event message
    /// </summary>
    public class EventQueue : IEventQueue
    {
        private readonly Dictionary<string, List<Action<IEvent>>> _subscribers;

        public EventQueue()
        {
            _subscribers = new Dictionary<string, List<Action<IEvent>>>();
        }

        /// <summary>
        /// publish a event to queue
        /// </summary>
        /// <param name="ev"></param>
        public void Publish(IEvent ev)
        {
            var name = ev.GetType().AssemblyQualifiedName;
            if (name == null) return;
            if (_subscribers.ContainsKey(name))
            {
                var actions = _subscribers[name];
                foreach (var action in actions)
                {
                    action.Invoke(ev);
                }
            }
        }

        /// <summary>
        /// Subscribe a handler to queue for processing incoming events
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void Subscribe<T>(Action<T> action) where T : IEvent
        {
            var name = typeof(T).AssemblyQualifiedName;

            if (name == null) return;
            if (!_subscribers.Any(s => s.Key.Equals(name)))
            {
                _subscribers.Add(name, new List<Action<IEvent>>());
            }

            _subscribers[name].Add((ev) => action((T)ev));
        }
    }
}
