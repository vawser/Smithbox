namespace StudioCore.Application;

/// <summary>
///     Class of feature flags, which can be used to enable/disable experimental
///     or in development features. Flags should generally be removed as features mature.
/// </summary>
public static class FeatureFlags
{
    // WIP Editors
    public static bool EnableAnimEditor = false;
    public static bool EnableMapDataEditor = true;


}
