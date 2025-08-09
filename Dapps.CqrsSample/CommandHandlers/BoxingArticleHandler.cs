using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsSample.Aggregates;
using Dapps.CqrsSample.EventHandlers;
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

    public class BoxingArticleHandler : ICqrsCommandHandler<BoxingArticle>
    {
        private readonly ILogger<BoxingArticle> _logger;
        private readonly ICqrsCommandStore _commandStore;
        private readonly ICqrsEventRepository _repository;
        private readonly ICqrsEventDispatcher _eventDispatcher;

        public BoxingArticleHandler(ILogger<BoxingArticle> logger, ISnapshotRepository snapshotRepository, ICqrsCommandStore commandStore, ICqrsEventDispatcher eventDispatcher)
        {
            _logger = logger;
            _commandStore = commandStore;
            _logger.LogInformation("Init event handler");
            _repository = snapshotRepository;
            _eventDispatcher = eventDispatcher;
        }

        public async Task Handle(BoxingArticle command, CancellationToken cancellationToken)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle BoxingArticle message");

            try
            {
                var aggregate = await _repository.GetAsync<ArticleAggregate>(command.AggregateId);

                if (aggregate == null) return;

                await _commandStore.BoxAsync(aggregate.Id, cancellationToken);

                _logger.LogInformation("=========Boxing command Ok!");

                await _repository.BoxAsync(aggregate, cancellationToken);

                //emit boxed event
                _eventDispatcher.Publish(new ArticleBoxed(aggregate.Id));

                _logger.LogInformation("=========BoxingArticle Ok!");
            }
            catch (Exception exception)
            {
                _logger.LogError($"=========Error {exception}");
            }
        }
    }
}
