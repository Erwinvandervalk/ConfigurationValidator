using System;

namespace ConfigurationValidation
{
    /// <summary>
    /// Indicates that a certain property is not automatically configured
    /// and thus ignored by config validation
    /// </summary>
    public class NotConfiguredAttribute : Attribute
    {

    }
}