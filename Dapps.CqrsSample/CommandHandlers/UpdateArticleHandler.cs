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
    public class UpdateArticle : CqrsCommand
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string Details;
        public Guid UserId { get; set; }

        public UpdateArticle(string title, string summary, string details, Guid userId)
        {
            Title = title;
            Summary = summary;
            Details = details;
            UserId = userId;
        }
    }

    public class UpdateArticleHandler : CommandHandler<UpdateArticle>
    {
        private readonly ILogger<UpdateArticle> _logger;
        public UpdateArticleHandler(ICqrsCommandDispatcher queue, ICqrsEventRepository eventRepository, ICqrsEventDispatcher eventQueue,
            ILogger<UpdateArticle> logger, SnapshotRepository snapshotRepository) : base(queue, eventRepository, eventQueue, snapshotRepository)
        {
            _logger = logger;
            _logger.LogInformation("Init event handler");
        }

        public override async Task Handle(UpdateArticle command, CancellationToken cancellationToken)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle command message");

            var aggregate = await GetAsync<ArticleAggregate>(command.AggregateId, cancellationToken);

            if (aggregate != null)
            {
                aggregate.UpdateArticle(command.AggregateId, command.Title, command.Summary, command.Details, command.Id);

                _logger.LogInformation("=========Fire event to event handler");

                await CommitAsync(aggregate, cancellationToken);
            }

            _logger.LogInformation("=========Article not existed");
        }
    }
}
