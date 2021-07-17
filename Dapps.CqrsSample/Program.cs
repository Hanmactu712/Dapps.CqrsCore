using System;
using System.IO;
using System.Reflection;
using Dapps.CqrsCore.AspNetCore;
using Dapps.CqrsCore.Persistence.Read;
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
        static void Main(string[] args)
        {
            Console.WriteLine("Command processing!");

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    //var readContext = services.GetRequiredService<UserDBContext>();

                    //readContext.Database.Migrate();

                    //var config = services.GetRequiredService<IConfiguration>();

                    //var password = config["SeedUserPassword"];

                    //var writeContext = services.GetRequiredService<PersistenceDBContext>();
                    //writeContext.Database.Migrate();

                    ////SeedData.Initialize(services, password).Wait();
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
                    var currentAssembly = Assembly.GetAssembly(typeof(Program)).GetName().Name;

                    //services.AddCqrsService(_configuration, option =>
                    //    {
                    //        option.SaveAll = Convert.ToBoolean(_configuration.GetSection("CoreSettings:SaveAll").Value);
                    //        //option.DbContextOption = sql =>
                    //        //    sql.UseSqlServer(_configuration.GetConnectionString("CqrsConnection"),
                    //        //        migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
                    //        option.DbContextOption = sql => sql.UseInMemoryDatabase("CqrsConnection");
                    //    })
                    //    .AddHandlers();

                    services.AddCqrsService(_configuration, config =>
                        {
                            config.SaveAll = Convert.ToBoolean(_configuration.GetSection("CoreSettings:SaveAll").Value);
                        })
                    //services.AddCqrsService(_configuration)
                        .AddCommandStoreDb<CommandDbContext>(option =>
                        {
                            option.UseSqlServer(_configuration.GetConnectionString("CommandDbConnection"),
                                    migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
                            //option.UseInMemoryDatabase("CommandDb");
                        })
                        .AddEventStoreDb<EventDbContext>(option =>
                        {
                            option.UseSqlServer(_configuration.GetConnectionString("EventDbConnection"),
                                migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
                            //option.UseInMemoryDatabase("EventDb");
                        })
                        //.AddSnapshotFeature()
                        //.AddSnapshotStoreDb<SnapshotDbContext>(option => option.UseInMemoryDatabase("SnapshotDb"))
                        .AddHandlers();

                    //add db context & repository for read part of the application
                    services.AddDbContext<ApplicationDbContext>(option =>
                    {
                        option.UseSqlServer(_configuration.GetConnectionString("ApplicationDbConnection"),
                            migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
                        //option.UseInMemoryDatabase("ApplicationDb");
                    });

                    services.AddScoped<IEfRepository<Article, ApplicationDbContext>, EfRepository<Article, ApplicationDbContext>>();

                    services.AddHostedService<HostedService>();
                });
        }
    }
}
