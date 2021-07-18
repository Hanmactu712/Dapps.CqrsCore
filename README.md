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

The CQRS service contains these mains components: 

### A. WRITE side

1. Serializer - using to serialize / deserialize command & event messsage before persist or get from store database

3. Command (Represent for Write side in the CQRS pattern)
   - Base Command class, Command interface. 
   - Command Store - to persist all the command messages
   - Command Queue - to queue the command message and deliver it to corresponding handler which handle the business logic and publish corresponding events
   - Command Handler - to handle the command message received from command queue
   
3. Event (Represent for Write side in the CQRS pattern)
   - Base Event class, Event interface
   - Event Store - to persist all the event messages
   - Evemt Queue - to queue the event message and deliver it to corresponding handler which handle the business logic and save data to the READ side of CQRS
   - Event Repository - to processing event logic and communicate with event store to persist event message
   - Event Handler - to handle the event message received from event queue.
   
5. Snapshot

### A. READ side

With read side, you can use many ways to build the project such as REST API, GRAPHQL API or MVC. It's up to your purpose and target.

## 2. Getting Started

Here's all you need to get started (you can also check the [sample project](https://github.com/Hanmactu712/Dapps.CqrsCore/tree/master/Dapps.CqrsSample) for more information):

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

The configuration option is: 
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

In case you want to using your owner implementation of such Serializer, Command Store, Event Store Event Queue, Event Repository, use can register them to override the default implemenation. 

``` csharp
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
      .AddSerializer<Serializer>() //add custom serializer if needed
      .AddCommandStore<CommandStore>( ) //add custom CommandStore if needed
      .AddCommandQueue<CommandQueue>() //add custom CommandQueue if needed
      .AddEventStore<EventStore>() //add custom EventStore if needed
      .AddEventQueue<EventQueue>() //add custom EventQueue if needed
      .AddEventRepository<EventRepository>() //add custom EventRepository if needed
```

You can also change the default configuration for store database to separate the database store.

``` csharp
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

``` csharp
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
      .AddSnapshotFeature(option =>
      {
          option.Interval = 10;
          option.LocalStorage = "C:\\Users\\ducdd\\OneDrive\\Desktop\\LocalStorage";
      })
```

same with command & events, you can override the default snapshot store db configuration, snapshot repository as well.

``` csharp
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
```

