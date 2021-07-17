using System;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsSample.Aggregates;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.CommandHandlers
{
    public class UpdateArticle : Command
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string Details;

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
        public UpdateArticleHandler(ICommandQueue queue, IEventRepository eventRepository, IEventQueue eventQueue,
            ILogger<UpdateArticle> logger, SnapshotRepository snapshotRepository) : base(queue, eventRepository, eventQueue, snapshotRepository)
        {
            _logger = logger;
            _logger.LogInformation("Init event handler");
        }

        public override void Handle(UpdateArticle command)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle command message");

            var aggregate = Get<ArticleAggregate>(command.AggregateId);

            if (aggregate != null)
            {
                aggregate.UpdateArticle(command.AggregateId, command.Title, command.Summary, command.Details,
                    command.UserId, command.Id);

                _logger.LogInformation("=========Fire event to event handler");

                Commit(aggregate);
            }

            _logger.LogInformation("=========Article not existed");
        }
    }
}
