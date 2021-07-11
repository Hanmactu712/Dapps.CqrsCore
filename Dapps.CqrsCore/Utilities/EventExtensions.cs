using Dapps.CqrsCore.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapps.CqrsCore.Utilities
{
    public static class EventExtensions
    {
        public static T MapTo<T>(this object message, Dictionary<string, string> exceptionMapping = null, List<string> ignoreFields = null) where T : class
        {
            var t = Activator.CreateInstance(typeof(T));

            //return (T)t;
            return message.MapTo((T)t, exceptionMapping, ignoreFields);
        }

        //public static T MapTo<T>(this IEvent message, T target, Dictionary<string, string> exceptionMapping = null) where T : class
        //{
        //    //var t = Activator.CreateInstance(typeof(T));

        //    var targetFields = typeof(T).GetProperties();
        //    var sourceFields = message.GetType().GetFields();
        //    var sourceProperties = message.GetType().GetProperties();

        //    foreach (var targetField in targetFields)
        //    {
        //        foreach (var sourceField in sourceFields)
        //        {
        //            if (sourceField.Name.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase))
        //            {
        //                targetField.SetValue(target, sourceField.GetValue(message));
        //            }

        //            if (exceptionMapping == null) continue;

        //            if (exceptionMapping.Any(e =>
        //                e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                var mapping = exceptionMapping.SingleOrDefault(e =>
        //                    e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase));
        //                if (mapping.Value.Equals(sourceField.Name, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    targetField.SetValue(target, sourceField.GetValue(message));
        //                }
        //            }
        //        }

        //        foreach (var sourceField in sourceProperties)
        //        {
        //            if (sourceField.Name.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase))
        //            {
        //                targetField.SetValue(target, sourceField.GetValue(message));
        //            }

        //            if (exceptionMapping == null) continue;

        //            if (exceptionMapping.Any(e =>
        //                e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                var mapping = exceptionMapping.SingleOrDefault(e =>
        //                    e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase));
        //                if (mapping.Value.Equals(sourceField.Name, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    targetField.SetValue(target, sourceField.GetValue(message));
        //                }
        //            }
        //        }
        //    }

        //    return target;
        //}

        public static T MapTo<T>(this object message, T target, Dictionary<string, string> exclusiveMapping = null, List<string> ignoreFields = null) where T : class
        {
            //var t = Activator.CreateInstance(typeof(T));

            var targetFields = typeof(T).GetProperties();
            var sourceFields = message.GetType().GetFields();
            var sourceProperties = message.GetType().GetProperties();

            foreach (var targetField in targetFields)
            {
                if (ignoreFields != null &&
                    ignoreFields.Any(e => e.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (exclusiveMapping != null)
                {
                    if (exclusiveMapping.Any(e =>
                        e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        var mapping = exclusiveMapping.SingleOrDefault(e =>
                            e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase));

                        var sourceField = sourceFields.SingleOrDefault(e =>
                            e.Name.Equals(mapping.Value, StringComparison.OrdinalIgnoreCase) &&
                            e.FieldType.Name.Equals(targetField.PropertyType.Name));

                        //check if field exists
                        if (sourceField != null)
                        {
                            targetField.SetValue(target, sourceField.GetValue(message));
                        }
                        else
                        {
                            //check if properties exists
                            var sourceProperty = sourceProperties.SingleOrDefault(e =>
                                e.Name.Equals(mapping.Value, StringComparison.OrdinalIgnoreCase) &&
                                e.PropertyType.Name.Equals(targetField.PropertyType.Name));

                            if (sourceProperty != null)
                            {
                                targetField.SetValue(target, sourceProperty.GetValue(message));
                            }
                        }
                    }
                }

                //mapping based on name if no exclusive fields nor ignore fields are specified

                foreach (var sourceField in sourceFields)
                {
                    if (sourceField.Name.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase) &&
                        sourceField.FieldType.Name.Equals(targetField.PropertyType.Name))
                    {
                        targetField.SetValue(target, sourceField.GetValue(message));
                    }

                    //if (exclusiveMapping == null) continue;

                    //if (exclusiveMapping.Any(e =>
                    //    e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    var mapping = exclusiveMapping.SingleOrDefault(e =>
                    //        e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase));
                    //    if (mapping.Value.Equals(sourceField.Name, StringComparison.OrdinalIgnoreCase))
                    //    {
                    //        targetField.SetValue(target, sourceField.GetValue(message));
                    //    }
                    //}
                }

                foreach (var sourceField in sourceProperties)
                {
                    if (sourceField.Name.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase) &&
                        sourceField.PropertyType.Name.Equals(targetField.PropertyType.Name))
                    {
                        targetField.SetValue(target, sourceField.GetValue(message));
                    }

                    //if (exclusiveMapping == null) continue;

                    //if (exclusiveMapping.Any(e =>
                    //    e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    var mapping = exclusiveMapping.SingleOrDefault(e =>
                    //        e.Key.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase));
                    //    if (mapping.Value.Equals(sourceField.Name, StringComparison.OrdinalIgnoreCase))
                    //    {
                    //        targetField.SetValue(target, sourceField.GetValue(message));
                    //    }
                    //}
                }
            }

            return target;
        }
    }
}
