using System;
using System.IO;
using Dapps.CqrsCore.AspNetCore;
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
            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("D:\\Workplace\\Projects\\Dapps.CqrsCore\\Dapps.CqrsSample\\bin\\Debug\\netcoreapp3.1\\hostsettings.json", optional: true);
                    configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configuration) =>
                {
                    configuration.Sources.Clear();
                    IHostEnvironment env = hostContext.HostingEnvironment;
                    configuration.AddJsonFile("D:\\Workplace\\Projects\\Dapps.CqrsCore\\Dapps.CqrsSample\\bin\\Debug\\netcoreapp3.1\\appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"D:\\Workplace\\Projects\\Dapps.CqrsCore\\Dapps.CqrsSample\\bin\\Debug\\netcoreapp3.1\\appsettings.{env.EnvironmentName}.json", true, true);

                    _configuration = configuration.Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddCqrsService(_configuration, option =>
                        {
                            option.SaveAll = Convert.ToBoolean(_configuration.GetSection("CoreSettings:SaveAll").Value);
                        });
                    //services.AddHostedService<CommandService>();
                });
        }
    }
}
