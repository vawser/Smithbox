using StudioCore.Platform;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;
using StudioCore.Settings;

namespace StudioCore;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(CFG))]
internal partial class CfgSerializerContext : JsonSerializerContext
{
}

public class CFG
{
    public const string FolderName = "Smithbox";
    public const string Config_FileName = "Smithbox_Config.json";
    public const string Keybinds_FileName = "Smithbox_Keybinds.json";

    public const int MAX_RECENT_PROJECTS = 20;
    public static bool IsEnabled = true;

    private static readonly object _lock_SaveLoadCFG = new();

    //private string _Param_Export_Array_Delimiter = "|";
    private string _Param_Export_Delimiter = ",";

    // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
#pragma warning disable IDE0051
    [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData;
#pragma warning restore IDE0051

    // Settings: System
    public bool EnableCheckProgramUpdate = true;
    public bool ShowUITooltips = true;
    public float UIScale = 1.0f;
    public bool EnableSoapstone = true;

    public bool FontChinese = false;
    public bool FontCyrillic = false;
    public bool FontKorean = false;
    public bool FontThai = false;
    public bool FontVietnamese = false;

    // Settings: Map Editor
    public bool EnableFrustrumCulling = true;
    public bool EnableTexturing = false;
    public bool Map_AlwaysListLoadedMaps = true;
    public bool EnableEldenRingAutoMapOffset = true;

    public bool SceneView_ShowCharacterNames = true;

    public bool Map_EnableViewportGrid = true;
    public int Map_ViewportGridType = 0;
    public Vector3 GFX_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
    public int Map_ViewportGrid_TotalSize = 1000;
    public int Map_ViewportGrid_IncrementSize = 10;
    public float Map_ViewportGrid_Offset = 0;
    public float Map_ViewportGrid_ShortcutIncrement = 1;

    public bool Map_EnableSelectionOutline = true;

    public float Map_MoveSelectionToCamera_Radius = 3.0f;
    public float GFX_Camera_FOV { get; set; } = 60.0f;
    public float GFX_Camera_MoveSpeed_Slow { get; set; } = 1.0f;
    public float GFX_Camera_MoveSpeed_Normal { get; set; } = 20.0f;
    public float GFX_Camera_MoveSpeed_Fast { get; set; } = 200.0f;
    public float GFX_Camera_Sensitivity { get; set; } = 0.0160f;
    public float GFX_RenderDistance_Max { get; set; } = 50000.0f;
    public float Map_ArbitraryRotation_X_Shift { get; set; } = 90.0f;
    public float Map_ArbitraryRotation_Y_Shift { get; set; } = 90.0f;

    public uint GFX_Limit_Buffer_Flver_Bone = 65536;
    public uint GFX_Limit_Buffer_Indirect_Draw = 50000;
    public int GFX_Limit_Renderables = 50000;

    public float GFX_Wireframe_Color_Variance = 0.11f;

    public Vector3 GFX_Renderable_Box_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_Box_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_Cylinder_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_Cylinder_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_Sphere_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_Sphere_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_Point_BaseColor = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 GFX_Renderable_Point_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_DummyPoly_BaseColor = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 GFX_Renderable_DummyPoly_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_BonePoint_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_BonePoint_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_ModelMarker_Chr_BaseColor = Utils.GetDecimalColor(Color.Firebrick);
    public Vector3 GFX_Renderable_ModelMarker_Chr_HighlightColor = Utils.GetDecimalColor(Color.Tomato);

    public Vector3 GFX_Renderable_ModelMarker_Object_BaseColor = Utils.GetDecimalColor(Color.MediumVioletRed);
    public Vector3 GFX_Renderable_ModelMarker_Object_HighlightColor = Utils.GetDecimalColor(Color.DeepPink);

    public Vector3 GFX_Renderable_ModelMarker_Player_BaseColor = Utils.GetDecimalColor(Color.DarkOliveGreen);
    public Vector3 GFX_Renderable_ModelMarker_Player_HighlightColor = Utils.GetDecimalColor(Color.OliveDrab);

    public Vector3 GFX_Renderable_ModelMarker_Other_BaseColor = Utils.GetDecimalColor(Color.Wheat);
    public Vector3 GFX_Renderable_ModelMarker_Other_HighlightColor = Utils.GetDecimalColor(Color.AntiqueWhite);

    public Vector3 GFX_Renderable_PointLight_BaseColor = Utils.GetDecimalColor(Color.YellowGreen);
    public Vector3 GFX_Renderable_PointLight_HighlightColor = Utils.GetDecimalColor(Color.Yellow);

    public Vector3 GFX_Renderable_SpotLight_BaseColor = Utils.GetDecimalColor(Color.Goldenrod);
    public Vector3 GFX_Renderable_SpotLight_HighlightColor = Utils.GetDecimalColor(Color.Violet);

    public Vector3 GFX_Renderable_DirectionalLight_BaseColor = Utils.GetDecimalColor(Color.Cyan);
    public Vector3 GFX_Renderable_DirectionalLight_HighlightColor = Utils.GetDecimalColor(Color.AliceBlue);

    public Vector3 GFX_Gizmo_X_BaseColor = new(0.952f, 0.211f, 0.325f);
    public Vector3 GFX_Gizmo_X_HighlightColor = new(1.0f, 0.4f, 0.513f);

    public Vector3 GFX_Gizmo_Y_BaseColor = new(0.525f, 0.784f, 0.082f);
    public Vector3 GFX_Gizmo_Y_HighlightColor = new(0.713f, 0.972f, 0.270f);

    public Vector3 GFX_Gizmo_Z_BaseColor = new(0.219f, 0.564f, 0.929f);
    public Vector3 GFX_Gizmo_Z_HighlightColor = new(0.407f, 0.690f, 1.0f);

    public RenderFilter LastSceneFilter { get; set; } = RenderFilter.All ^ RenderFilter.Light;

    public RenderFilterPreset SceneFilter_Preset_01 { get; set; } = new("Map",
        RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);

    public RenderFilterPreset SceneFilter_Preset_02 { get; set; } = new("Collision",
        RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);

    public RenderFilterPreset SceneFilter_Preset_03 { get; set; } = new("Collision & Navmesh",
        RenderFilter.Collision | RenderFilter.Navmesh | RenderFilter.Object | RenderFilter.Character |
        RenderFilter.Region);

    public RenderFilterPreset SceneFilter_Preset_04 { get; set; } = new("Lighting (Map)",
        RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.Light);

    public RenderFilterPreset SceneFilter_Preset_05 { get; set; } = new("Lighting (Collision)",
        RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.Light);

    public RenderFilterPreset SceneFilter_Preset_06 { get; set; } = new("All", RenderFilter.All);

    // Settings: Model Editor

    // Settings: Param Editor
    public bool Param_MasseditPopupInContextMenu = true;
    public bool Param_AdvancedMassedit = false;
    public bool Param_AllowFieldReorder = true;
    public bool Param_AlphabeticalParams = true;
    public bool Param_DisableLineWrapping = false;
    public bool Param_DisableRowGrouping = false;
    public bool Param_HideEnums = false;
    public bool Param_HideReferenceRows = false;
    public bool Param_MakeMetaNamesPrimary = true;
    public bool Param_PasteAfterSelection = false;
    public bool Param_ShowFieldOffsets = false;
    public bool Param_ShowHotkeysInContextMenu = true;
    public bool Param_ShowSecondaryNames = true;
    public bool Param_ShowVanillaParams = true;
    public bool Param_FieldNameInContextMenu = true;
    public bool Param_FieldDescriptionInContextMenu = true;
    public bool Param_SplitContextMenu = false;
    public bool UI_CompactParams = false;

    // Settings: Text Editor
    public bool FMG_NoFmgPatching = false;
    public bool FMG_NoGroupedFmgEntries = false;
    public bool FMG_ShowOriginalNames = false;

    // Settings: Browsers
    public bool AssetBrowser_ShowTagsInBrowser = true;
    public bool AssetBrowser_UpdateSelectionName = true;
    public bool AssetBrowser_SuspendListWhenInViewport = true;

    public bool EventFlagBrowser_Open = false;
    public bool EventFlagBrowser_ShowTagsInBrowser = true;
    public bool EventFlagBrowser_ShowAliasAddition = false;

    public bool ParticleBrowser_Open = false;
    public bool ParticleBrowser_ShowTagsInBrowser = true;
    public bool ParticleBrowser_ShowAliasAddition = false;

    public bool MapAliases_ShowMapAliasEditList = false;
    public bool MapAliases_ShowUnusedNames = false;
    public bool MapAliases_ShowTagsInBrowser = true;
    public bool MapAliases_ShowAliasAddition = false;

    // CFG
    public static CFG Current { get; private set; }
    public static CFG Default { get; } = new();

    public string LastProjectFile { get; set; } = "";
    public List<RecentProject> RecentProjects { get; set; } = new();

    public GameType Game_Type { get; set; } = GameType.Undefined;

    

    public int GFX_Display_Width { get; set; } = 1920;
    public int GFX_Display_Height { get; set; } = 1057;

    public int GFX_Display_X { get; set; } = 0;
    public int GFX_Display_Y { get; set; } = 23;

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

    public static string GetConfigFilePath()
    {
        return $@"{GetConfigFolderPath()}\{Config_FileName}";
    }

    public static string GetBindingsFilePath()
    {
        return $@"{GetConfigFolderPath()}\{Keybinds_FileName}";
    }

    public static string GetConfigFolderPath()
    {
        return $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{FolderName}";
    }

    private static void LoadConfig()
    {
        if (!File.Exists(GetConfigFilePath()))
        {
            Current = new CFG();
        }
        else
        {
            try
            {
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(File.ReadAllText(GetConfigFilePath()),
                    CfgSerializerContext.Default.CFG);
                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"{e.Message}\n\nConfig could not be loaded. Reset settings?",
                    $"{Config_FileName} Load Error", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    throw new Exception($"{Config_FileName} could not be loaded.\n\n{e.Message}");
                }

                Current = new CFG();
                SaveConfig();
            }
        }
    }

    private static void LoadKeybinds()
    {
        if (!File.Exists(GetBindingsFilePath()))
        {
            KeyBindings.Current = new KeyBindings.Bindings();
        }
        else
        {
            try
            {
                KeyBindings.Current = JsonSerializer.Deserialize(File.ReadAllText(GetBindingsFilePath()),
                    KeybindingsSerializerContext.Default.Bindings);
                if (KeyBindings.Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"{e.Message}\n\nKeybinds could not be loaded. Reset keybinds?",
                    $"{Keybinds_FileName} Load Error", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    throw new Exception($"{Keybinds_FileName} could not be loaded.\n\n{e.Message}");
                }

                KeyBindings.Current = new KeyBindings.Bindings();
                SaveKeybinds();
            }
        }
    }

    private static void SaveConfig()
    {
        var json = JsonSerializer.Serialize(
            Current, CfgSerializerContext.Default.CFG);
        File.WriteAllText(GetConfigFilePath(), json);
    }

    private static void SaveKeybinds()
    {
        var json = JsonSerializer.Serialize(
            KeyBindings.Current, KeybindingsSerializerContext.Default.Bindings);
        File.WriteAllText(GetBindingsFilePath(), json);
    }

    public static void Save()
    {
        if (IsEnabled)
        {
            lock (_lock_SaveLoadCFG)
            {
                if (!Directory.Exists(GetConfigFolderPath()))
                {
                    Directory.CreateDirectory(GetConfigFolderPath());
                }

                SaveConfig();
                SaveKeybinds();
            }
        }
    }

    public static void AttemptLoadOrDefault()
    {
        if (IsEnabled)
        {
            lock (_lock_SaveLoadCFG)
            {
                if (!Directory.Exists(GetConfigFolderPath()))
                {
                    Directory.CreateDirectory(GetConfigFolderPath());
                }

                LoadConfig();
                LoadKeybinds();
            }
        }
    }

    /// <summary>
    /// Inserts a RecentProject to the top of the list of recent projects.
    /// Updates LastProjectFile and removes any project dupes in the list.
    /// </summary>
    public static void AddMostRecentProject(RecentProject proj)
    {
        foreach (var otherProj in Current.RecentProjects.ToArray())
        {
            if (proj.IsSameProjectLocation(otherProj))
            {
                Current.RecentProjects.Remove(otherProj);
            }
        }

        Current.RecentProjects.Insert(0, proj);

        if (Current.RecentProjects.Count > MAX_RECENT_PROJECTS)
        {
            Current.RecentProjects.RemoveAt(Current.RecentProjects.Count - 1);
        }

        Current.LastProjectFile = proj.ProjectFile;

        CFG.Save();
    }

    /// <summary>
    /// Removes a RecentProject from the list of recent projects.
    /// Also removes any dupes.
    /// </summary>
    public static void RemoveRecentProject(RecentProject proj)
    {
        foreach (var otherProj in Current.RecentProjects.ToArray())
        {
            if (proj.IsSameProjectLocation(otherProj))
            {
                Current.RecentProjects.Remove(otherProj);
            }
        }

        CFG.Save();
    }

    public class RecentProject
    {
        // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
#pragma warning disable IDE0051
        [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051

        public string Name { get; set; }
        public string ProjectFile { get; set; }
        public GameType GameType { get; set; }

        public bool IsSameProjectLocation(RecentProject otherProject)
        {
            if (ProjectFile == otherProject.ProjectFile)
            {
                return true;
            }
            return false;
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
}
