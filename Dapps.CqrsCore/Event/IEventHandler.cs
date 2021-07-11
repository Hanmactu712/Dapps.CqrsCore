using System.Threading.Tasks;

namespace Dapps.CqrsCore.Event
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        void Handle(TEvent @event);
        Task HandleAsync(TEvent @event);
    }
}
