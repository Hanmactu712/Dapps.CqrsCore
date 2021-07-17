using System;
using System.Runtime.InteropServices;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsSample.Aggregates;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.CommandHandlers
{
    public class CreateArticle : Command
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string Details;

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
        public CreateArticleHandler(ICommandQueue queue, IEventRepository eventRepository, IEventQueue eventQueue,
            ILogger<CreateArticle> logger, SnapshotRepository snapshotRepository) : base(queue, eventRepository, eventQueue, snapshotRepository)
        {
            _logger = logger;
            _logger.LogInformation("Init event handler");
        }

        public override void Handle(CreateArticle command)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle command message");

            var aggregate = new ArticleAggregate() { Id = command.AggregateId != Guid.Empty ? command.AggregateId : Guid.NewGuid() };

            aggregate.CreateArticle(command.Title, command.Summary, command.Details, command.UserId, command.Id);

            _logger.LogInformation("=========Fire event to event handler");

            Commit(aggregate);
        }
    }
}
