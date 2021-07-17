using System;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsSample.Aggregates;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.CommandHandlers
{
    public class UnboxingArticle : Command
    {
        public UnboxingArticle(Guid aggregateId, Guid userId)
        {
            AggregateId = aggregateId;
            UserId = userId;
        }
    }

    public class UnboxingArticleHandler : CommandHandler<UnboxingArticle>
    {
        private readonly ILogger<UnboxingArticle> _logger;
        public UnboxingArticleHandler(ICommandQueue queue, IEventRepository eventRepository, IEventQueue eventQueue,
            ILogger<UnboxingArticle> logger, SnapshotRepository snapshotRepository) : base(queue, eventRepository, eventQueue, snapshotRepository)
        {
            _logger = logger;
            _logger.LogInformation("Init event handler");
        }

        public override void Handle(UnboxingArticle command)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle UnboxingArticle");

            var aggregate = EventRepository.Unbox<ArticleAggregate>(command.AggregateId);
            
            var newAggregate = new ArticleAggregate() { Id = aggregate.Id };

            var state = (ArticleState)aggregate.State;

            _logger.LogInformation($"========= Title = {state.Title} - {state.Summary}");

            newAggregate.CreateArticle(state.Title, state.Summary, state.Details, command.UserId, command.Id);

            Commit(newAggregate);

            _logger.LogInformation("=========Fire event to UnboxingArticle event handler");
        }
    }
}
