using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dapps.CqrsCore.AspNetCore
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection UseCqrsWithEventSourcing(this IServiceCollection services, Action<CqrsServiceOptions> builder)
        {
            var options = new CqrsServiceOptions();
            builder(options);
            
            return services;
        }
    }
}
