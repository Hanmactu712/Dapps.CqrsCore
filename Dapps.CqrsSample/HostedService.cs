using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Command;
using Dapps.CqrsSample.CommandHandlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample
{
    public class HostedService : IHostedService
    {
        private readonly ICqrsCommandDispatcher _queue;
        private readonly ILogger<HostedService> _logger;

        public HostedService(IServiceProvider provider, ILogger<HostedService> logger, ICqrsCommandDispatcher queue)
        {
            _logger = logger;
            _queue = queue;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var random = new Random();
            //test with sending a chains of commands

            var testBoxingId = Guid.Empty;

            for (int i = 0; i < 1; i++)
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
                await _queue.SendAsync(command);
                _logger.LogInformation($"Create command {command.Title} is sent");

                var updateTimes = random.Next(5, 100);

                if (i == 0)
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
                    await _queue.SendAsync(updateCommand);
                    _logger.LogInformation($"Update command {command.Title} is sent");
                }
            }

            //test boxing aggregate
            var boxingCommand = new BoxingArticle(testBoxingId, Guid.NewGuid());
            await _queue.SendAsync(boxingCommand);
            //test unboxing aggregate

            var unBoxingCommand = new UnboxingArticle(testBoxingId, Guid.NewGuid());
            await _queue.SendAsync(unBoxingCommand);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}
