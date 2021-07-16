using System;
using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Command;
using Dapps.CqrsSample.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dapps.CqrsSample
{
    public class HostedService : IHostedService
    {
        private readonly IServiceProvider _provider;

        public HostedService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var command = new CreateArticle($"Test title {DateTime.Now}", $"Test summary {DateTime.Now}", $"Test details {DateTime.Now}");

            var queue = _provider.CreateScope().ServiceProvider.GetRequiredService<ICommandQueue>();
            queue.Send(command);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}
