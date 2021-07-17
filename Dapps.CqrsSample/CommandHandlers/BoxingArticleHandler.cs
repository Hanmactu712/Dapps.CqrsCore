using System;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsSample.Aggregates;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.CommandHandlers
{
    public class BoxingArticle : Command
    {
        public BoxingArticle(Guid aggregateId, Guid userId)
        {
            AggregateId = aggregateId;
            UserId = userId;
        }
    }

    public class BoxingArticleHandler : CommandHandler<BoxingArticle>
    {
        private readonly ILogger<BoxingArticle> _logger;
        public BoxingArticleHandler(ICommandQueue queue, IEventRepository eventRepository, IEventQueue eventQueue,
            ILogger<BoxingArticle> logger, SnapshotRepository snapshotRepository) : base(queue, eventRepository, eventQueue, snapshotRepository)
        {
            _logger = logger;
            _logger.LogInformation("Init event handler");
        }

        public override void Handle(BoxingArticle command)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle BoxingArticle message");

            var aggregate = Get<ArticleAggregate>(command.AggregateId);

            if (aggregate != null)
            {
                EventRepository.Box(aggregate);
            }

            _logger.LogInformation("=========Fire event to BoxingArticle event handler");
        }
    }
}
