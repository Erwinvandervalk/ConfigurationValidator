using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConfigurationValidation
{
    public static class ConfigLoggerExtensions
    {
        public static void LogConfig(this ILogger logger, IConfiguration config, object configurationSettings)
        {
            var printedConfig = ConfigPrinter.PrintConfig(config, configurationSettings);

            // Add the settings (and optionally the errors) as metadata to the logmessage
            // not part of the actual logmessage itself. Otherwise, the logmessage
            // explodes in your logviewer. Now it's a one liner, that, when you open it up, it shows the information
            var logScope = new Dictionary<string, object>() { { "configsettings", printedConfig }, };

            if (!ConfigurationValidator.TryValidate(configurationSettings, out var errors))
            {
                var errorsString = string.Join(System.Environment.NewLine, errors.Select(x => x.ToString()));
                logScope.Add("errors", errorsString);
                using (logger.BeginScope(logScope))
                {
                    logger.LogError("Configuration errors detected");
                }
            }
            else
            {
                using (logger.BeginScope(logScope))
                {
                    logger.LogInformation("Configuration loaded");
                }
            }
        }
    }
}