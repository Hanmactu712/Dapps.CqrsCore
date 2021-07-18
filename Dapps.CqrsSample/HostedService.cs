using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsSample.CommandHandlers;
using Dapps.CqrsSample.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample
{
    public class HostedService : IHostedService
    {
        private readonly IServiceProvider _provider;
        private ICommandHandler<CreateArticle> _cmdHandler;
        private IEventHandler<ArticleCreated> _eventHandler;
        private readonly ICommandQueue _queue;
        private readonly ILogger<HostedService> _logger;

        public HostedService(IServiceProvider provider, ILogger<HostedService> logger, ICommandQueue queue)
        {
            _provider = provider;
            _cmdHandler = provider.GetRequiredService<ICommandHandler<CreateArticle>>();
            _eventHandler = provider.GetRequiredService<IEventHandler<ArticleCreated>>();

            var cmdHandler = provider.GetRequiredService<ICommandHandler<UpdateArticle>>();
            var eventHandler = provider.GetRequiredService<IEventHandler<ArticleUpdated>>();
            var x = provider.GetRequiredService<ICommandHandler<BoxingArticle>>();
            var y = provider.GetRequiredService<ICommandHandler<UnboxingArticle>>();

            _logger = logger;
            _queue = queue;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var random = new Random();
            //test with sending a chains of commands

            var testBoxingId = Guid.Empty;

            for (int i = 0; i < 10; i++)
            {
                var aggregateId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                var command = new CreateArticle($"Test title {i} {DateTime.Now}", $"Test summary {DateTime.Now}",
                        $"Test details {DateTime.Now}", Guid.NewGuid())
                {
                    AggregateId = aggregateId,
                    UserId = userId
                };

                _logger.LogInformation($"Send create command {command.Title}");
                _queue.Send(command);
                _logger.LogInformation($"Create command {command.Title} is sent");

                var updateTimes = random.Next(5, 100);

                if (i == 5)
                {
                    testBoxingId = aggregateId;
                }

                for (int j = 0; j < updateTimes; j++)
                {
                    var updateCommand = new UpdateArticle($"Update title {i} {DateTime.Now}", $"Update summary {DateTime.Now}",
                        $"Update details {DateTime.Now}", Guid.NewGuid())
                    {
                        AggregateId = aggregateId,
                        UserId = userId
                    };

                    _logger.LogInformation($"Send update command {command.Title}");
                    _queue.Send(updateCommand);
                    _logger.LogInformation($"Update command {command.Title} is sent");
                }
            }

            //test boxing aggregate
            var boxingCommand = new BoxingArticle(testBoxingId, Guid.NewGuid());
            _queue.Send(boxingCommand);
            //test unboxing aggregate

            var unBoxingCommand = new UnboxingArticle(testBoxingId, Guid.NewGuid());
            _queue.Send(unBoxingCommand);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}
