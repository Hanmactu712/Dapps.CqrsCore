using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Read;
using Dapps.CqrsCore.Utilities;
using Dapps.CqrsSample.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.EventHandlers;

public class ArticleCreated : Event
{
    public readonly string Title;
    public readonly string Summary;
    public readonly string Details;

    public ArticleCreated(Guid aggregateId, string title, string summary, string details, Guid userId, Guid commandId)
    {
        AggregateId = aggregateId;
        Title = title;
        Summary = summary;
        Details = details;
        UserId = userId;
        ReferenceId = commandId;
    }
}

public class ArticleCreatedHandler : CqrsCore.Event.EventHandler<ArticleCreated>
{
    private readonly ILogger<ArticleCreatedHandler> _logger;
    private readonly IEfRepository<Article, ApplicationDbContext> _repository;

    public ArticleCreatedHandler(IEventQueue queue, ILogger<ArticleCreatedHandler> logger, IServiceProvider service) : base(queue)
    {
        _logger = logger;
        _repository = service.CreateScope().ServiceProvider.GetRequiredService<IEfRepository<Article, ApplicationDbContext>>();

        _logger.LogInformation($"Register event {typeof(ArticleCreated)}");
    }

    public override void Handle(ArticleCreated message)
    {
        _logger.LogInformation($"================Handle event {typeof(ArticleCreated)} - {message.Title}");

        var article = message.MapTo<Article>(new Dictionary<string, string>() { { "AggregateId", "Id" } });
        _repository.Add(article);

        _logger.LogInformation($"================Handle event {typeof(ArticleCreated)} - {message.Title} is handled");
    }

    public override Task HandleAsync(ArticleCreated message)
    {
        throw new NotImplementedException();
    }
}
