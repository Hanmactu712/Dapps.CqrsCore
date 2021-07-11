using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Exceptions;
using System;
using System.Reflection;

namespace Dapps.CqrsCore.Aggregate
{
    public abstract class AggregateState
    {
        public void Apply(IEvent ev)
        {
            var when = GetType().GetMethod("When", new[] { ev.GetType() });

            if (when == null)
            {
                throw new MethodNotFoundException(GetType(), "When", ev.GetType());
            }

            when.Invoke(this, new object[] { ev });
        }

        public void AssignStateValue(IEvent message)
        {
            var targetFields = this.GetType().GetProperties();
            var sourceFields = message.GetType().GetFields();

            foreach (var targetField in targetFields)
            {
                foreach (var sourceField in sourceFields)
                {
                    if (sourceField.Name.Equals(targetField.Name))
                    {
                        targetField.SetValue(this, sourceField.GetValue(message));
                    }
                }
            }
        }
    }
}
