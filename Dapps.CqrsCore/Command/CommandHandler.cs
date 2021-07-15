﻿using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Dapps.CqrsCore.Utilities;

namespace Dapps.CqrsCore.Command
{
    /// <summary>
    /// Base command handlers which register handlers to command queue and commit changes changes to event sourcing as well as public event for subscribers
    /// </summary>
    public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
    {
        private readonly IEventRepository _repository;
        private readonly IEventQueue _eventQueue;
        private readonly ILogger _logger;

        protected CommandHandler(ICommandQueue queue, IEventRepository eventRepository, IEventQueue eventQueue, ILogger logger)
        {
            _eventQueue = eventQueue;
            _logger = logger;
            _repository = eventRepository;
            queue.Subscribe<TCommand>(Handle);

            //var currentAsm = Assembly.GetCallingAssembly();
            //register Handle to command queue
            //RegisterHandlerForCommands(queue, currentAsm);
        }

        //private void RegisterHandlerForCommands(ICommandQueue queue, Assembly asm)
        //{

        //    var types = AssemblyUtils.GetTypesDerivedFromType(asm, typeof(Command));

        //    foreach (var type in types)
        //    {
        //        try
        //        {
        //            var handle = GetType().GetMethod("Handle", new[] { type });

        //            if (handle == null)
        //            {
        //                throw new MethodNotFoundException(GetType(), "Handle", type);
        //            }

        //            RegisterHandleToQueue(queue, type, handle);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogInformation(ex.Message);
        //        }
        //    }

        //}

        //private void RegisterHandleToQueue(ICommandQueue queue, Type commandType, MethodInfo handle)
        //{
        //    var method = typeof(ICommandQueue).GetMethods()
        //        .FirstOrDefault(i => i.Name == "Subscribe" && i.IsGenericMethod);

        //    if (method == null) return;

        //    var generic = method.MakeGenericMethod(commandType);

        //    var genActionType = typeof(Action<>);
        //    var actionType = genActionType.MakeGenericType(commandType);

        //    //var target = Activator.CreateInstance(eventType);

        //    var action = handle.CreateDelegate(actionType, this);

        //    object[] parametersArray = { action };
        //    generic.Invoke(queue, parametersArray);

        //}

        /// <summary>
        /// Get aggregate from event sourcing
        /// </summary>
        /// <typeparam name="T">aggregate</typeparam>
        /// <param name="id">aggregate id</param>
        /// <returns></returns>
        public virtual T Get<T>(Guid id) where T : AggregateRoot
        {
            return _repository.Get<T>(id);
        }

        /// <summary>
        /// commit all changes of a aggregate to event sourcing
        /// </summary>
        /// <param name="aggregate">aggregate need to save</param>
        public virtual void Commit(AggregateRoot aggregate)
        {
            var changes = _repository.Save(aggregate);
            foreach (var change in changes)
            {
                _eventQueue.Publish(change);
            }
        }

        public abstract void Handle(TCommand message);
    }
}
