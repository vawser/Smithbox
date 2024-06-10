namespace StudioCore.Settings;

/// <summary>
///     Class of feature flags, which can be used to enable/disable experimental
///     or in development features. Flags should generally be removed as features mature.
/// </summary>
public static class FeatureFlags
{
    public static bool StrictResourceChecking = true;

    // Feature Toggles
    public static bool EnableNavmeshBuilder = false;

    // Editor Toggles
    public static bool EnableEditor_TimeAct = false;
    public static bool EnableEditor_Cutscene = false;
    public static bool EnableEditor_Material = false;
    public static bool EnableEditor_Particle = false;
    public static bool EnableEditor_Evemd = false;
    public static bool EnableEditor_Esd = false;
    public static bool EnableEditor_HavokBehavior = false;
    public static bool EnableEditor_MergeTool = false;

#if DEBUG
    public static bool DebugMenu = true;
#else
    public static bool DebugMenu = false;
#endif
}
