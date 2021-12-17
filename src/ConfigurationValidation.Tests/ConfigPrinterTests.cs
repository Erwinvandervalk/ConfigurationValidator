using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VerifyXunit;
using Xunit;

namespace ConfigurationValidation.Tests;

[UsesVerify]
public class ConfigPrinterTests
{   
    [Fact]
    public async Task Can_print_settings()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { nameof(ExampleSettings.ExampleValue), "value" },
                { nameof(ExampleSettings.Sub) + ":" + nameof(SubSettings.SubValue), "value" },

            })
            .Build();

        var settings = config.Get<ExampleSettings>();

        var result = ConfigPrinter.PrintConfig(config, settings);
        await Verifier.Verify(result);
    }
    public class ExampleSettings
    {
        [Required]
        public string? ExampleValue { get; set; }

        public SubSettings? Sub { get; set; }
    }

    public class SubSettings
    {
        [Required]
        public string? SubValue { get; set; }
    }

}