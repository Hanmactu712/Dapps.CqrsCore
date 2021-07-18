[![Nuget](https://img.shields.io/nuget/v/Dapps.CqrsCore.AspNetCore)](https://www.nuget.org/packages/Dapps.CqrsCore.AspNetCore/)
[![Nuget](https://img.shields.io/nuget/dt/Dapps.CqrsCore.AspNetCore)](https://www.nuget.org/packages/Dapps.CqrsCore.AspNetCore/)

# ASP.NET Core CQRS Service

A project for supporting # ASP.NET Core CQRS Service & CQRS + ES pattern in ASP.NET Core applications.

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Table of Contents

[1. Introducing](#1-introducing)

[2. Getting Started](#3-getting-started)

## 1. Introducing

CQRS pattern is the common parttern is used to tackle the complicated application. 
This project is to provide a simple way to apply CQRS to any application using .net 5.

## 2. Getting Started

Here's all you need to get started (you can also check the sample project):

1. Add the [Dapps.CqrsCore.AspNetCore NuGet package](https://www.nuget.org/packages/Dapps.CqrsCore.AspNetCore/) to your ASP.NET Core project.

2. Configure CQRS service dependency injection
   a. With the default configuration, just need to do this: 
``` csharp
    var currentAssembly = Assembly.GetAssembly(typeof(Program)).GetName().Name;
    services.AddCqrsService(_configuration,
    config =>
    {
        config.SaveAll = true;
        config.CommandLocalStorage = "D:\\LocalStorage";
        config.EventLocalStorage = "D:\\LocalStorage";
        config.SnapshotLocalStorage = "D:\\LocalStorage";

        config.DbContextOption = sql =>
            sql.UseSqlServer(_configuration.GetConnectionString("CqrsConnection"),
                migrationOps => migrationOps.MigrationsAssembly(currentAssembly));
    }, _logger)
```
with this confgiuration, the system will auto register all the neccessary services for running cqrs such as Serializer, Command Store, Command Queue, Event Store, Event Repository, Event Queue.
The configuration for : 
``` csharp
    public class CqrsServiceOptions
    {        
        /// <summary>
        /// Option to save all the commands. If false, only save scheduled commands
        /// </summary>
        public bool SaveAll { get; set; } = false;
        /// <summary>
        /// Local storage path to store command data when boxing & unboxing. Default is {the current location of execution file}/Commands.
        /// </summary>
        public string CommandLocalStorage { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// Local storage path to store event data when boxing & unboxing. Default is {the current location of execution file}/EventSourcing.
        /// </summary>
        public string EventLocalStorage { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Indicate the snapshot will be taken after how many version of aggregate. Default is 200.
        /// </summary>
        public int Interval { get; set; } = 200;

        /// <summary>
        /// Local storage path to store snapshot data when boxing & unboxing. Default is {the current location of execution file}/Snapshots.
        /// </summary>
        public string SnapshotLocalStorage { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Default Db Builder Option for CQRS service
        /// </summary>
        public Action<DbContextOptionsBuilder> DbContextOption { get; set; }
    }
```
### Adding common endpoint groupings using Swagger



Examples of the configuration can be found in the sample API project
