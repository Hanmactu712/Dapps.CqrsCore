using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence;
using Dapps.CqrsCore.Persistence.Store;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsCore.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dapps.CqrsCore.AspNetCore;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
    private readonly ILogger<Mediator> _logger;

    public LoggingBehavior(ILogger<Mediator> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null");
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);

            // Reflection! Could be a performance concern
            Type myType = request.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            foreach (PropertyInfo prop in props)
            {
                object? propValue = prop?.GetValue(request, null);
                _logger.LogInformation("Property {Property} : {@Value}", prop?.Name, propValue);
            }
        }

        var sw = Stopwatch.StartNew();

        var response = await next();

        _logger.LogInformation("Handled {RequestName} with {Response} in {ms} ms", typeof(TRequest).Name, response, sw.ElapsedMilliseconds);
        sw.Stop();
        return response;
    }
}

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
        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register CommandStore.......");

        builder.Services.AddScoped(typeof(ICqrsCommandStore), typeof(CommandStore));

        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register CommandStore: {typeof(CommandStore)} ........Done!");

        return builder;
    }

    private static ICqrsServiceBuilder AddDefaultCommandDispatcher(this ICqrsServiceBuilder builder)
    {
        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register CommandDispatcher........");

        builder.Services.AddTransient(typeof(ICqrsCommandDispatcher), typeof(CommandDispatcher));

        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register CommandDispatcher: {typeof(CommandDispatcher)}........Done!");

        return builder;
    }

    private static ICqrsServiceBuilder AddDefaultEventStore(this ICqrsServiceBuilder builder)
    {
        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register EventStore........");

        builder.Services.AddTransient(typeof(ICqrsEventStore), typeof(EventStore));

        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register EventStore: {typeof(EventStore)}........Done!");

        return builder;
    }

    private static ICqrsServiceBuilder AddDefaultEventRepository(this ICqrsServiceBuilder builder)
    {
        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register EventRepository........");

        builder.Services.AddScoped(typeof(ICqrsEventRepository), typeof(EventRepository));

        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register EventRepository: {typeof(EventRepository)}........Done!");

        return builder;
    }

    private static ICqrsServiceBuilder AddDefaultEventDispatcher(this ICqrsServiceBuilder builder)
    {
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register EventQueue........");

        builder.Services.AddTransient(typeof(ICqrsEventDispatcher), typeof(EventDispatcher));

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register EventRepository:  {typeof(EventDispatcher)} ........Done!e!");

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

            //logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"CQRS configuration option : {JsonConvert.SerializeObject(options)}");

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
                .AddDefaultCommandDispatcher()
                .AddDefaultEventStore()
                .AddDefaultEventRepository()
                .AddDefaultEventDispatcher();

        }
        catch (Exception ex)
        {
            logger?.LogError($"Error occurs when initiating CQRS service!. Stack Tracing: {ex}");
        }
        finally
        {
            logger?.LogInformation($"Finished initiating CQRS services!");
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

        builder.Services.AddScoped(typeof(ISnapshotStore), typeof(SnapshotStore));

        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register SnapshotStore........ Done!");

        builder.Services.AddScoped<SnapshotRepository>();

        builder.Logger?.LogDebug(_messageFormat, DateTimeOffset.Now, $"Register SnapshotRepository........ Done!");

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Enabling Snapshot feature........Done!");

        return builder;
    }

    /// <summary>
    /// Override the default snapshot repository with the new one, the lifetime is Scoped by default.
    /// </summary>
    /// <typeparam name="TSnapshotRepository"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ICqrsServiceBuilder AddSnapshotRepository<TSnapshotRepository>(this ICqrsServiceBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TSnapshotRepository : ICqrsEventRepository
    {
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding default Snapshot Repository........");

        builder.Services.Replace(new ServiceDescriptor(typeof(SnapshotRepository), typeof(TSnapshotRepository), lifetime));

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding default Snapshot Repository with {typeof(SnapshotRepository)}........ Done!");

        return builder;
    }

    /// <summary>
    /// Override default Serializer, the lifetime is Singleton by default.
    /// </summary>
    /// <typeparam name="TSerializer"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ICqrsServiceBuilder AddSerializer<TSerializer>(this ICqrsServiceBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TSerializer : class, ISerializer
    {
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding default Serializer........");

        var descriptor = new ServiceDescriptor(typeof(ISerializer), typeof(TSerializer), lifetime);
        builder.Services.Replace(descriptor);

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding default Serializer: {typeof(TSerializer)}........ Done!");

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

        builder.Services.Replace(new ServiceDescriptor(typeof(IEventDbContext), typeof(TContext),
            ServiceLifetime.Scoped));
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Event Store Db Context: {typeof(TContext)}........ Done!");

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
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Snapshot Store Db Context: {typeof(TContext)}........ Done!");

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
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store Db Context: {typeof(TContext)}........ Done!");

        return builder;
    }

    /// <summary>
    /// Override the default event store with the new one, the lifetime is Scoped by default.
    /// </summary>
    /// <typeparam name="TEventStore"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ICqrsServiceBuilder AddEventStore<TEventStore>(this ICqrsServiceBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEventStore : ICqrsEventStore
    {
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store........");

        builder.Services.Replace(new ServiceDescriptor(typeof(ICqrsEventStore), typeof(TEventStore), lifetime));

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store: {typeof(TEventStore)}........ Done!");

        return builder;
    }

    /// <summary>
    /// Override default event repository with the new one, the lifetime is Scoped by default.
    /// </summary>
    /// <typeparam name="TEventRepository"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ICqrsServiceBuilder AddEventRepository<TEventRepository>(this ICqrsServiceBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEventRepository : ICqrsEventRepository
    {
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Repository........");

        builder.Services.Replace(new ServiceDescriptor(typeof(ICqrsEventRepository), typeof(TEventRepository), lifetime));

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Repository: {typeof(TEventRepository)}........ Done!");

        return builder;
    }

    /// <summary>
    /// Override default Event Queue, the lifetime is Transient by default.
    /// </summary>
    /// <typeparam name="TQueue"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ICqrsServiceBuilder AddEventDispatcher<TQueue>(this ICqrsServiceBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TQueue : ICqrsEventDispatcher
    {
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Event Queue........");

        builder.Services.Replace(new ServiceDescriptor(typeof(ICqrsEventDispatcher), typeof(TQueue), lifetime));

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Event Queue: {typeof(TQueue)}........ Done!");

        return builder;
    }

    /// <summary>
    /// Override the default command store with the new one, the lifetime is Scoped by default.
    /// </summary>
    /// <typeparam name="TCommandStore"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ICqrsServiceBuilder AddCommandStore<TCommandStore>(this ICqrsServiceBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TCommandStore : ICqrsCommandStore
    {
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store........");

        builder.Services.Replace(new ServiceDescriptor(typeof(ICqrsCommandStore), typeof(TCommandStore), lifetime));

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Store: {typeof(TCommandStore)}........ Done!");

        return builder;
    }

    /// <summary>
    /// Override default command queue, the lifetime is Transient by default.
    /// </summary>
    /// <typeparam name="TQueue"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ICqrsServiceBuilder AddCommandDispatcher<TQueue>(this ICqrsServiceBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TQueue : ICqrsCommandDispatcher
    {
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Queue........");

        builder.Services.Replace(new ServiceDescriptor(typeof(ICqrsCommandDispatcher), typeof(TQueue), lifetime));

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Overriding Command Queue: {typeof(TQueue)}........ Done!");
        return builder;
    }

    /// <summary>
    /// Register all ICommandHandler && IEventHandler in the calling assembly, or in the assemblies specified in the options.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="optionAction"></param>
    /// <returns></returns>
    public static ICqrsServiceBuilder AddHandlers(this ICqrsServiceBuilder builder,
        Action<CqrsHandlerOptions> optionAction = null)
    {
        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register CQRS Handlers........");

        var options = new CqrsHandlerOptions();
        optionAction?.Invoke(options);

        var assemblyNames = options?.HandlerAssemblyNames?.ToList();

        var assemblies = assemblyNames != null && assemblyNames.Count > 0
            ? AppDomain.CurrentDomain.GetAssemblies().Where(e =>
                    assemblyNames.Contains(e.FullName) || assemblyNames.Contains(e.GetName().Name)).ToList()

            : AppDomain.CurrentDomain.GetAssemblies().ToList();

        if (options.HandlerAssemblies != null && options.HandlerAssemblies.Any())
        {
            assemblies.AddRange(options.HandlerAssemblies.ToList());
        }

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies.ToArray()))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        builder.Logger?.LogInformation(_messageFormat, DateTimeOffset.Now, $"Register CQRS Handlers........ Done!");
        return builder;
    }
}
