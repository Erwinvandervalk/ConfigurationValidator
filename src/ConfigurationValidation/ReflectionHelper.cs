using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConfigurationValidation
{
    internal class ReflectionHelper
    {
        internal static List<SettingsProperty> GetProperties(Type type, object subject, string prefix = null)
        {
            var properties = type.GetProperties();
            var propertyNames = new List<SettingsProperty>();
            foreach (var property in properties)
            {
                object propertyValue = subject == null
                    ? null
                    : property.GetValue(subject);

                // Check if this property can be safely ignored
                if (property.GetCustomAttribute<NotConfiguredAttribute>() != null)
                {
                    continue;
                }

                bool redacted = property.GetCustomAttribute<RedactedAttribute>() != null;
                bool noValidationRequired = property.GetCustomAttribute<NotConfiguredAttribute>() != null;

                if (property.PropertyType == typeof(string) || property.PropertyType.IsValueType || property.PropertyType == typeof(Uri))
                {
                    propertyNames.Add(new SettingsProperty(prefix + property.Name, propertyValue, redacted, property, noValidationRequired));
                }
                else
                {
                    foreach (var subProperty in GetProperties(property.PropertyType, propertyValue, prefix + property.Name + ":"))
                    {
                        propertyNames.Add(subProperty);
                    }
                }
            }

            return propertyNames;
        }
    }
}