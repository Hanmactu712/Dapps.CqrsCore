using System;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence;
using Dapps.CqrsCore.Persistence.Store;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dapps.CqrsCore.AspNetCore
{
    public static class CqrsServiceCollectionExtensions
    {
        public static ICqrsServiceBuilder AddCqrsServiceBuilder(this IServiceCollection services)
        {
            return new CqrsServiceBuilder(services);
        }

        public static ICqrsServiceBuilder AddCqrsService(this IServiceCollection services, IConfiguration configuration)
        {
            var builder = services.AddCqrsServiceBuilder();

            builder.AddDefaultSerializer()
                .AddDefaultEventSourcingConfigurationStore(configuration)
                .AddDefaultCommandStore()
                .AddDefaultCommandQueue()
                .AddDefaultEventStore()
                .AddDefaultEventRepository()
                .AddDefaultEventQueue();

            return builder;
        }

        public static ICqrsServiceBuilder AddCqrsService(this IServiceCollection services, IConfiguration configuration, Action<CqrsServiceOptions> cqrsServiceOptions)
        {
            services.Configure(cqrsServiceOptions);
            var builder = services.AddCqrsService(configuration);

            return builder;
        }

        public static ICqrsServiceBuilder AddSerializer<TSerializer>(this ICqrsServiceBuilder builder) where TSerializer : class, ISerializer
        {
            var descriptor = new ServiceDescriptor(typeof(ISerializer), typeof(TSerializer), ServiceLifetime.Scoped);
            builder.Services.Replace(descriptor);

            //builder.Services.AddScoped(typeof(ISerializer), typeof(TSerializer));
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultEventSourcingConfigurationStore(this ICqrsServiceBuilder builder,
            IConfiguration configuration)
        {
            builder.Services.AddDbContext<EventSourcingDbContext>(option =>
                option.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultSerializer(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddScoped(typeof(ISerializer), typeof(Serializer));
            return builder;
        }
        private static ICqrsServiceBuilder AddDefaultCommandStore(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddScoped(typeof(ICommandStore), typeof(CommandStore));
            return builder;
        }
        private static ICqrsServiceBuilder AddDefaultCommandQueue(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddScoped(typeof(ICommandQueue), typeof(CommandQueue));
            return builder;
        }


        private static ICqrsServiceBuilder AddDefaultEventStore(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddScoped(typeof(IEventStore), typeof(EventStore));
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultEventRepository(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddScoped(typeof(IEventRepository), typeof(EventRepository));
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultEventQueue(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddScoped(typeof(IEventQueue), typeof(EventQueue));
            return builder;
        }
        private static ICqrsServiceBuilder AddSnapshot(this ICqrsServiceBuilder builder, Action<SnapshotOptions> configure)
        {
            var option = new SnapshotOptions();
            configure(option);

            builder.Services.AddSingleton(option);

            builder.Services.AddScoped(typeof(ISnapshotStrategy), typeof(SnapshotStrategy));
            builder.Services.AddScoped(typeof(ISnapshotStore), typeof(SnapshotStore));

            builder.Services.AddScoped<SnapshotRepository>();
            return builder;
        }
        
        public static ICqrsServiceBuilder AddEventSourcingConfigurationStore(this ICqrsServiceBuilder builder, Action<DbContextOptionsBuilder> dbOptions)
        {
            builder.Services.AddDbContext<EventSourcingDbContext>(dbOptions);
            return builder;
        }
        //public static ICqrsServiceBuilder AddEvenPersistenceStore(this ICqrsServiceBuilder builder, Action<DbContextOptionsBuilder> dbOptions)
        //{
        //    builder.Services.AddDbContext<EventSourcingDBContext>(dbOptions);
        //    return builder;
        //}
    }
}
