using System;
using System.Linq;
using System.Reflection;
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
using Newtonsoft.Json;

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
            IConfiguration configuration, Action<DbContextOptionsBuilder> dbOptions)
        {
            builder.Services.AddDbContext<EventSourcingDbContext>(option => dbOptions?.Invoke(option));

            builder.Services.AddScoped<ICommandDbContext, EventSourcingDbContext>();
            builder.Services.AddScoped<IEventDbContext, EventSourcingDbContext>();
            builder.Services.AddScoped<ISnapshotDbContext, EventSourcingDbContext>();

            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultSerializer(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddSingleton(typeof(ISerializer), typeof(Serializer));
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultCommandStore(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddSingleton(typeof(ICommandStore), typeof(CommandStore));
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultCommandQueue(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddSingleton(typeof(ICommandQueue), typeof(CommandQueue));
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultEventStore(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddSingleton(typeof(IEventStore), typeof(EventStore));
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultEventRepository(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddSingleton(typeof(IEventRepository), typeof(EventRepository));
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultEventQueue(this ICqrsServiceBuilder builder)
        {
            builder.Services.AddSingleton(typeof(IEventQueue), typeof(EventQueue));
            return builder;
        }

        ///// <summary>
        ///// Add cqrs services with default configuration
        ///// </summary>
        ///// <param name="services"></param>
        ///// <param name="configuration"></param>
        ///// <returns></returns>
        //public static ICqrsServiceBuilder AddCqrsService(this IServiceCollection services, IConfiguration configuration)
        //{
        //    var builder = services.AddCqrsServiceBuilder();

        //    var options = new CqrsServiceOptions();
        //    configuration.GetSection(CqrsServiceOptions.Name).Bind(options);

        //    var commandOption = new CommandStoreOptions() { SaveAll = options.SaveAll };

        //    var snapshotOption = new SnapshotOptions()
        //    {
        //        Interval = options.Snapshot.Interval,
        //        LocalStorage = options.Snapshot.LocalStorage
        //    };

        //    Console.WriteLine($"====== CqrsServiceOptions: {JsonConvert.SerializeObject(options)}");

        //    builder.Services.AddSingleton(commandOption);
        //    builder.Services.AddSingleton(snapshotOption);

        //    builder.AddDefaultSerializer()
        //        .AddDefaultEventSourcingDb(configuration)
        //        .AddDefaultCommandStore()
        //        .AddDefaultCommandQueue()
        //        .AddDefaultEventStore()
        //        .AddDefaultEventRepository()
        //        .AddDefaultEventQueue();

        //    return builder;
        //}

        /// <summary>
        /// Add default configuration for snapshot feature
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddSnapshotFeature(this ICqrsServiceBuilder builder,
            Action<SnapshotOptions> configure = null)
        {
            var option = new SnapshotOptions();
            configure?.Invoke(option);

            builder.Services.Replace(new ServiceDescriptor(typeof(SnapshotOptions), option));

            builder.Services.AddSingleton(typeof(ISnapshotStrategy), typeof(SnapshotStrategy));
            builder.Services.AddSingleton(typeof(ISnapshotStore), typeof(SnapshotStore));

            builder.Services.AddSingleton<SnapshotRepository>();

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
            Action<CqrsServiceOptions> cqrsServiceOptions = null)
        {
            if (cqrsServiceOptions != null)
                services.Configure(cqrsServiceOptions);

            var builder = services.AddCqrsServiceBuilder();

            var options = new CqrsServiceOptions();

            if (cqrsServiceOptions != null)
            {
                services.AddSingleton(options);
                cqrsServiceOptions.Invoke(options);
            }
            else
            {
                configuration.GetSection(CqrsServiceOptions.Name).Bind(options);
            }

            var commandOption = new CommandStoreOptions() { SaveAll = options.SaveAll };

            var snapshotOption = new SnapshotOptions()
            {
                Interval = options.Snapshot.Interval,
                LocalStorage = options.Snapshot.LocalStorage
            };

            Console.WriteLine($"====== CqrsServiceOptions: {options.SaveAll} - Interval: {snapshotOption.Interval} - LocalStorage {snapshotOption.LocalStorage}");

            builder.Services.AddSingleton(commandOption);
            builder.Services.AddSingleton(snapshotOption);

            builder.AddDefaultSerializer()
                .AddDefaultEventSourcingDb(configuration, options.DbContextOption)
                .AddDefaultCommandStore()
                .AddDefaultCommandQueue()
                .AddDefaultEventStore()
                .AddDefaultEventRepository()
                .AddDefaultEventQueue();

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
            var descriptor = new ServiceDescriptor(typeof(ISerializer), typeof(TSerializer), ServiceLifetime.Singleton);
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
                ServiceLifetime.Singleton));
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
                ServiceLifetime.Singleton));
            return builder;
        }

        /// <summary>
        /// Register all ICommandHandler && IEventHandler in the calling assembly
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddHandlers(this ICqrsServiceBuilder builder)
        {
            builder.AddCommandHandlers();
            builder.AddEventHandlers();

            return builder;
        }

        /// <summary>
        /// Register all ICommandHandler in the calling assembly
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddCommandHandlers(this ICqrsServiceBuilder builder)
        {
            builder.RegisterHandler(typeof(ICommand), typeof(ICommandHandler<>), typeof(CommandHandler<>));

            return builder;
        }

        /// <summary>
        /// Register all IEventHandler in the calling assembly
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddEventHandlers(this ICqrsServiceBuilder builder)
        {
            builder.RegisterHandler(typeof(IEvent), typeof(IEventHandler<>), typeof(Event.EventHandler<>));

            return builder;
        }

        private static ICqrsServiceBuilder RegisterHandler(this ICqrsServiceBuilder builder, Type argumentType, Type interfaceType, Type implementationType)
        {
            var callerAssembly = Assembly.GetEntryAssembly();

            var commands = AssemblyUtils.GetTypesDerivedFromType(callerAssembly, argumentType);

            foreach (var commandType in commands)
            {
                var handlerType = interfaceType;
                var implementedType = implementationType;
                Type[] arg = { commandType };
                var genericHandlerType = handlerType.MakeGenericType(arg);
                var genericImplementedHandlerType = implementedType.MakeGenericType(arg);

                var commandHandlerType = AssemblyUtils.GetTypesDerivedFromType(callerAssembly, genericImplementedHandlerType).ToList();

                if (commandType != null && commandHandlerType.Count > 0)
                {
                    builder.Services.AddSingleton(genericHandlerType, commandHandlerType[0]);
                }
            }
            return builder;
        }
    }
}
