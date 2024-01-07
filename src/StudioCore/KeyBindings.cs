using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Veldrid;

namespace StudioCore;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(KeyBindings.Bindings))]
[JsonSerializable(typeof(KeyBind))]
internal partial class KeybindingsSerializerContext : JsonSerializerContext
{
}

public class KeyBind
{
    public bool Alt_Pressed;
    public bool Ctrl_Pressed;
    public Key PrimaryKey;
    public bool Shift_Pressed;

    [JsonConstructor]
    public KeyBind()
    {
    }

    public KeyBind(Key primaryKey = Key.Unknown, bool ctrlKey = false, bool altKey = false, bool shiftKey = false)
    {
        PrimaryKey = primaryKey;
        Ctrl_Pressed = ctrlKey;
        Alt_Pressed = altKey;
        Shift_Pressed = shiftKey;
    }

    [JsonIgnore]
    public string HintText
    {
        get
        {
            if (PrimaryKey == Key.Unknown)
            {
                return "";
            }

            var str = "";
            if (Ctrl_Pressed)
            {
                str += "Ctrl+";
            }

            if (Alt_Pressed)
            {
                str += "Alt+";
            }

            if (Shift_Pressed)
            {
                str += "Shift+";
            }

            str += PrimaryKey.ToString();
            return str;
        }
    }
}

public class KeyBindings
{
    public static Bindings Current { get; set; }
    //public static Bindings Default { get; set; } = new();

    public static void ResetKeyBinds()
    {
        Current = new Bindings();
    }

    public class Bindings
    {
        public KeyBind Core_Delete = new(Key.Delete);
        public KeyBind Core_Duplicate = new(Key.D, true);
        public KeyBind Core_HelpMenu = new(Key.F2);
        public KeyBind Core_Redo = new(Key.Y, true);
        public KeyBind Core_SaveAllEditors = new();

        // Core
        public KeyBind Core_SaveCurrentEditor = new(Key.S, true);
        public KeyBind Core_Undo = new(Key.Z, true);
        public KeyBind Map_ArbitraryRotation_Roll = new(Key.J);
        public KeyBind Map_ArbitraryRotation_Yaw = new(Key.K, false, false, true);
        public KeyBind Map_ArbitraryRotation_Yaw_Pivot = new(Key.K);
        public KeyBind Map_Dummify = new(Key.Comma, false, false, true);
        public KeyBind Map_DuplicateToMap = new(Key.D, false, false, true);
        public KeyBind Map_GotoSelectionInObjectList = new(Key.G);
        public KeyBind Map_HideToggle = new(Key.H, true);
        public KeyBind Map_MoveSelectionToCamera = new(Key.X);

        // Map
        public KeyBind Map_PropSearch = new(Key.F, true);
        public KeyBind Map_RenderGroup_GetDisp = new(Key.G, true);
        public KeyBind Map_RenderGroup_GetDraw = new();
        public KeyBind Map_RenderGroup_GiveDisp = new();
        public KeyBind Map_RenderGroup_GiveDraw = new();
        public KeyBind Map_RenderGroup_HideAll = new();
        public KeyBind Map_RenderGroup_ShowAll = new(Key.R, true);
        public KeyBind Map_RenderGroup_SelectHighlights = new();
        public KeyBind Map_ResetRotation = new(Key.L);
        public KeyBind Map_UnDummify = new(Key.Period, false, false, true);
        public KeyBind Map_UnhideAll = new(Key.H, false, true);
        public KeyBind Param_Copy = new(Key.C, true);
        public KeyBind Param_ExportCSV = new();
        public KeyBind Param_GotoBack = new(Key.Escape);
        public KeyBind Param_GotoRowID = new(Key.G, true);
        public KeyBind Param_GotoSelectedRow = new(Key.G);
        public KeyBind Param_HotReload = new(Key.F5);
        public KeyBind Param_HotReloadAll = new(Key.F5, false, false, true);
        public KeyBind Param_ImportCSV = new();
        public KeyBind Param_MassEdit = new();
        public KeyBind Param_Paste = new(Key.V, true);
        public KeyBind Param_SearchField = new(Key.N, true);
        public KeyBind Param_SearchParam = new(Key.P, true);
        public KeyBind Param_SearchRow = new(Key.F, true);

        // Parameters
        public KeyBind Param_SelectAll = new(Key.A, true);
        public KeyBind TextFMG_ExportAll = new();
        public KeyBind TextFMG_Import = new();

        // Text Editor
        public KeyBind TextFMG_Search = new(Key.F, true);
        public KeyBind Viewport_Cam_Back = new(Key.S);
        public KeyBind Viewport_Cam_Down = new(Key.Q);

        // Viewport (Map & Model)
        public KeyBind Viewport_Cam_Forward = new(Key.W);
        public KeyBind Viewport_Cam_Left = new(Key.A);
        public KeyBind Viewport_Cam_Reset = new(Key.R);
        public KeyBind Viewport_Cam_Right = new(Key.D);
        public KeyBind Viewport_Cam_Up = new(Key.E);
        public KeyBind Viewport_FrameSelection = new(Key.F);
        public KeyBind Viewport_RotationMode = new(Key.E);
        public KeyBind Viewport_ToggleGizmoOrigin = new(Key.Home);
        public KeyBind Viewport_ToggleGizmoSpace = new();
        public KeyBind Viewport_TranslateMode = new(Key.W);
#pragma warning disable IDE0051
        // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
        [JsonExtensionData] internal IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051
    }
}
