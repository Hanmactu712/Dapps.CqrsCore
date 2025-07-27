using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Read;
using Dapps.CqrsSample.Data;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.EventHandlers
{
    public class ArticleUpdated : Event
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string Details;

        public ArticleUpdated(Guid aggregateId, string title, string summary, string details, Guid commandId)
        {
            AggregateId = aggregateId;
            Title = title;
            Summary = summary;
            Details = details;
            ReferenceId = commandId;
        }
    }

    public class ArticleUpdatedHandler : ICqrsEventHandler<ArticleUpdated>
    {
        private readonly ILogger<ArticleUpdatedHandler> _logger;
        private readonly IEfRepository<Article, ApplicationDbContext> _repository;

        public ArticleUpdatedHandler(ICqrsEventDispatcher queue, ILogger<ArticleUpdatedHandler> logger, IEfRepository<Article, ApplicationDbContext> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Handle(ArticleUpdated message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"================Handle event {typeof(ArticleUpdated)} - {message.Title}");

            var entity = _repository.GetById(message.AggregateId);

            if (entity == null) return;

            entity.Title = message.Title;
            entity.Summary = message.Summary;
            entity.Details = message.Details;

            await _repository.UpdateAsync(entity, cancellationToken);

            _logger.LogInformation(
                $"================Handle event {typeof(ArticleUpdated)} - {message.Title} is handled");

            await Task.CompletedTask;
        }

    }
}
