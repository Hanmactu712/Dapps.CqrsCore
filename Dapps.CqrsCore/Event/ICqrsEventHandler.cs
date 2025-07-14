using MediatR;

namespace Dapps.CqrsCore.Event;

/// <summary>
/// Interface of event handler
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface ICqrsEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : ICqrsEvent
{
}
