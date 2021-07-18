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
        private readonly ICommandStore _commandStore;
        public BoxingArticleHandler(ICommandQueue queue, IEventRepository eventRepository, IEventQueue eventQueue,
            ILogger<BoxingArticle> logger, SnapshotRepository snapshotRepository, ICommandStore commandStore) : base(queue, eventRepository, eventQueue, snapshotRepository)
        {
            _logger = logger;
            _commandStore = commandStore;
            _logger.LogInformation("Init event handler");
        }

        public override void Handle(BoxingArticle command)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle BoxingArticle message");

            try
            {
                var aggregate = Get<ArticleAggregate>(command.AggregateId);

                if (aggregate == null) return;
                
                _commandStore.Box(aggregate.Id);

                _logger.LogInformation("=========Boxing command Ok!");

                EventRepository.Box(aggregate);

                _logger.LogInformation("=========BoxingArticle Ok!");
            }
            catch (Exception exception)
            {
                _logger.LogError($"=========Error {exception}");
            }


        }
    }
}
