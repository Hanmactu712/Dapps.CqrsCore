using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Exceptions;

namespace Dapps.CqrsCore.Aggregate;

public abstract class AggregateState
{
    public void Apply(ICqrsEvent ev)
    {
        var when = GetType().GetMethod("When", new[] { ev.GetType() });

        if (when == null)
        {
            throw new MethodNotFoundException(GetType(), "When", ev.GetType());
        }

        when.Invoke(this, [ev]);
    }

    public void AssignStateValue(ICqrsEvent message)
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
