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

public enum Category
{
    Core,
    Window,
    MapEditor,
    ParamEditor,
    TextEditor,
    Viewport
}

public class KeyBind
{
    public bool Alt_Pressed;
    public bool Ctrl_Pressed;
    public Key PrimaryKey;
    public bool Shift_Pressed;

    public string PresentationName;
    public Category KeyCategory;

    [JsonConstructor]
    public KeyBind()
    {
    }

    public KeyBind(string name, Category category, Key primaryKey = Key.Unknown, bool ctrlKey = false, bool altKey = false, bool shiftKey = false)
    {
        PresentationName = name;
        KeyCategory = category;

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
        // Core
        public KeyBind Core_Delete = new("Delete", Category.Core, Key.Delete);
        public KeyBind Core_Duplicate = new("Duplicate", Category.Core, Key.D, true);
        public KeyBind Core_Redo = new("Redo", Category.Core, Key.Y, true);
        public KeyBind Core_SaveAllEditors = new("Save All", Category.Core);
        public KeyBind Core_SaveCurrentEditor = new("Save", Category.Core, Key.S, true);
        public KeyBind Core_Undo = new("Undo", Category.Core, Key.Z, true);

        // Windows
        public KeyBind Window_Help = new("Toggle Help Window", Category.Window, Key.F2);
        public KeyBind Window_FlagBrowser = new("Toggle Event Flag Window", Category.Window, Key.F3);

        // Map Toolbar
        public KeyBind Toolbar_Rotate_X = new("Rotate X", Category.MapEditor, Key.J);
        public KeyBind Toolbar_Rotate_Y = new("Rotate Y", Category.MapEditor, Key.K, false, false, true);
        public KeyBind Toolbar_Rotate_Y_Pivot = new("Rotate Y Pivot", Category.MapEditor, Key.K);
        public KeyBind Toolbar_Go_to_Selection_in_Object_List = new("Go to Selection in Object List", Category.MapEditor, Key.G);
        public KeyBind Toolbar_Move_Selection_to_Camera = new("Move Selection to Camera", Category.MapEditor, Key.X);
        public KeyBind Toolbar_Frame_Selection_in_Viewport = new("Frame Selection in Viewport", Category.MapEditor, Key.F);
        public KeyBind Toolbar_Toggle_Selection_Visibility_Flip = new("Toggle Selection Visibility: Flip", Category.MapEditor, Key.H, true);
        public KeyBind Toolbar_Toggle_Map_Visibility_Flip = new("Toggle Map Visibility: Flip", Category.MapEditor, Key.B, true);
        public KeyBind Toolbar_Toggle_Selection_Visibility_Enabled = new("Toggle Selection Visibility: Enable", Category.MapEditor, Key.H, true, true);
        public KeyBind Toolbar_Toggle_Map_Visibility_Enabled = new("Toggle Map Visibility: Enable", Category.MapEditor, Key.B, true, true);
        public KeyBind Toolbar_Toggle_Selection_Visibility_Disabled = new("Toggle Selection Visibility: Disable", Category.MapEditor, Key.H, true, true, true);
        public KeyBind Toolbar_Toggle_Map_Visibility_Disabled = new("Toggle Map Visibility: Disable", Category.MapEditor, Key.B, true, true, true);
        public KeyBind Toolbar_Reset_Rotation = new("Reset Rotation", Category.MapEditor, Key.L);
        public KeyBind Toolbar_Dummify = new("Dummify", Category.MapEditor, Key.Comma, false, false, true);
        public KeyBind Toolbar_Undummify = new("Undummify", Category.MapEditor, Key.Period, false, false, true);
        public KeyBind Toolbar_Scramble = new("Scramble", Category.MapEditor, Key.S, false, true);
        public KeyBind Toolbar_Replicate = new("Replicate", Category.MapEditor, Key.R, false, true);
        public KeyBind Toolbar_Set_to_Grid = new("Set to Grid", Category.MapEditor, Key.G, false, true);
        public KeyBind Toolbar_Create = new("Create", Category.MapEditor, Key.C, false, true);
        public KeyBind Toolbar_RenderEnemyPatrolRoutes = new("Render Patrol Routes", Category.MapEditor, Key.P, true);

        // Map
        public KeyBind Map_DuplicateToMap = new("Duplicate to Map", Category.MapEditor, Key.D, false, false, true);
        public KeyBind Map_PropSearch = new("Property Search", Category.MapEditor, Key.F, true);
        public KeyBind Map_RenderGroup_GetDisp = new("Render Group: Get Display Group", Category.MapEditor, Key.G, true);
        public KeyBind Map_RenderGroup_GetDraw = new("Render Group: Get Draw Group", Category.MapEditor);
        public KeyBind Map_RenderGroup_GiveDisp = new("Render Group: Give Display Group", Category.MapEditor);
        public KeyBind Map_RenderGroup_GiveDraw = new("Render Group: Give Draw Group", Category.MapEditor);
        public KeyBind Map_RenderGroup_HideAll = new("Render Group: Hide All", Category.MapEditor);
        public KeyBind Map_RenderGroup_ShowAll = new("Render Group: Show All", Category.MapEditor, Key.R, true);
        public KeyBind Map_RenderGroup_SelectHighlights = new("Render Group: Select Highlights", Category.MapEditor);
        public KeyBind Map_ViewportGrid_Lower = new("Map Grid: Lower", Category.MapEditor, Key.Q, true);
        public KeyBind Map_ViewportGrid_Raise = new("Map Grid: Raise", Category.MapEditor, Key.E, true);
        public KeyBind Map_ToggleRenderOutline = new("Toggle Selection Outline", Category.MapEditor);

        // Param
        public KeyBind Param_Copy = new("Copy", Category.ParamEditor, Key.C, true);
        public KeyBind Param_ExportCSV = new("Export CSV", Category.ParamEditor);
        public KeyBind Param_GotoBack = new("Go to Back", Category.ParamEditor, Key.Escape);
        public KeyBind Param_GotoRowID = new("Go to Row ID", Category.ParamEditor, Key.G, true);
        public KeyBind Param_GotoSelectedRow = new("Go to Selected Row", Category.ParamEditor, Key.G);
        public KeyBind Param_HotReload = new("Hot Reload", Category.ParamEditor, Key.F5);
        public KeyBind Param_HotReloadAll = new("Hot Reload All", Category.ParamEditor, Key.F5, false, false, true);
        public KeyBind Param_ImportCSV = new("Import CSV", Category.ParamEditor);
        public KeyBind Param_MassEdit = new("Mass Edit", Category.ParamEditor);
        public KeyBind Param_Paste = new("Paste", Category.ParamEditor, Key.V, true);
        public KeyBind Param_SearchField = new("Search Field", Category.ParamEditor, Key.N, true);
        public KeyBind Param_SearchParam = new("Search Param", Category.ParamEditor, Key.P, true);
        public KeyBind Param_SearchRow = new("Search Row", Category.ParamEditor, Key.F, true);
        public KeyBind Param_SelectAll = new("Select All", Category.ParamEditor, Key.A, true);

        // Text FMG
        public KeyBind TextFMG_ExportAll = new("Export All", Category.TextEditor);
        public KeyBind TextFMG_Import = new("Import", Category.TextEditor);
        public KeyBind TextFMG_Search = new("Search", Category.TextEditor, Key.F, true);

        // Viewport
        public KeyBind Viewport_Cam_Back = new("Back", Category.Viewport, Key.S);
        public KeyBind Viewport_Cam_Down = new("Down", Category.Viewport, Key.Q);
        public KeyBind Viewport_Cam_Forward = new("Forward", Category.Viewport, Key.W);
        public KeyBind Viewport_Cam_Left = new("Left", Category.Viewport, Key.A);
        public KeyBind Viewport_Cam_Reset = new("Reset", Category.Viewport, Key.R);
        public KeyBind Viewport_Cam_Right = new("Right", Category.Viewport, Key.D);
        public KeyBind Viewport_Cam_Up = new("Up", Category.Viewport, Key.E);
        public KeyBind Viewport_RotationMode = new("Gizmo Rotation Mode", Category.Viewport, Key.E);
        public KeyBind Viewport_ToggleGizmoOrigin = new("Toggle Gizmo Origin", Category.Viewport, Key.Home);
        public KeyBind Viewport_ToggleGizmoSpace = new("Toggle Gizmo Space", Category.Viewport);
        public KeyBind Viewport_TranslateMode = new("Gizmo Translate Mode", Category.Viewport, Key.W);

#pragma warning disable IDE0051
        // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
        [JsonExtensionData] internal IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051
    }
}
