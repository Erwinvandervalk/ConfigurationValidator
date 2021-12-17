using System;

namespace ConfigurationValidation
{
    public class RedactedAttribute : Attribute
    {
        public static readonly string[] LikelyRedactedKeywords = new[]
        {
            "secret",
            "token",
            "password",
            "connectionstring"
        };

    }
}