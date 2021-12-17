using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ConfigurationValidation
{
    public class ConfigPrinter
    {
        private readonly IConfiguration _configuration;
        public ConfigPrinter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string PrintConfig(object configurationSettings)
        {
            var builder = new StringBuilder();

            PrintProperties(_configuration, configurationSettings, builder);
            return builder.ToString();
        }

        public static string PrintConfig(IConfiguration configuration, object configurationSettings)
        {
            var builder = new StringBuilder();

            PrintProperties(configuration, configurationSettings, builder);
            return builder.ToString();
        }


        private static void PrintProperties<T>(
            IConfiguration configRoot,
            T subject,
            StringBuilder builder,
            string prefix = null)
        {

            // Verify if config is an iconfiguration root. 
            // aspnet core startup registers the configuration as an IConfiguration, 
            // not as the IConfigurationRoot as it actually is. So, the choice is 
            // to cast it everywhere when using it or to cast it here. I'm casting 
            // it here. 
            if (!(configRoot is IConfigurationRoot))
            {
                throw new ArgumentException("Config has to be an IConfigurationRoot", nameof(configRoot));
            }

            var type = subject.GetType();
            var properties = ReflectionHelper.GetProperties(type, subject).OrderBy(x => x.Name);

            var providers = ((IConfigurationRoot)configRoot).GetProvidersByKey();

            foreach (var property in properties)
            {
                var propertyName = prefix + property.Name;
                var definedIn = GetProvidersThatDefineThisProperty(providers, propertyName);

                var printableValue = property.Value?.ToString();
                if (string.IsNullOrEmpty(printableValue))
                {
                    printableValue = "<missing>";
                }
                else if (property.Redacted)
                {
                    printableValue = $"<redacted #{GetHash(property)}>";
                }
                else printableValue = "\"" + printableValue + "\"";

                builder.AppendLine($" - {propertyName} = {printableValue}{definedIn}");
            }
        }


        /// <summary>
        /// Calculates a deterministic number from the input to allow a visual
        /// inspection of the result to see if the value has changed
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string GetHash(SettingsProperty input)
        {
            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input.Value.ToString())));
        }

        private static string GetProvidersThatDefineThisProperty(IDictionary<string, IConfigurationProvider[]> providers, string property)
        {
            string definedIn = "";
            if (providers.TryGetValue(property, out var values))
            {
                var providerName = string.Join(", ", values.Select(x => x.GetType().Name));
                definedIn = $" - from ({providerName})";
            }

            return definedIn;
        }
    }
}