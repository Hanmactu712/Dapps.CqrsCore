﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dapps.CqrsCore.AspNetCore;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence;
using Dapps.CqrsCore.Persistence.Read;
using Dapps.CqrsCore.Persistence.Store;
using Dapps.CqrsCore.Snapshots;
using Dapps.CqrsSample.Data;
using Dapps.CqrsSample.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample
{
    class Program
    {
        private static IConfigurationRoot _configuration;
        private static ILogger _logger;
        static void Main(string[] args)
        {
            Console.WriteLine("Command processing!");

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    //seeding database
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory;

            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile($"{rootPath}\\hostsettings.json", optional: true);
                    configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configuration) =>
                {
                    configuration.Sources.Clear();
                    IHostEnvironment env = hostContext.HostingEnvironment;
                    configuration.AddJsonFile($"{rootPath}\\appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"{rootPath}\\appsettings.{env.EnvironmentName}.json", true, true);

                    _configuration = configuration.Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    _logger = services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

                    var currentAssembly = Assembly.GetAssembly(typeof(Program));

                    //Register CQRS service. Using services.AddCqrsService() only if want to use default configuration & services
                    services.AddCqrsService(_configuration,
                            config =>
                            {
                                config.SaveAll = true;
                                config.CommandLocalStorage = "C:\\Users\\ducdd\\OneDrive\\Desktop\\LocalStorage";
                                config.EventLocalStorage = "C:\\Users\\ducdd\\OneDrive\\Desktop\\LocalStorage";
                                config.SnapshotLocalStorage = "C:\\Users\\ducdd\\OneDrive\\Desktop\\LocalStorage";

                                config.DbContextOption = sql =>
                                    sql.UseSqlServer(_configuration.GetConnectionString("CqrsConnection"),
                                        migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
                            }, _logger)
                        //add custom command Store DB if needed
                        .AddCommandStoreDb<CommandDbContext>(option =>
                        {
                            option.UseSqlServer(_configuration.GetConnectionString("CommandDbConnection"),
                                migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
                            //option.UseInMemoryDatabase("CommandDb");
                        })
                        //add custom event Store DB if needed
                        .AddEventStoreDb<EventDbContext>(option =>
                        {
                            option.UseSqlServer(_configuration.GetConnectionString("EventDbConnection"),
                                migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
                            //option.UseInMemoryDatabase("EventDb");
                        })
                        .AddSerializer<Serializer>() //add custom serializer if needed
                        .AddCommandStore<CommandStore>() //add custom CommandStore if needed
                        .AddCommandDispatcher<CommandDispatcher>() //add custom CommandQueue if needed
                        .AddEventStore<EventStore>() //add custom EventStore if needed
                        .AddEventDispatcher<EventDispatcher>() //add custom EventQueue if needed
                        .AddEventRepository<EventRepository>() //add custom EventRepository if needed
                        .AddSnapshotFeature(option =>
                        {
                            option.Interval = 10;
                            option.LocalStorage = "C:\\Users\\ducdd\\OneDrive\\Desktop\\LocalStorage";
                        })
                        //add custom snapshot repository if needed
                        .AddSnapshotRepository<SnapshotRepository>()
                        //add custom snapshot Store DB if needed
                        .AddSnapshotStoreDb<SnapshotDbContext>(option =>
                        {
                            option.UseSqlServer(_configuration.GetConnectionString("SnapshotDbConnection"),
                                migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
                            //option.UseInMemoryDatabase("SnapshotDb");
                        })
                        //This functions will looking for all command handlers & event handlers to register to Service Providers
                        .AddHandlers(option => option.HandlerAssemblies = new List<Assembly>()
                        {
                            currentAssembly
                        });

                    //add db context & repository for read part of the application
                    services.AddDbContext<ApplicationDbContext>(option =>
                    {
                        option.UseSqlServer(_configuration.GetConnectionString("ApplicationDbConnection"),
                            migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
                        //option.UseInMemoryDatabase("ApplicationDb");
                    });

                    //add db context for read side
                    services
                        .AddScoped<IEfRepository<Article, ApplicationDbContext>,
                            EfRepository<Article, ApplicationDbContext>>();

                    services.AddHostedService<HostedService>();
                });
        }
    }
}
