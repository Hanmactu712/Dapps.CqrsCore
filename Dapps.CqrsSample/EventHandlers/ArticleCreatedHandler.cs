using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Read;
using Dapps.CqrsSample.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.EventHandlers;

public class ArticleCreated : Event
{
    public readonly string Title;
    public readonly string Summary;
    public readonly string Details;

    public ArticleCreated(Guid aggregateId, string title, string summary, string details, Guid commandId)
    {
        AggregateId = aggregateId;
        Title = title;
        Summary = summary;
        Details = details;
        ReferenceId = commandId;
    }
}

public class ArticleCreatedHandler : ICqrsEventHandler<ArticleCreated>
{
    private readonly ILogger<ArticleCreatedHandler> _logger;
    private readonly IEfRepository<Article, ApplicationDbContext> _repository;

    public ArticleCreatedHandler(ICqrsEventDispatcher queue, ILogger<ArticleCreatedHandler> logger, IServiceProvider service)
    {
        _logger = logger;
        _repository = service.CreateScope().ServiceProvider.GetRequiredService<IEfRepository<Article, ApplicationDbContext>>();

        _logger.LogInformation($"Register event {typeof(ArticleCreated)}");
    }

    public async Task Handle(ArticleCreated message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"================Handle event {typeof(ArticleCreated)} - {message.Title}");

        var article = new Article()
        {
            Id = message.AggregateId,
            Title = message.Title,
            Summary = message.Summary,
            Details = message.Details,
        };

        await _repository.AddAsync(article, cancellationToken);

        _logger.LogInformation($"================Handle event {typeof(ArticleCreated)} - {message.Title} is handled");

        await Task.CompletedTask;
    }
}
