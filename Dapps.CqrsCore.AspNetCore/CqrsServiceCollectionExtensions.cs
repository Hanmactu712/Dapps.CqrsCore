using System;
using Dapps.CqrsCore.Persistence.Store;
using Dapps.CqrsCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dapps.CqrsCore.AspNetCore
{
    public static class CqrsServiceCollectionExtensions
    {
        public static ICqrsServiceBuilder AddCqrsServiceBuilder(this IServiceCollection services)
        {
            return new CqrsServiceBuilder(services);
        }
        
        public static ICqrsServiceBuilder AddCqrsService(this IServiceCollection services)
        {
            var builder = services.AddCqrsServiceBuilder();
            //builder.AddRequiredSerializer();
            //builder.AddRequiredSerializer();

            return builder;
        }

        public static ICqrsServiceBuilder AddCqrsService(this IServiceCollection services, Action<CqrsServiceOptions> cqrsServiceOptions)
        {
            services.Configure(cqrsServiceOptions);
            var builder = services.AddCqrsService();

            return builder;
        }

        public static ICqrsServiceBuilder AddSerializer<TSerializer>(this ICqrsServiceBuilder builder) where TSerializer: class, ISerializer
        {
            builder.Services.AddSingleton(typeof(ISerializer), typeof(TSerializer));
            return builder;
        }
        
        public static ICqrsServiceBuilder AddEvenSourcingConfigurationStore(this ICqrsServiceBuilder builder, Action<DbContextOptionsBuilder> dbOptions)
        {
            builder.Services.AddDbContext<EventSourcingDBContext>(dbOptions);
            return builder;
        }
        public static ICqrsServiceBuilder AddEvenPersistenceStore(this ICqrsServiceBuilder builder, Action<DbContextOptionsBuilder> dbOptions)
        {
            builder.Services.AddDbContext<EventSourcingDBContext>(dbOptions);
            return builder;
        }
    }
}
