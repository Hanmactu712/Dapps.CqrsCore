using Dapps.CqrsCore.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using Dapps.CqrsCore.Utilities;

namespace Dapps.CqrsCore.Event
{
    public class EventHandlers
    {
        private readonly ILogger _logger;
        public EventHandlers(IEventQueue queue, ILogger logger)
        {
            _logger = logger;
            var currentAsm = Assembly.GetCallingAssembly();
            RegisterHandlerForEvents(queue, currentAsm);

        }

        private void RegisterHandlerForEvents(IEventQueue queue, Assembly asm)
        {

            var types = AssemblyUtils.GetTypesDerivedFromType(asm, typeof(Event));

            foreach (var type in types)
            {
                try
                {

                    var handle = GetType().GetMethod("Handle", new[] { type });

                    if (handle == null)
                    {
                        throw new MethodNotFoundException(GetType(), "Handle", type);
                    }

                    RegisterHandleToQueue(queue, type, handle);
                }
                catch (Exception exception)
                {
                    _logger?.LogInformation(exception.Message);
                }
            }

        }

        private void RegisterHandleToQueue(IEventQueue queue, Type eventType, MethodInfo handle)
        {
            var method = typeof(IEventQueue).GetMethods()
                .FirstOrDefault(i => i.Name == "Subscribe" && i.IsGenericMethod);

            if (method == null) return;

            var generic = method.MakeGenericMethod(eventType);

            var genActionType = typeof(Action<>);
            var actionType = genActionType.MakeGenericType(eventType);

            //var target = Activator.CreateInstance(eventType);

            var action = handle.CreateDelegate(actionType, this);

            object[] parametersArray = { action };
            generic.Invoke(queue, parametersArray);

        }
    }
}
