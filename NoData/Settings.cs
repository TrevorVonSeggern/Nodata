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
            CopyFromTo(this, other);
        }

        public static Settings CreateFromOther(Settings other) => new Settings(other);
        public static void CopyFromTo(Settings left, Settings? right = null)
        {
            if (right is null)
                right = new Settings();

            left.MaxExpandDepth = right.MaxExpandDepth;
            left.AllowCount = right.AllowCount;
        }

        public uint? MaxExpandDepth { get; set; } = 3; // negative to have no max expand. 0 for root properties only (no navigation), and positive numbers for expand depth limits.
        public bool AllowCount { get; set; } = true;
    }

    public class SettingsForType<T> : Settings
    {
        public SettingsForType() : base(DefaultSettingsForType<T>.SettingsForType)
        {
        }

        public static SettingsForType<T> Create(Settings other)
        {
            var settings = new SettingsForType<T>();
            Settings.CopyFromTo(settings, other);
            return settings;
        }
    }

    public static class DefaultSettingsForType<T>
    {
        public static SettingsForType<T> SettingsForType { get; set; } = SettingsForType<T>.Create(DefaultSettings.Settings);
    }

    public static class DefaultSettings
    {
        public static Settings Settings { get; set; } = new Settings();
    }
}
