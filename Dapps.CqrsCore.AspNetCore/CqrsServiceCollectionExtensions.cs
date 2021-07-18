using System;
using System.Linq;
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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dapps.CqrsCore.AspNetCore
{
    public static class CqrsServiceCollectionExtensions
    {
        private static string _messageFormat = "===> CQRS Service ===> {TimeStamp}: \r\n{Content}";
        /// <summary>
        /// Cqrs builder
        /// </summary>
        /// <param name="services"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static ICqrsServiceBuilder AddCqrsServiceBuilder(this IServiceCollection services, ILogger logger)
        {
            return new CqrsServiceBuilder(services, logger);
        }

        private static ICqrsServiceBuilder AddDefaultEventSourcingDb(this ICqrsServiceBuilder builder, Action<DbContextOptionsBuilder> dbOptions)
        {
            builder.Services.AddDbContext<EventSourcingDbContext>(option => dbOptions?.Invoke(option));

            builder.Services.AddScoped<ICommandDbContext, EventSourcingDbContext>();
            builder.Services.AddScoped<IEventDbContext, EventSourcingDbContext>();
            builder.Services.AddScoped<ISnapshotDbContext, EventSourcingDbContext>();

            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultSerializer(this ICqrsServiceBuilder builder)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register Serializer........");
            builder.Services.AddSingleton(typeof(ISerializer), typeof(Serializer));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register Serializer........Done!");
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultCommandStore(this ICqrsServiceBuilder builder)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register CommandStore........");
            builder.Services.AddSingleton(typeof(ICommandStore), typeof(CommandStore));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register CommandStore........Done!");
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultCommandQueue(this ICqrsServiceBuilder builder)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register CommandQueue........");
            builder.Services.AddSingleton(typeof(ICommandQueue), typeof(CommandQueue));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register CommandQueue........Done!");
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultEventStore(this ICqrsServiceBuilder builder)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register EventStore........");
            builder.Services.AddSingleton(typeof(IEventStore), typeof(EventStore));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register EventStore........Done!");
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultEventRepository(this ICqrsServiceBuilder builder)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register EventRepository........");
            builder.Services.AddSingleton(typeof(IEventRepository), typeof(EventRepository));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register EventRepository........Done!");
            return builder;
        }

        private static ICqrsServiceBuilder AddDefaultEventQueue(this ICqrsServiceBuilder builder)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register EventQueue........");
            builder.Services.AddSingleton(typeof(IEventQueue), typeof(EventQueue));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register EventQueue........Done!");
            return builder;
        }


        /// <summary>
        /// Add cqrs services with default configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="cqrsServiceOptions"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddCqrsService(this IServiceCollection services, IConfiguration configuration,
            Action<CqrsServiceOptions> cqrsServiceOptions = null, ILogger logger = null)
        {
            logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"========Initiating CQRS service........");

            var builder = services.AddCqrsServiceBuilder(logger);

            try
            {
                logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Building CQRS configuration option........");

                if (cqrsServiceOptions != null)
                    services.Configure(cqrsServiceOptions);

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

                var commandOption = new CommandStoreOptions()
                {
                    SaveAll = options.SaveAll,
                    CommandLocalStorage = options.CommandLocalStorage
                };

                var snapshotOption = new SnapshotOptions()
                {
                    Interval = options.Interval,
                    LocalStorage = options.SnapshotLocalStorage
                };

                var eventStoreOption = new EventStoreOptions()
                {
                    EventLocalStorage = options.EventLocalStorage
                };

                logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"CQRS configuration option : {JsonConvert.SerializeObject(options)}");

                builder.Services.AddSingleton(commandOption);
                logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"CQRS Command configuration option : {JsonConvert.SerializeObject(commandOption)}");
                builder.Services.AddSingleton(snapshotOption);
                logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"CQRS Snapshot configuration option : {JsonConvert.SerializeObject(snapshotOption)}");
                builder.Services.AddSingleton(eventStoreOption);
                logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"CQRS Event configuration option : {JsonConvert.SerializeObject(eventStoreOption)}");

                logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Finish produce CQRS configuration option........");

                builder.AddDefaultSerializer()
                    .AddDefaultEventSourcingDb(options.DbContextOption)
                    .AddDefaultCommandStore()
                    .AddDefaultCommandQueue()
                    .AddDefaultEventStore()
                    .AddDefaultEventRepository()
                    .AddDefaultEventQueue();

            }
            catch (Exception ex)
            {
                logger?.LogInformation($"Error occurs when initiating CQRS service!. Stack Tracing: {ex}");
            }
            finally
            {
                logger?.LogInformation($"Finished initiating CQRS service!");
            }
            return builder;
        }

        /// <summary>
        /// Add default configuration for snapshot feature
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddSnapshotFeature(this ICqrsServiceBuilder builder,
            Action<SnapshotOptions> configure = null)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Enabling Snapshot feature........");
            builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Building snapshot configuration option........");
            var option = new SnapshotOptions();
            configure?.Invoke(option);

            builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Snapshot configuration: {JsonConvert.SerializeObject(option)}");

            builder.Services.Replace(new ServiceDescriptor(typeof(SnapshotOptions), option));

            builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register Snapshot configuration option........ Done!");

            builder.Services.AddSingleton(typeof(ISnapshotStrategy), typeof(SnapshotStrategy));

            builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register SnapshotStrategy........ Done!");

            builder.Services.AddSingleton(typeof(ISnapshotStore), typeof(SnapshotStore));

            builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register SnapshotStore........ Done!");

            builder.Services.AddSingleton<SnapshotRepository>();

            builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register SnapshotRepository........ Done!");

            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Enabling Snapshot feature........Done!");
            return builder;
        }

        /// <summary>
        /// Override the default snapshot repository with the new one
        /// </summary>
        /// <typeparam name="TSnapshotRepository"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddSnapshotRepository<TSnapshotRepository>(this ICqrsServiceBuilder builder)
            where TSnapshotRepository : IEventRepository
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding default Snapshot Repository........");

            builder.Services.Replace(new ServiceDescriptor(typeof(SnapshotRepository), typeof(TSnapshotRepository),
                ServiceLifetime.Singleton));

            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding default Snapshot Repository........ Done!");
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
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding default Serializer........");

            var descriptor = new ServiceDescriptor(typeof(ISerializer), typeof(TSerializer), ServiceLifetime.Singleton);
            builder.Services.Replace(descriptor);

            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding default Serializer........ Done!");
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
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Event Store Db Context........");
            builder.Services.AddDbContext<TContext>(dbOptions);
            //builder.Services.AddScoped<ICommandDbContext, TContext>();

            builder.Services.Replace(new ServiceDescriptor(typeof(IEventDbContext), typeof(TContext),
                ServiceLifetime.Scoped));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Event Store Db Context........ Done!");
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
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Snapshot Store Db Context........");
            builder.Services.AddDbContext<TContext>(dbOptions);
            //builder.Services.AddScoped<ISnapshotDbContext, TContext>();
            builder.Services.Replace(new ServiceDescriptor(typeof(ISnapshotDbContext), typeof(TContext),
                ServiceLifetime.Scoped));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Snapshot Store Db Context........ Done!");
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
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store Db Context........");
            builder.Services.AddDbContext<TContext>(dbOptions);
            //builder.Services.AddScoped<ICommandDbContext, TContext>();
            builder.Services.Replace(new ServiceDescriptor(typeof(ICommandDbContext), typeof(TContext),
                ServiceLifetime.Scoped));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store Db Context........ Done!");
            return builder;
        }

        /// <summary>
        /// Override the default event store with the new one
        /// </summary>
        /// <typeparam name="TEventStore"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddEventStore<TEventStore>(this ICqrsServiceBuilder builder)
            where TEventStore : IEventStore
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store........");

            builder.Services.Replace(new ServiceDescriptor(typeof(IEventStore), typeof(TEventStore),
                ServiceLifetime.Singleton));

            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store........ Done!");
            return builder;
        }

        /// <summary>
        /// Override default event repository with the new one
        /// </summary>
        /// <typeparam name="TEventRepository"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddEventRepository<TEventRepository>(this ICqrsServiceBuilder builder)
            where TEventRepository : IEventRepository
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Repository........");

            builder.Services.Replace(new ServiceDescriptor(typeof(IEventRepository), typeof(TEventRepository),
                ServiceLifetime.Singleton));
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Repository........ Done!");
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
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Event Queue........");
            builder.Services.Replace(new ServiceDescriptor(typeof(IEventQueue), typeof(TQueue),
                ServiceLifetime.Singleton));

            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Event Queue........ Done!");
            return builder;
        }

        /// <summary>
        /// Override the default command store with the new one
        /// </summary>
        /// <typeparam name="TCommandStore"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddCommandStore<TCommandStore>(this ICqrsServiceBuilder builder)
            where TCommandStore : ICommandStore
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store........");
            builder.Services.Replace(new ServiceDescriptor(typeof(ICommandStore), typeof(TCommandStore),
                ServiceLifetime.Singleton));

            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store........ Done!");
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
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Queue........");

            builder.Services.Replace(new ServiceDescriptor(typeof(ICommandQueue), typeof(TQueue),
                ServiceLifetime.Singleton));

            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Queue........ Done!");
            return builder;
        }

        /// <summary>
        /// Register all ICommandHandler && IEventHandler in the calling assembly
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionAction"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddHandlers(this ICqrsServiceBuilder builder,
            Action<CqrsHandlerOptions> optionAction = null)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register CQRS Handlers........");
            builder.AddCommandHandlers(optionAction);
            builder.AddEventHandlers(optionAction);
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register CQRS Handlers........ Done!");
            return builder;
        }

        /// <summary>
        /// Register all ICommandHandler exist in the given assembly or all running assembly context
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionAction"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddCommandHandlers(this ICqrsServiceBuilder builder,
            Action<CqrsHandlerOptions> optionAction = null)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register Command Handlers........");
            var options = new CqrsHandlerOptions();
            optionAction?.Invoke(options);

            builder.RegisterHandler(typeof(ICommand), typeof(ICommandHandler<>), typeof(CommandHandler<>), options);
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register Command Handlers........ Done!");
            return builder;
        }

        /// <summary>
        /// Register all IEventHandler exist in the given assembly or all running assembly context
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionAction"></param>
        /// <returns></returns>
        public static ICqrsServiceBuilder AddEventHandlers(this ICqrsServiceBuilder builder,
            Action<CqrsHandlerOptions> optionAction = null)
        {
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register Event Handlers........");
            var options = new CqrsHandlerOptions();
            optionAction?.Invoke(options);
            builder.RegisterHandler(typeof(IEvent), typeof(IEventHandler<>), typeof(Event.EventHandler<>), options);
            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register Event Handlers........ Done!");
            return builder;
        }
        private static ICqrsServiceBuilder RegisterHandler(this ICqrsServiceBuilder builder, Type argumentType,
            Type interfaceType, Type implementationType, CqrsHandlerOptions options)
        {
            var assemblyNames = options?.HandlerAssemblyNames?.ToList();

            var assemblies = assemblyNames != null && assemblyNames.Count > 0
                ? AppDomain.CurrentDomain.GetAssemblies().Where(e =>
                        assemblyNames.Contains(e.FullName) || assemblyNames.Contains(e.GetName().Name))
                    .ToList()
                : AppDomain.CurrentDomain.GetAssemblies().ToList();

            builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Number of loading Assemblies: {assemblies.Count}");

            int count = 0;

            foreach (var assembly in assemblies)
            {
                if (assembly == null) continue;

                builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Loading Assemblies: {assembly.FullName}........");

                var commands = AssemblyUtils.GetTypesDerivedFromType(assembly, argumentType);

                builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Number of Commands/Events: {commands.Length}........");
                
                foreach (var commandType in commands)
                {
                    builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Scanning Commands/Events type: {commandType}........");

                    var handlerType = interfaceType;
                    var implementedType = implementationType;
                    Type[] arg = { commandType };
                    var genericHandlerType = handlerType.MakeGenericType(arg);
                    var genericImplementedHandlerType = implementedType.MakeGenericType(arg);

                    var commandHandlerType = AssemblyUtils
                        .GetTypesDerivedFromType(assembly, genericImplementedHandlerType).ToList();

                    builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Found {commandHandlerType.Count} handlers matches with {commandType}........");

                    if (commandType != null && commandHandlerType.Count > 0)
                    {
                        builder.Services.AddSingleton(genericHandlerType, commandHandlerType[0]);
                        count++;

                        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register handler {commandHandlerType[0].FullName} for {commandType}........ Done!");
                    }
                    else
                    {
                        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"There is no Handler matches with {commandType}........");
                    }
                }
                
                builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Loading Assemblies: {assembly.FullName}........ Done!");
            }

            builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Total handlers has been registered: {count}........");

            return builder;
        }
    }
}
