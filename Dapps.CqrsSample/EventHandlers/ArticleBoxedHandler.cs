using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Read;
using Dapps.CqrsSample.Data;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.EventHandlers;

public class ArticleBoxed : CqrsEvent
{
    public ArticleBoxed(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }
}

public class ArticleBoxedHandler : ICqrsEventHandler<ArticleBoxed>
{
    private readonly ILogger<ArticleBoxedHandler> _logger;
    private readonly IEfRepository<Article, ApplicationDbContext> _repository;

    public ArticleBoxedHandler(ICqrsEventDispatcher queue, ILogger<ArticleBoxedHandler> logger, IEfRepository<Article, ApplicationDbContext> repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Handle(ArticleBoxed message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"================Handle event {typeof(ArticleBoxed)} - {message.AggregateId}");

        var item = await _repository.GetByIdAsync(message.AggregateId, cancellationToken);

        if (item != null) { 
            await _repository.DeleteAsync(item);
        }

        _logger.LogInformation($"================Handle event {typeof(ArticleBoxed)} - {message.AggregateId} is handled");

        await Task.CompletedTask;
    }
}
