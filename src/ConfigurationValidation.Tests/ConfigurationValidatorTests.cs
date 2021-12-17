using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shouldly;
using Xunit;

namespace ConfigurationValidation.Tests;

public class ConfigurationValidatorTests
{
    [Fact]
    public void Given_valid_settings_when_validating_then_IsValid()
    {
        var settings = new ExampleSettings()
        {
            ExampleValue = "value",
            Sub = new SubSettings()
            {
                SubValue = "value"
            }
        };

        ConfigurationValidator.TryValidate(settings, out var errors)
            .ShouldBe(true);
    }

    [Fact]
    public void Can_detect_sub_settings_being_null()
    {
        var settings = new ExampleSettings()
        {
            ExampleValue = "value"
        };

        ConfigurationValidator.TryValidate(settings, out var errors)
            .ShouldBe(false);

        errors.ShouldBeEquivalentTo(new List<ConfigurationError>
        {
            new ConfigurationError()
            {
                ErrorMessage = "Sub is null",
                Property = "Sub"
            }
        });
    }

    [Fact]
    public void Given_invalid_settings_when_validating_then_IsValid()
    {
        var settings = new ExampleSettings()
        {
            Sub = new SubSettings()
        };

        ConfigurationValidator.TryValidate(settings, out var errors)
            .ShouldBe(false);

        errors.ShouldBeEquivalentTo(new List<ConfigurationError>
        {
            new ()
            {
                ErrorMessage = "The ExampleValue field is required.",
                Property = nameof(ExampleSettings.ExampleValue)
            },
            new ConfigurationError()
            {
                ErrorMessage = "The SubValue field is required.",
                Property = "Sub:SubValue"
            }
        });
    }

    [Fact]
    public void Can_see_if_all_settings_are_validated()
    {
        ConfigurationValidator.AreAllPropertiesValidated(new ExampleSettings(), out var errors)
            .ShouldBeTrue(string.Join(',', errors));
    }

    [Fact]
    public void Will_fail_if_unvalidated_attributes_found()
    {
        ConfigurationValidator.AreAllPropertiesValidated(new SettingsWithMissingValidationAttribute(), out var errors)
            .ShouldBeFalse(string.Join(',', errors));

        errors.ShouldBe(new []
        {
            nameof(SettingsWithMissingValidationAttribute.AccidentallyNotValidated)
        });
    }

    public class SettingsWithMissingValidationAttribute
    {
        [Required]
        public string? ValidatedAttribute { get; set; }

        [NotConfiguredAttribute]
        public string? IntentionallyNotValidated { get; set; }

        public string? AccidentallyNotValidated { get; set; }
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