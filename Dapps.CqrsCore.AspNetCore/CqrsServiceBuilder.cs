using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsCore.AspNetCore
{
    /// <summary>
    /// CQRS service builder
    /// </summary>
    public class CqrsServiceBuilder : ICqrsServiceBuilder
    {
        public ILogger Logger { get; }
        public CqrsServiceBuilder(IServiceCollection services, ILogger logger)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Logger = logger;
        }

        public IServiceCollection Services { get; }
    }
}
