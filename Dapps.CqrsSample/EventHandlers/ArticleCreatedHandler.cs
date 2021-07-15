using System;
using Dapps.CqrsCore.Event;

namespace Dapps.CqrsSample.EventHandlers
{
    public class ArticleCreated : Event
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string Details;

        public ArticleCreated(Guid aggregateId, string title, string summary, string details)
        {
            AggregateId = aggregateId;
            Title = title;
            Summary = summary;
            Details = details;
        }
    }

    public class ArticleCreatedHandler : IEventHandler<ArticleCreated>
    {
        public void Handle(ArticleCreated @event)
        {
            
        }
    }
}
