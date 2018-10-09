using System;
using System.Collections.Generic;
using System.Text;
using CodeTools;

namespace NoData
{
    // public enum FilterSecurityTypes
    // {
    //     AllowOnlyVisibleValues,
    //     AllowFilteringOnPropertiesThatAreNotDisplayed,
    //     AllowFilteringOnNonExplicitlyExpandedItems,
    // }

    public class Settings
    {
        public Settings()
        {
        }
        public Settings(Settings other)
        {
            if (other is null)
                other = new Settings();

            MaxExpandDepth = other.MaxExpandDepth;
        }
        public int MaxExpandDepth = -1; // negative to have no max expand. 0 for root properties only (no navigation), and positive numbers for expand depth limits.
    }

    public class SettingsForType<T> : Settings
    {
        public SettingsForType() : this(DefaultSettingsForType<T>.SettingsForType)
        {
        }
        public SettingsForType(Settings other) : base(other) { }
        public SettingsForType(SettingsForType<T> other) : base(other) { }
    }

    public class DefaultSettingsForType<T> : Settings
    {
        public static SettingsForType<T> SettingsForType { get; set; } = new SettingsForType<T>(DefaultSettings.Settings);
    }

    public static class DefaultSettings
    {
        public static Settings Settings { get; set; } = new Settings();
    }
}
