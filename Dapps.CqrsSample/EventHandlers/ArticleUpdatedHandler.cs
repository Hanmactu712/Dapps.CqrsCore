using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Read;
using Dapps.CqrsCore.Utilities;
using Dapps.CqrsSample.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.EventHandlers
{
    public class ArticleUpdated : Event
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string Details;

        public ArticleUpdated(Guid aggregateId, string title, string summary, string details, Guid userId, Guid commandId)
        {
            AggregateId = aggregateId;
            Title = title;
            Summary = summary;
            Details = details;
            UserId = userId;
            ReferenceId = commandId;
        }
    }

    public class ArticleUpdatedHandler : Dapps.CqrsCore.Event.EventHandler<ArticleUpdated>
    {
        private readonly ILogger<ArticleUpdatedHandler> _logger;
        private readonly IEfRepository<Article, ApplicationDbContext> _repository;

        public ArticleUpdatedHandler(IEventQueue queue, ILogger<ArticleUpdatedHandler> logger, IServiceProvider service) : base(queue)
        {
            _logger = logger;
            _repository = service.CreateScope().ServiceProvider.GetRequiredService<IEfRepository<Article, ApplicationDbContext>>();

            _logger.LogInformation($"Register event {typeof(ArticleUpdated)}");
        }

        public override void Handle(ArticleUpdated message)
        {
            _logger.LogInformation($"================Handle event {typeof(ArticleUpdated)} - {message.Title}");

            var entity = _repository.GetById(message.AggregateId);
            
            if (entity == null) return;

            var article = message.MapTo(entity, new Dictionary<string, string>() {{"AggregateId", "Id"}});
            _repository.Update(article);

            _logger.LogInformation(
                $"================Handle event {typeof(ArticleUpdated)} - {message.Title} is handled");
        }

        public override Task HandleAsync(ArticleUpdated message)
        {
            throw new NotImplementedException();
        }
    }
}
