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

    public class CreateArticleHandler : ICqrsCommandHandler<CreateArticle>
    {
        private readonly ILogger<CreateArticle> _logger;
        private readonly ICqrsEventRepository _repository;

        public CreateArticleHandler(ILogger<CreateArticle> logger, ISnapshotRepository snapshotRepository)
        {
            _logger = logger;
            _logger.LogInformation("Init event handler");
            _repository = snapshotRepository;
        }

        public async Task Handle(CreateArticle command, CancellationToken cancellationToken)
        {
            //Console.WriteLine("Save to database");
            _logger.LogInformation("=========Handle command message");
            var aggregate = new ArticleAggregate() { Id = command.AggregateId != Guid.Empty ? command.AggregateId : Guid.NewGuid() };
            aggregate.CreateArticle(command.Title, command.Summary, command.Details, command.Id);
            _logger.LogInformation("=========Fire event to event handler");

            await _repository.SaveAsync(aggregate, cancellation: cancellationToken);
        }
    }
}
