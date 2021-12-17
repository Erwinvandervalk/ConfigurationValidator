using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ConfigurationValidation
{
    public class ConfigurationValidator
    {

        /// <summary>
        /// Use this method to make sure all properties in your configuration either have a validation
        /// attribute on them OR that they have an explicit [NotConfigured] attribute on them. 
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static bool AreAllPropertiesValidated(object subject, out string[] errors)
        {
            var props = ReflectionHelper.GetProperties(subject.GetType(), subject);

            var propsWithErrors = props.Where(x =>
                !x.PropertyInfo.GetCustomAttributes<ValidationAttribute>().Any() && !x.NoValidationRequired);

            errors = propsWithErrors.Select(x => x.Name).ToArray();

            return !errors.Any();
        }

        internal static bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results) =>
            TryValidateObjectRecursive(obj, results, new HashSet<object>());

        public static bool TryValidate<T>(T subject, out IReadOnlyCollection<ConfigurationError> errorsResult)
        {
            var validationResult = new List<ValidationResult>();
            var isValid = TryValidateObjectRecursive(subject, validationResult);

            var errors = new List<ConfigurationError>();

            foreach (var error in validationResult)
            {
                errors.Add(new ConfigurationError()
                {
                    ErrorMessage = error.ErrorMessage,
                    Property = error.MemberNames?.FirstOrDefault(),
                });
            }

            errorsResult = errors;
            return isValid;
        }

        private static bool TryValidateObjectRecursive<T>(
            T obj,
            List<ValidationResult> results,
            ISet<object> validatedObjects)
        {
            if (validatedObjects.Contains(obj))
            {
                return true;
            }

            validatedObjects.Add(obj);
            var result = Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);

            var properties = obj.GetType().GetProperties()
                .Where(prop => prop.CanRead && prop.GetIndexParameters().Length == 0)
                .OrderBy(x => x.Name)
                .ToList();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string) || property.PropertyType.IsValueType)
                {
                    continue;
                }

                var propertyInfo = obj.GetType().GetProperty(property.Name);
                var value = propertyInfo != null ? propertyInfo.GetValue(obj, null) : string.Empty;

                if (value == null)
                {
                    if (property.GetCustomAttribute<NotConfiguredAttribute>() == null)
                    {
                        results.Add(new ValidationResult($"{property.Name} is null", new string[] { property.Name }));
                    }

                    continue;
                }

                if (value is IEnumerable asEnumerable)
                {
                    foreach (var enumObj in asEnumerable)
                    {
                        if (enumObj != null)
                        {
                            var nestedResults = new List<ValidationResult>();
                            if (!TryValidateObjectRecursive(enumObj, nestedResults, validatedObjects))
                            {
                                foreach (var validationResult in nestedResults)
                                {
                                    var property1 = property;
                                    results.Add(new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(x => property1.Name + ':' + x)));
                                }
                            };
                        }
                        else
                        {
                            results.Add(new ValidationResult($"{property.Name} is null", new string[] { property.Name }));
                        }
                    }
                }
                else
                {
                    var nestedResults = new List<ValidationResult>();
                    if (!TryValidateObjectRecursive(value, nestedResults, validatedObjects))
                    {
                        foreach (var validationResult in nestedResults)
                        {
                            var property1 = property;
                            results.Add(new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(x => property1.Name + ':' + x)));
                        }
                    };
                }
            }

            return !results.Any();
        }
    }
}