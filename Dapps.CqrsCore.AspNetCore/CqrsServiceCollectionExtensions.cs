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
        /// <summary>
        /// Cqrs builder
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static ICqrsServiceBuilder AddCqrsServiceBuilder(this IServiceCollection services)
        {
            return new CqrsServiceBuilder(services);
        }
        
        private static ICqrsServiceBuilder AddDefaultEventSourcingDb(this ICqrsServiceBuilder builder,
            IConfiguration configuration)
        {
            builder.Services.AddDbContext<EventSourcingDbContext>(option =>
                option.UseSqlServer(configuration.GetConnectionString("CqrsConnection")));

            builder.Services.AddScoped<ICommandDbContext, EventSourcingDbContext>();
            builder.Services.AddScoped<IEventDbContext, EventSourcingDbContext>();
            builder.Services.AddScoped<ISnapshotDbContext, EventSourcingDbContext>();

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


        /// <summary>
        /// Add cqrs services with default configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddCqrsService(this IServiceCollection services, IConfiguration configuration)
        {
            var builder = services.AddCqrsServiceBuilder();

            builder.AddDefaultSerializer()
                .AddDefaultEventSourcingDb(configuration)
                .AddDefaultCommandStore()
                .AddDefaultCommandQueue()
                .AddDefaultEventStore()
                .AddDefaultEventRepository()
                .AddDefaultEventQueue();

            return builder;
        }

        /// <summary>
        /// Add default configuration for snapshot feature
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddSnapshotFeature(this ICqrsServiceBuilder builder,
            Action<SnapshotOptions> configure)
        {
            var option = new SnapshotOptions();
            configure(option);

            builder.Services.AddSingleton(option);
            //builder.Services.AddScoped<ISnapshotDbContext, EventSourcingDbContext>();

            builder.Services.AddScoped(typeof(ISnapshotStrategy), typeof(SnapshotStrategy));
            builder.Services.AddScoped(typeof(ISnapshotStore), typeof(SnapshotStore));

            //builder.Services.AddScoped<SnapshotRepository>();

            builder.Services.Replace(new ServiceDescriptor(typeof(IEventRepository), typeof(SnapshotRepository),
                ServiceLifetime.Scoped));

            return builder;
        }

        /// <summary>
        /// Add cqrs services with default configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="cqrsServiceOptions"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddCqrsService(this IServiceCollection services, IConfiguration configuration,
            Action<CqrsServiceOptions> cqrsServiceOptions)
        {
            services.Configure(cqrsServiceOptions);

            var commandOption = new CommandStoreOptions();
            var cqrsOption = new CqrsServiceOptions();
            cqrsServiceOptions(cqrsOption);
            commandOption.SaveAll = cqrsOption.SaveAll;

            services.AddSingleton(commandOption);

            var builder = services.AddCqrsService(configuration);

            return builder;
        }

        /// <summary>
        /// Override default Serializer
        /// </summary>
        /// <typeparam name="TSerializer"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddSerializer<TSerializer>(this ICqrsServiceBuilder builder)
            where TSerializer : class, ISerializer
        {
            var descriptor = new ServiceDescriptor(typeof(ISerializer), typeof(TSerializer), ServiceLifetime.Scoped);
            builder.Services.Replace(descriptor);

            //builder.Services.AddScoped(typeof(ISerializer), typeof(TSerializer));
            return builder;
        }
        
        /// <summary>
        /// Override default configuration for event & aggregate store database
        /// </summary>
        /// <typeparam name="TContext">database context</typeparam>
        /// <param name="builder">Cqrs builder</param>
        /// <param name="dbOptions">Db option</param>
        /// <returns>cqrs builder</returns>
        public static ICqrsServiceBuilder AddEventStoreDb<TContext>(this ICqrsServiceBuilder builder,
            Action<DbContextOptionsBuilder> dbOptions) where TContext : DbContext, IEventDbContext
        {
            builder.Services.AddDbContext<TContext>(dbOptions);
            //builder.Services.AddScoped<ICommandDbContext, TContext>();

            builder.Services.Replace(new ServiceDescriptor(typeof(IEventDbContext), typeof(TContext),
                ServiceLifetime.Scoped));

            return builder;
        }

        /// <summary>
        /// Override default configuration for snapshot store database.
        /// </summary>
        /// <typeparam name="TContext">database context</typeparam>
        /// <param name="builder">Cqrs builder</param>
        /// <param name="dbOptions">Db option</param>
        /// <returns>cqrs builder</returns>
        public static ICqrsServiceBuilder AddSnapshotStoreDb<TContext>(this ICqrsServiceBuilder builder,
            Action<DbContextOptionsBuilder> dbOptions) where TContext : DbContext, ISnapshotDbContext
        {
            builder.Services.AddDbContext<TContext>(dbOptions);
            //builder.Services.AddScoped<ISnapshotDbContext, TContext>();
            builder.Services.Replace(new ServiceDescriptor(typeof(ISnapshotDbContext), typeof(TContext),
                ServiceLifetime.Scoped));
            return builder;
        }

        /// <summary>
        /// Override default configuration for command store database
        /// </summary>
        /// <typeparam name="TContext">database context</typeparam>
        /// <param name="builder">Cqrs builder</param>
        /// <param name="dbOptions">Db option</param>
        /// <returns>cqrs builder</returns>
        public static ICqrsServiceBuilder AddCommandStoreDb<TContext>(this ICqrsServiceBuilder builder,
            Action<DbContextOptionsBuilder> dbOptions) where TContext : DbContext, ICommandDbContext
        {
            builder.Services.AddDbContext<TContext>(dbOptions);
            //builder.Services.AddScoped<ICommandDbContext, TContext>();
            builder.Services.Replace(new ServiceDescriptor(typeof(ICommandDbContext), typeof(TContext),
                ServiceLifetime.Scoped));
            return builder;
        }

        /// <summary>
        /// Override default Event Queue
        /// </summary>
        /// <typeparam name="TQueue"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddEventQueue<TQueue>(this ICqrsServiceBuilder builder)
            where TQueue : IEventQueue
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(IEventQueue), typeof(TQueue),
                ServiceLifetime.Scoped));
            return builder;
        }

        /// <summary>
        /// Override default command queue
        /// </summary>
        /// <typeparam name="TQueue"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddCommandQueue<TQueue>(this ICqrsServiceBuilder builder)
            where TQueue : ICommandQueue
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ICommandQueue), typeof(TQueue),
                ServiceLifetime.Scoped));
            return builder;
        }
    }
}
