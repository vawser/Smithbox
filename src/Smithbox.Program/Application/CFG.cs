using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace StudioCore.Application;

public class CFG
{
    // Types:
    // Preferences: shown in the Preferences menu
    // Options: exposes via ImGui elements such as checkboxes or combo-lists
    // Internal: used for persistent state

    ///------------------------------------------------------------
    /// Sections: groups of settings in their own classes, saved in the same flat file (see ICfgSection)
    ///------------------------------------------------------------
    [JsonIgnore]
    public readonly GraphicsSettings Graphics = new();

    [JsonIgnore]
    public readonly ViewportSettings Viewport = new();

    [JsonIgnore]
    public readonly MapEditorSettings MapEditor = new();

    [JsonIgnore]
    public readonly ParamEditorSettings ParamEditor = new();

    [JsonIgnore]
    public readonly InterfaceSettings Interface = new();

    [JsonIgnore]
    public readonly TextEditorSettings TextEditor = new();

    [JsonIgnore]
    public readonly ModelEditorSettings ModelEditor = new();

    [JsonIgnore]
    public readonly TimeActEditorSettings TimeActEditor = new();

    [JsonIgnore]
    public readonly LuaEditorSettings LuaEditor = new();

    [JsonIgnore]
    public readonly FxEditorSettings FxEditor = new();

    [JsonIgnore]
    public readonly ProjectileEditorSettings ProjectileEditor = new();

    [JsonIgnore]
    public readonly AnimEditorSettings AnimEditor = new();

    [JsonIgnore]
    public readonly GparamEditorSettings GparamEditor = new();

    [JsonIgnore]
    public readonly CutsceneEditorSettings CutsceneEditor = new();

    [JsonIgnore]
    public readonly HavokEditorSettings HavokEditor = new();

    [JsonIgnore]
    public readonly MaterialEditorSettings MaterialEditor = new();

    [JsonIgnore]
    public readonly BehaviorEditorSettings BehaviorEditor = new();

    [JsonIgnore]
    public readonly EmevdEditorSettings EmevdEditor = new();

    [JsonIgnore]
    public readonly EsdEditorSettings EsdEditor = new();

    [JsonIgnore]
    public readonly SoundEditorSettings SoundEditor = new();

    [JsonIgnore]
    public readonly FaceGenEditorSettings FaceGenEditor = new();

    [JsonIgnore]
    public readonly FileBrowserSettings FileBrowser = new();

    [JsonIgnore]
    public readonly TextureViewerSettings TextureViewer = new();

    [JsonIgnore]
    public readonly ProjectSettings Project = new();

    [JsonIgnore]
    public readonly DeveloperSettings Developer = new();

    [JsonIgnore]
    public readonly ProgramSettings Program = new();

    /// <summary>The sections with how each is serialized, for Configuration.json</summary>
    private IEnumerable<(ICfgSection Section, JsonTypeInfo Info)> Sections()
    {
        yield return (Graphics, ProjectJsonSerializerContext.Default.GraphicsSettings);
        yield return (Viewport, ProjectJsonSerializerContext.Default.ViewportSettings);
        yield return (MapEditor, ProjectJsonSerializerContext.Default.MapEditorSettings);
        yield return (ParamEditor, ProjectJsonSerializerContext.Default.ParamEditorSettings);
        yield return (Interface, ProjectJsonSerializerContext.Default.InterfaceSettings);
        yield return (TextEditor, ProjectJsonSerializerContext.Default.TextEditorSettings);
        yield return (ModelEditor, ProjectJsonSerializerContext.Default.ModelEditorSettings);
        yield return (TimeActEditor, ProjectJsonSerializerContext.Default.TimeActEditorSettings);
        yield return (LuaEditor, ProjectJsonSerializerContext.Default.LuaEditorSettings);
        yield return (FxEditor, ProjectJsonSerializerContext.Default.FxEditorSettings);
        yield return (ProjectileEditor, ProjectJsonSerializerContext.Default.ProjectileEditorSettings);
        yield return (AnimEditor, ProjectJsonSerializerContext.Default.AnimEditorSettings);
        yield return (GparamEditor, ProjectJsonSerializerContext.Default.GparamEditorSettings);
        yield return (CutsceneEditor, ProjectJsonSerializerContext.Default.CutsceneEditorSettings);
        yield return (HavokEditor, ProjectJsonSerializerContext.Default.HavokEditorSettings);
        yield return (MaterialEditor, ProjectJsonSerializerContext.Default.MaterialEditorSettings);
        yield return (BehaviorEditor, ProjectJsonSerializerContext.Default.BehaviorEditorSettings);
        yield return (EmevdEditor, ProjectJsonSerializerContext.Default.EmevdEditorSettings);
        yield return (EsdEditor, ProjectJsonSerializerContext.Default.EsdEditorSettings);
        yield return (SoundEditor, ProjectJsonSerializerContext.Default.SoundEditorSettings);
        yield return (FaceGenEditor, ProjectJsonSerializerContext.Default.FaceGenEditorSettings);
        yield return (FileBrowser, ProjectJsonSerializerContext.Default.FileBrowserSettings);
        yield return (TextureViewer, ProjectJsonSerializerContext.Default.TextureViewerSettings);
        yield return (Project, ProjectJsonSerializerContext.Default.ProjectSettings);
        yield return (Developer, ProjectJsonSerializerContext.Default.DeveloperSettings);
        yield return (Program, ProjectJsonSerializerContext.Default.ProgramSettings);
    }



    #region Interface

    public const float MinRailIconScale = 0.5f;
    public const float MaxRailIconScale = 3.0f;










    #endregion

    #region Param Editor







































    // Misc
    public string Param_Export_Delimiter
    {
        get
        {
            if (_Param_Export_Delimiter.Length == 0)
            {
                _Param_Export_Delimiter = Default.Param_Export_Delimiter;
            }
            else if (_Param_Export_Delimiter == "|")
            {
                _Param_Export_Delimiter =
                    Default
                        .Param_Export_Delimiter; // Temporary measure to prevent conflicts with byte array delimiters. Will be removed later.
            }

            return _Param_Export_Delimiter;
        }
        set => _Param_Export_Delimiter = value;
    }
    private string _Param_Export_Delimiter = ",";



    #endregion


    // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
#pragma warning disable IDE0051
    [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData;
#pragma warning restore IDE0051

    ///------------------------------------------------------------
    /// CFG
    ///------------------------------------------------------------
    public static CFG Current { get; set; }
    public static CFG Default { get; } = new();

    public static void Setup()
    {
        Current = new CFG();
    }

    public const float MinUIScale = 0.5f;
    public const float MaxUIScale = 4.0f;
    public const float MinFontSize = 8.0f;
    public const float MaxFontSize = 32.0f;

    /// <summary>
    /// Keeps the UI scale and font size from the configuration file within the ranges the preferences allow.
    /// An edited or broken file could otherwise give a scale or font size of 0, which collapses the UI.
    /// </summary>
    private static void ClampInterfaceScale()
    {
        Current.Interface.Interface_UI_Scale = float.IsFinite(Current.Interface.Interface_UI_Scale)
            ? Math.Clamp(Current.Interface.Interface_UI_Scale, MinUIScale, MaxUIScale)
            : Default.Interface.Interface_UI_Scale;

        Current.Interface.Interface_Font_Size = float.IsFinite(Current.Interface.Interface_Font_Size)
            ? Math.Clamp(Current.Interface.Interface_Font_Size, MinFontSize, MaxFontSize)
            : Default.Interface.Interface_Font_Size;
    }

    public static void Load()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Configuration.json");

        if (!File.Exists(file))
        {
            Current = new CFG();
            TrySave();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                Current = FromJson(filestring);

                if (Current == null)
                {
                    throw new Exception($"{LOC.Get("SYS_CFG_Failed_To_Deserialize", file)}");
                }

                MigrateGridSettings(filestring);
                ClampInterfaceScale();
            }
            catch (Exception e)
            {
                Smithbox.LogError<CFG>($"{LOC.Get("SYS_CFG_Failed_To_Read", file)}", e);

                // The read can fail on a file that is only locked for a moment, so it's copied before the defaults
                // are saved over it
                ConfigFiles.KeepUnreadable(file, typeof(CFG));

                Current = new CFG();
                TrySave();
            }
        }
    }

    /// <summary>
    /// <see cref="Save"/>, logging a warning instead of throwing when the file can't be written.
    /// </summary>
    public static bool TrySave()
    {
        var file = Path.Combine(ProjectUtils.GetConfigurationFolder(), "Configuration.json");

        return ConfigFiles.TryWrite(file, ToJson(Current), typeof(CFG));
    }

    /// <summary>
    /// Moves the grid settings of older configs into the grid sets. A failure only loses the old grid settings.
    /// </summary>
    private static void MigrateGridSettings(string json)
    {
        try
        {
            // The one place CFG reaches an editor: the grid sets it migrates belong to the editors' sections
            StudioCore.Editors.Viewport.ViewportGridSettingsMigration.Migrate(Current, json);
        }
        catch (Exception e)
        {
            Smithbox.LogError<CFG>("Failed to migrate the viewport grid settings.", e);
        }
    }

    public static void Save()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Configuration.json");

        ConfigFiles.Write(file, ToJson(Current));
    }

    /// <summary>
    /// The configuration as Configuration.json holds it: CFG's own settings and every section's, in one flat object
    /// </summary>
    public static string ToJson(CFG config)
    {
        var context = ProjectJsonSerializerContext.Default;
        var root = JsonSerializer.SerializeToNode(config, context.CFG)!.AsObject();

        foreach (var (section, info) in config.Sections())
        {
            var members = JsonSerializer.SerializeToNode(section, info)!.AsObject();

            foreach (var (key, value) in members.ToList())
            {
                members.Remove(key);
                root[key] = value;
            }
        }

        return root.ToJsonString(context.Options);
    }

    /// <summary>
    /// A configuration read from Configuration.json's text, or null if the text holds none. CFG and each section read
    /// the same flat object, each taking its own keys.
    /// </summary>
    public static CFG FromJson(string json)
    {
        var config = JsonSerializer.Deserialize(json, ProjectJsonSerializerContext.Default.CFG);

        if (config == null)
            return null;

        foreach (var (section, info) in config.Sections())
        {
            // Read into the section in place: it is the same object for as long as the configuration is
            if (JsonSerializer.Deserialize(json, info) is { } loaded)
                CFGHelpers.CopyMembers(loaded, section);
        }

        return config;
    }
}

public class RenderFilterPreset
{
    [JsonConstructor]
    public RenderFilterPreset()
    {
    }

    public RenderFilterPreset(string name, RenderFilter filters)
    {
        Name = name;
        Filters = filters;
    }

    public string Name { get; set; }
    public RenderFilter Filters { get; set; }
}

public static class CFGHelpers
{
    public static void ResetCurrentToDefault()
    {
        if (CFG.Current == null)
        {
            CFG.Current = new CFG();
        }

        CopyMembers(CFG.Default, CFG.Current);
    }

    /// <summary>
    /// Copies the settings from one configuration (or section) to another of the same type. Sections are copied into,
    /// not replaced, so each configuration keeps its own section objects.
    /// </summary>
    public static void CopyMembers(object source, object target)
    {
        var type = source.GetType();

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (typeof(ICfgSection).IsAssignableFrom(field.FieldType))
            {
                CopyMembers(field.GetValue(source), field.GetValue(target));
                continue;
            }

            if (field.IsInitOnly)
                continue;

            var value = field.GetValue(source);
            field.SetValue(target, CloneIfNeeded(value));
        }

        foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            if (prop.GetIndexParameters().Length > 0)
                continue;

            var value = prop.GetValue(source);
            prop.SetValue(target, CloneIfNeeded(value));
        }
    }

    private static object CloneIfNeeded(object value)
    {
        if (value == null)
            return null;

        var type = value.GetType();
        if (type.IsValueType || value is string)
            return value;

        if (value is IList<string> stringList)
            return new List<string>(stringList);

        if (value is IList<int> intList)
            return new List<int>(intList);

        if (value is IList<float> floatList)
            return new List<float>(floatList);

        return value;
    }

    /// <summary>Resets one setting, on CFG or in one of its sections, by its name</summary>
    public static bool ResetMemberToDefault(string memberName)
    {
        if (CFG.Current == null)
            return false;

        return ResetMemberToDefault(CFG.Default, CFG.Current, memberName);
    }

    private static bool ResetMemberToDefault(object defaults, object target, string memberName)
    {
        var type = target.GetType();

        var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
        if (field != null && !field.IsInitOnly)
        {
            var defaultValue = field.GetValue(defaults);
            field.SetValue(target, CloneIfNeeded(defaultValue));
            return true;
        }

        var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public);
        if (prop != null && prop.CanRead && prop.CanWrite && prop.GetIndexParameters().Length == 0)
        {
            var defaultValue = prop.GetValue(defaults);
            prop.SetValue(target, CloneIfNeeded(defaultValue));
            return true;
        }

        // A setting in one of the sections
        foreach (var section in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                     .Where(f => typeof(ICfgSection).IsAssignableFrom(f.FieldType)))
        {
            if (ResetMemberToDefault(section.GetValue(defaults), section.GetValue(target), memberName))
                return true;
        }

        return false;
    }

}