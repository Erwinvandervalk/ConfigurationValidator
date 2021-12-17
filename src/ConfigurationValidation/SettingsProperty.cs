using System.Reflection;

namespace ConfigurationValidation
{
    public class SettingsProperty
    {
        public readonly string Name;
        public readonly object Value;
        public readonly bool Redacted;
        public readonly PropertyInfo PropertyInfo;
        public readonly bool NoValidationRequired;

        public SettingsProperty(string name, object value, bool redacted, PropertyInfo propertyInfo, bool noValidationRequired)
        {
            this.Name = name;
            this.Value = value;
            this.Redacted = redacted;
            this.PropertyInfo = propertyInfo;
            this.NoValidationRequired = noValidationRequired;
        }
    }
}