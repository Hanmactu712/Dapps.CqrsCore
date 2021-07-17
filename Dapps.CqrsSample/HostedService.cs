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

            _logger = logger;
            _queue = queue;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            //test with sending a chain of commands
            for (int i = 0; i < 10; i++)
            {
                var commandId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                var command = new CreateArticle($"Test title {i} {DateTime.Now}", $"Test summary {DateTime.Now}",
                        $"Test details {DateTime.Now}", Guid.NewGuid())
                {
                    AggregateId = commandId,
                    UserId = userId
                };

                _logger.LogInformation($"Send create command {command.Title}");
                _queue.Send(command);
                _logger.LogInformation($"Create command {command.Title} is sent");

                var updateTimes = i != 5 ? 10 : 30;

                for (int j = 0; j < updateTimes; j++)
                {
                    var updateCommand = new UpdateArticle($"Update title {i} {DateTime.Now}", $"Update summary {DateTime.Now}",
                        $"Update details {DateTime.Now}", Guid.NewGuid())
                    {
                        AggregateId = commandId,
                        UserId = userId
                    };

                    _logger.LogInformation($"Send update command {command.Title}");
                    _queue.Send(updateCommand);
                    _logger.LogInformation($"Update command {command.Title} is sent");
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}
