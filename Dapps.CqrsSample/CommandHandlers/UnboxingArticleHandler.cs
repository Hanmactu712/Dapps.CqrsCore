﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsSample.Aggregates;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.CommandHandlers
{
    public class UnboxingArticle : CqrsCommand
    {
        public Guid UserId { get; set; }
        public UnboxingArticle(Guid aggregateId, Guid userId)
        {
            AggregateId = aggregateId;
            UserId = userId;
        }
    }

    public class UnboxingArticleHandler : CommandHandler<UnboxingArticle>
    {
        private readonly ILogger<UnboxingArticle> _logger;
        public UnboxingArticleHandler(ICqrsCommandDispatcher queue, ICqrsEventRepository eventRepository, ICqrsEventDispatcher eventQueue,
            ILogger<UnboxingArticle> logger, SnapshotRepository snapshotRepository) : base(queue, eventRepository, eventQueue, snapshotRepository)
        {
            _logger = logger;
            _logger.LogInformation("Init event handler");
        }

        public override async Task Handle(UnboxingArticle command, CancellationToken cancellationToken)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle UnboxingArticle");

            var aggregate = await EventRepository.UnboxAsync<ArticleAggregate>(command.AggregateId, cancellationToken);
            
            _logger.LogInformation($"========= Title = {aggregate.Id}");

            await CommitAsync(aggregate, cancellationToken);

            _logger.LogInformation("=========Fire event to UnboxingArticle event handler");
        }
    }
}
