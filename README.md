[![Dapps.CqrsCore.AspNetCore](https://img.shields.io/nuget/v/Dapps.CqrsCore.AspNetCore)](https://www.nuget.org/packages/Dapps.CqrsCore.AspNetCore/)
[![Dapps.CqrsCore.AspNetCore](https://img.shields.io/nuget/dt/Dapps.CqrsCore.AspNetCore)](https://www.nuget.org/packages/Dapps.CqrsCore.AspNetCore/)

[![Dapps.CqrsCore](https://img.shields.io/nuget/r/Dapps.CqrsCore)](https://www.nuget.org/packages/Dapps.CqrsCore/)
[![Dapps.CqrsCore](https://img.shields.io/nuget/dt/Dapps.CqrsCore)](https://www.nuget.org/packages/Dapps.CqrsCore/)

[![Dapps.CqrsCore.Persistence](https://img.shields.io/nuget/w/Dapps.CqrsCore.Persistence)](https://www.nuget.org/packages/Dapps.CqrsCore.Persistence/)
[![Dapps.CqrsCore.Persistence](https://img.shields.io/nuget/dt/Dapps.CqrsCore.Persistence)](https://www.nuget.org/packages/Dapps.CqrsCore.Persistence/)

# ASP.NET Core CQRS Service

A project for supporting # ASP.NET Core CQRS Service & CQRS + ES pattern in ASP.NET Core applications.

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Table of Contents

[1. Introducing](#1-introducing)

[2. Getting Started](#2-getting-started)

[3. Dependency](#3-dependency)

## 1. Introducing

CQRS pattern is the common parttern is used to tackle the complicated application.
This project is to provide a simple way to apply CQRS to any application using .net 5.

The CQRS service contains these mains components:

### A. WRITE side

1. Serializer - using to serialize / deserialize command & event messsage before persist or get from store database

2. Aggregate - The main object which contains all the data properties & logic to handle a business

   - Aggregate Root - Base class of any aggregate object

3. Command (Represent for Write side in the CQRS pattern)
   - Base Command class, Command interface.
   - Command Store - to persist all the command messages
   - Command Dispatcher - to publish the command message and deliver it to corresponding handler which handle the business logic and publish corresponding events
   - Command Handler - to handle the command message received from command queue
4. Event (Represent for Write side in the CQRS pattern)
   - Base Event class, Event interface
   - Event Store - to persist all the event messages
   - Event Dispatcher - to publish the event message and deliver it to corresponding handler which handle the business logic and save data to the READ side of CQRS
   - Event Repository - to processing event logic and communicate with event store to persist event message
   - Event Handler - to handle the event message received from event queue.
5. Snapshot - the mechanism to capture the state of an aggregate after a specific version of events.
   - Snapshot store - to persist snapshot records
   - Snapshot repository - to handle logic relating to snapshot

### A. READ side

With read side, you can use many ways to build the project such as REST API, GRAPHQL API or MVC. It's up to your purpose and target.

## 2. Getting Started

Here's all you need to get started (you can also check the [sample project](https://github.com/Hanmactu712/Dapps.CqrsCore/tree/master/Dapps.CqrsSample) for more information):

### 1. Add the [Dapps.CqrsCore.AspNetCore NuGet package](https://www.nuget.org/packages/Dapps.CqrsCore.AspNetCore/) to your ASP.NET Core project.

### 2. Configure CQRS service dependency injection

a. With the default configuration, just need to do this:

```csharp
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

with this confgiuration, the system will auto register all the neccessary services for running cqrs such as Serializer, Command Store, Command Dispatcher, Event Store, Event Repository, Event Dispatcher.
In the latest version, the package will leverage [MediatR](https://www.nuget.org/packages/MediatR/) - using latest free version 12.5.0 for dispatching commands and events in memory.
For using Event broker such as Kafka, RabbitMQ,... , you need to customize the ICqrsEventDispatcher & ICqrsEventHandler accordingly.

The configuration option is:

```csharp
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

In case you want to using your owner implementation of such Serializer, Command Store, Event Store Event Queue, Event Repository, use can register them to override the default implemenation.

```csharp
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
    .AddSerializer<CqrsSerializer>() //add custom serializer if needed
    .AddCommandStore<CqrsCommandStore>() //add custom CommandStore if needed
    .AddCommandDispatcher<CqrsCommandDispatcher>() //add custom CommandDispatcher if needed
    .AddEventStore<CqrsEventStore>() //add custom EventStore if needed
    .AddEventDispatcher<CqrsEventDispatcher>() //add custom EventDispatcher if needed
    .AddEventRepository<CqrsEventRepository>() //add custom EventRepository if needed
```

You can also change the default configuration for store database to separate the database store.

```csharp
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
```

The Dapps.CqrsCore.AspNetCore project also provides a mechanism for snapshoting event incase the number of events of an aggregate is too big or incresing too fast there for it slow down the business logic processing. To enable the snapshot feature, using this:

```csharp
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
      .AddSnapshotFeature(option =>
      {
          option.Interval = 10;
          option.LocalStorage = "D:\\LocalStorage";
      })
```

same with command & events, you can override the default snapshot store db configuration, snapshot repository as well.

```csharp
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
      .AddSnapshotFeature(option =>
      {
          option.Interval = 10;
          option.LocalStorage = "D:\\LocalStorage";
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
```

### 3. Build your aggregates, commands, events as well as command handles, events handles to fit with your business.

Node that:

- All the aggregate should be derived from Aggregate Root.
- All the handler should be derived from ICqrsCommandHandler or ICqrsEventHandler accordingly. For best practice, 1 command/event should be handle by 1 corresponding class Command Handler / Event Handler
- The Dapps.CqrsCore.AspNetCore project provide a function to supports register all the command handlers & event handlers via service dependency injection. See sample below to know how to use this function

```csharp
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
      //This functions will looking for all command handlers & event handlers to register to Service Providers
      .AddHandlers(option => option.HandlerAssemblyNames = new List<string>()
      {
          currentAssembly
      })
```

## 3. Dependency

This project using these dependency:

#### 1. [Dapps.CqrsCore](https://www.nuget.org/packages/Dapps.CqrsCore/)

#### 2. [Dapps.CqrsCore.Persistence](https://www.nuget.org/packages/Dapps.CqrsCore.Persistence/)

#### 3. [Ardalis.Specification](https://www.nuget.org/packages/ardalis.specification/) (great thanks to Ardalis)

#### 4. [Ardalis.Specification.EntityFrameworkCore](https://www.nuget.org/packages/ardalis.specification.entityframeworkcore/) (great thanks to Ardalis)

#### 5. [MediatR](https://www.nuget.org/packages/MediatR/) - using latest free version 12.5.0
