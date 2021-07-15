using System;

namespace Dapps.CqrsCore.Event
{
    public interface IEventQueue
    {
        void Publish(IEvent ev);
        void Subscribe<T>(Action<T> action) where T : IEvent;

    }
}
