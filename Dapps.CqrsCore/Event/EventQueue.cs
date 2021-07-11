using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapps.CqrsCore.Event
{
    public class EventQueue : IEventQueue
    {
        private readonly Dictionary<string, List<Action<IEvent>>> _subscribers;

        public EventQueue()
        {
            _subscribers = new Dictionary<string, List<Action<IEvent>>>();
        }
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
