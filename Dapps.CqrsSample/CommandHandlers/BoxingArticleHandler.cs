using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsSample.Aggregates;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.CommandHandlers
{
    public class BoxingArticle : CqrsCommand
    {
        public Guid UserId { get; set; }

        public BoxingArticle(Guid aggregateId, Guid userId)
        {
            AggregateId = aggregateId;
            UserId = userId;
        }
    }

    public class BoxingArticleHandler : CommandHandler<BoxingArticle>
    {
        private readonly ILogger<BoxingArticle> _logger;
        private readonly ICqrsCommandStore _commandStore;
        public BoxingArticleHandler(ICqrsCommandDispatcher queue, ICqrsEventRepository eventRepository, ICqrsEventDispatcher eventQueue,
            ILogger<BoxingArticle> logger, SnapshotRepository snapshotRepository, ICqrsCommandStore commandStore) : base(queue, eventRepository, eventQueue, snapshotRepository)
        {
            _logger = logger;
            _commandStore = commandStore;
            _logger.LogInformation("Init event handler");
        }

        public override async Task Handle(BoxingArticle command, CancellationToken cancellationToken)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle BoxingArticle message");

            try
            {
                var aggregate = Get<ArticleAggregate>(command.AggregateId);

                if (aggregate == null) return;

                await _commandStore.BoxAsync(aggregate.Id, cancellationToken);

                _logger.LogInformation("=========Boxing command Ok!");

                await EventRepository.BoxAsync(aggregate, cancellationToken);

                _logger.LogInformation("=========BoxingArticle Ok!");
            }
            catch (Exception exception)
            {
                _logger.LogError($"=========Error {exception}");
            }
        }
    }
}
