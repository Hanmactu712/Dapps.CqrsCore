using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsSample.Aggregates;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Dapps.CqrsSample.CommandHandlers
{
    public class CreateArticle : CqrsCommand
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string Details;
        public Guid UserId { get; set; }

        public CreateArticle(string title, string summary, string details, Guid userId)
        {
            Title = title;
            Summary = summary;
            Details = details;
            UserId = userId;
        }
    }

    public class CreateArticleHandler : CommandHandler<CreateArticle>
    {
        private readonly ILogger<CreateArticle> _logger;
        public CreateArticleHandler(ICqrsCommandQueue queue, ICqrsEventRepository eventRepository, ICqrsEventQueue eventQueue,
            ILogger<CreateArticle> logger, SnapshotRepository snapshotRepository) : base(queue, eventRepository, eventQueue, snapshotRepository)
        {
            _logger = logger;
            _logger.LogInformation("Init event handler");
        }
        public override async Task Handle(CreateArticle command, CancellationToken cancellationToken)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle command message");
            var aggregate = new ArticleAggregate() { Id = command.AggregateId != Guid.Empty ? command.AggregateId : Guid.NewGuid() };
            aggregate.CreateArticle(command.Title, command.Summary, command.Details, command.Id);
            _logger.LogInformation("=========Fire event to event handler");

            await CommitAsync(aggregate);
        }
    }
}
