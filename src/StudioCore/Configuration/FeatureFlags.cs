namespace StudioCore.Settings;

/// <summary>
///     Class of feature flags, which can be used to enable/disable experimental
///     or in development features. Flags should generally be removed as features mature.
/// </summary>
public static class FeatureFlags
{
    public static bool EnableResourceLogs = true;

    // Feature Toggles
    public static bool EnableNavmeshBuilder = false;
}
