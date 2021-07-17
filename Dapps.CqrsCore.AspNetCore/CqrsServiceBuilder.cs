using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dapps.CqrsCore.AspNetCore
{
    /// <summary>
    /// CQRS service builder
    /// </summary>
    public class CqrsServiceBuilder : ICqrsServiceBuilder
    {
        public CqrsServiceBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}
