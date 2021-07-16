using System;
using Dapps.CqrsCore.Event;
using Microsoft.Extensions.Logging;

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

    public class ArticleCreatedHandler : Dapps.CqrsCore.Event.EventHandler<ArticleCreated>
    {
        private readonly ILogger<ArticleCreatedHandler> _logger;
        public ArticleCreatedHandler(IEventQueue queue, ILogger<ArticleCreatedHandler> logger) : base(queue)
        {
            _logger = logger;
            _logger.LogInformation($"Register event {typeof(ArticleCreated)}");
        }
        public override void Handle(ArticleCreated @event)
        {
            _logger.LogInformation($"================Handle event {typeof(ArticleCreated)} - {@event.Title}");
        }
    }
}
