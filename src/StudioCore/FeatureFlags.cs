namespace StudioCore;

/// <summary>
///     Class of feature flags, which can be used to enable/disable experimental
///     or in development features. Flags should generally be removed as features mature.
/// </summary>
public static class FeatureFlags
{
    public static bool LoadNavmeshes = true;
    public static bool LoadDS3Navmeshes = true;

    public static bool EnableNavmeshBuilder = false;

    public static bool StrictResourceChecking = true;

#if DEBUG
    public static bool TestMenu = true;
#else
        public static bool TestMenu = false;
#endif

    public static bool EnablePartialParam = false;

    public static bool AC6_MSB = false;
}
