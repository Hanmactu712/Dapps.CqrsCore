using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapps.CqrsCore.Utilities
{
    public static class ObjectMapping
    {
        /// <summary>
        /// mapping value from a object to new other object based on their fields/properties name
        /// </summary>
        /// <typeparam name="T">type of target object</typeparam>
        /// <param name="message">source object</param>
        /// <param name="exceptionMapping">list exclusive pair of fields/properties need to map value</param>
        /// <param name="ignoreFields">name of field/properties need to ignore mapping</param>
        /// <returns>target object</returns>
        public static T MapTo<T>(this object message, Dictionary<string, string> exceptionMapping = null,
            List<string> ignoreFields = null) where T : class
        {
            var t = Activator.CreateInstance(typeof(T));
            return message.MapTo((T)t, exceptionMapping, ignoreFields);
        }

        /// <summary>
        /// mapping value from a object to existing object based on their fields/properties name
        /// </summary>
        /// <typeparam name="T">type of target object</typeparam>
        /// <param name="source">source object</param>
        /// <param name="target">target object</param>
        /// <param name="exclusiveMapping">list exclusive pair of fields/properties need to map value</param>
        /// <param name="ignoreFields">name of field/properties need to ignore mapping</param>
        /// <returns>target object</returns>
        public static T MapTo<T>(this object source, T target, Dictionary<string, string> exclusiveMapping = null,
            List<string> ignoreFields = null) where T : class
        {
            target = source.MapFromFieldsToFields(target, exclusiveMapping, ignoreFields);
            target = source.MapFromFieldsToProperties(target, exclusiveMapping, ignoreFields);
            target = source.MapFromPropertiesToFields(target, exclusiveMapping, ignoreFields);
            target = source.MapFromPropertiesToProperties(target, exclusiveMapping, ignoreFields);

            return target;
        }

        /// <summary>
        /// mapping value from a object to existing object based on source field name & target property name
        /// </summary>
        /// <typeparam name="T">type of target object</typeparam>
        /// <param name="source">source object</param>
        /// <param name="target">target object</param>
        /// <param name="exclusiveMapping">list exclusive pair of fields/properties need to map value</param>
        /// <param name="ignoreFields">name of field/properties need to ignore mapping</param>
        /// <returns>target object</returns>
        public static T MapFromFieldsToProperties<T>(this object source, T target,
            Dictionary<string, string> exclusiveMapping = null, List<string> ignoreFields = null) where T : class
        {
            var targetFields = typeof(T).GetProperties();
            var sourceFields = source.GetType().GetFields();

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
                            targetField.SetValue(target, sourceField.GetValue(source));
                        }
                    }
                }

                //mapping based on name if no exclusive fields nor ignore fields are specified

                foreach (var sourceField in sourceFields)
                {
                    if (sourceField.Name.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase) &&
                        sourceField.FieldType.Name.Equals(targetField.PropertyType.Name))
                    {
                        targetField.SetValue(target, sourceField.GetValue(source));
                    }
                }

            }

            return target;
        }

        /// <summary>
        /// mapping value from a object to existing object based on source property name & target property name
        /// </summary>
        /// <typeparam name="T">type of target object</typeparam>
        /// <param name="source">source object</param>
        /// <param name="target">target object</param>
        /// <param name="exclusiveMapping">list exclusive pair of fields/properties need to map value</param>
        /// <param name="ignoreFields">name of field/properties need to ignore mapping</param>
        /// <returns>target object</returns>
        public static T MapFromPropertiesToProperties<T>(this object source, T target,
            Dictionary<string, string> exclusiveMapping = null, List<string> ignoreFields = null) where T : class
        {
            var targetFields = typeof(T).GetProperties();
            var sourceFields = source.GetType().GetProperties();

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
                            e.PropertyType.Name.Equals(targetField.PropertyType.Name));

                        //check if field exists
                        if (sourceField != null)
                        {
                            targetField.SetValue(target, sourceField.GetValue(source));
                        }
                    }
                }

                //mapping based on name if no exclusive fields nor ignore fields are specified

                foreach (var sourceField in sourceFields)
                {
                    if (sourceField.Name.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase) &&
                        sourceField.PropertyType.Name.Equals(targetField.PropertyType.Name))
                    {
                        targetField.SetValue(target, sourceField.GetValue(source));
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// mapping value from a object to existing object based on source field name & target field name
        /// </summary>
        /// <typeparam name="T">type of target object</typeparam>
        /// <param name="source">source object</param>
        /// <param name="target">target object</param>
        /// <param name="exclusiveMapping">list exclusive pair of fields/properties need to map value</param>
        /// <param name="ignoreFields">name of field/properties need to ignore mapping</param>
        /// <returns>target object</returns>
        public static T MapFromFieldsToFields<T>(this object source, T target,
            Dictionary<string, string> exclusiveMapping = null, List<string> ignoreFields = null) where T : class
        {
            var targetFields = typeof(T).GetFields();
            var sourceFields = source.GetType().GetFields();

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
                            e.FieldType.Name.Equals(targetField.FieldType.Name));

                        //check if field exists
                        if (sourceField != null)
                        {
                            targetField.SetValue(target, sourceField.GetValue(source));
                        }
                    }
                }

                //mapping based on name if no exclusive fields nor ignore fields are specified

                foreach (var sourceField in sourceFields)
                {
                    if (sourceField.Name.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase) &&
                        sourceField.FieldType.Name.Equals(targetField.FieldType.Name))
                    {
                        targetField.SetValue(target, sourceField.GetValue(source));
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// mapping value from a object to existing object based on source property name & target field name
        /// </summary>
        /// <typeparam name="T">type of target object</typeparam>
        /// <param name="source">source object</param>
        /// <param name="target">target object</param>
        /// <param name="exclusiveMapping">list exclusive pair of fields/properties need to map value</param>
        /// <param name="ignoreFields">name of field/properties need to ignore mapping</param>
        /// <returns>target object</returns>
        public static T MapFromPropertiesToFields<T>(this object source, T target,
            Dictionary<string, string> exclusiveMapping = null, List<string> ignoreFields = null) where T : class
        {
            var targetFields = typeof(T).GetFields();
            var sourceFields = source.GetType().GetProperties();

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
                            e.PropertyType.Name.Equals(targetField.FieldType.Name));

                        //check if field exists
                        if (sourceField != null)
                        {
                            targetField.SetValue(target, sourceField.GetValue(source));
                        }
                    }
                }

                //mapping based on name if no exclusive fields nor ignore fields are specified

                foreach (var sourceField in sourceFields)
                {
                    if (sourceField.Name.Equals(targetField.Name, StringComparison.OrdinalIgnoreCase) &&
                        sourceField.PropertyType.Name.Equals(targetField.FieldType.Name))
                    {
                        targetField.SetValue(target, sourceField.GetValue(source));
                    }
                }
            }

            return target;
        }
    }
}
