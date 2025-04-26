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

    // Editor Toggles
    public static bool EnableEditor_Cutscene = false;
    public static bool EnableEditor_Material = false;
    public static bool EnableEditor_Particle = false;
    public static bool EnableEditor_Evemd = true;
    public static bool EnableEditor_Esd = true;
    public static bool EnableEditor_HavokBehavior = false;


    /// <summary>
    /// NEW Feature flags
    /// </summary>
    public static bool EnableFileBrowser = true;
    public static bool EnableMapEditor = true;
    public static bool EnableModelEditor = true;
    public static bool EnableParamEditor = true;
    public static bool EnableTextEditor = true;
    public static bool EnableCutsceneEditor = true;
    public static bool EnableEventScriptEditor = true;
    public static bool EnableEzStateEditor = true;
    public static bool EnableGparamEditor = true;
    public static bool EnableMaterialEditor = true;
    public static bool EnableBehaviorEditor = true;
    public static bool EnableTextureEditor = true;
    public static bool EnableTimeActEditor = true;
}
