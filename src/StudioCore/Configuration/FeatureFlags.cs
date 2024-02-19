namespace StudioCore.Settings;

/// <summary>
///     Class of feature flags, which can be used to enable/disable experimental
///     or in development features. Flags should generally be removed as features mature.
/// </summary>
public static class FeatureFlags
{
    public static bool DebugMenu = true;
    public static bool TestMenu = false;

    public static bool StrictResourceChecking = true;

    // Feature Toggles
    public static bool EnableNavmeshBuilder = false;
    public static bool EnablePartialParam = false;

    // MSB Toggles
    public static bool AC6_MSB = true;
    public static bool AC6_MSB_Saving = false;

    // Editor Toggles
    public static bool EnableEditor_TimeAct = false;
    public static bool EnableEditor_Cutscene = false;
    public static bool EnableEditor_Material = false;
    public static bool EnableEditor_Particle = false;
    public static bool EnableEditor_Gparam = true;
    public static bool EnableEditor_EventScript = false;
    public static bool EnableEditor_TalkScript = false;
    public static bool EnableEditor_TextureViewer = false;
    public static bool EnableEditor_BehaviorEditor = false;

    // This is temporary until the Pipeline mess is fixed.
    public static bool EnableCollisionPipeline = false;
}
