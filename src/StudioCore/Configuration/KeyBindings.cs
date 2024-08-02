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

public enum KeybindCategory
{
    Core,
    Window,
    MapEditor,
    ModelEditor,
    ParamEditor,
    TextEditor,
    TimeActEditor,
    Viewport,
    TextureViewer
}

public class KeyBind
{
    public bool Alt_Pressed;
    public bool Ctrl_Pressed;
    public Key PrimaryKey;
    public bool Shift_Pressed;
    public bool FixedKey;

    public string PresentationName;
    public KeybindCategory KeyCategory;

    [JsonConstructor]
    public KeyBind()
    {
        PresentationName = "";
    }

    public KeyBind(string name, KeybindCategory category, Key primaryKey = Key.Unknown, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool fixedKey = false)
    {
        PresentationName = name;
        KeyCategory = category;

        PrimaryKey = primaryKey;
        Ctrl_Pressed = ctrlKey;
        Alt_Pressed = altKey;
        Shift_Pressed = shiftKey;
        FixedKey = fixedKey;
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
        public KeyBind Core_Create = new("Create", KeybindCategory.Core, Key.Insert);
        public KeyBind Core_Delete = new("Delete", KeybindCategory.Core, Key.Delete);
        public KeyBind Core_Duplicate = new("Duplicate", KeybindCategory.Core, Key.D, true);
        public KeyBind Core_Redo = new("Redo", KeybindCategory.Core, Key.Y, true);
        public KeyBind Core_SaveAllCurrentEditor = new("Save All", KeybindCategory.Core);
        public KeyBind Core_SaveCurrentEditor = new("Save", KeybindCategory.Core, Key.S, true);
        public KeyBind Core_Undo = new("Undo", KeybindCategory.Core, Key.Z, true);

        // Windows
        public KeyBind ToggleWindow_Project = new("Toggle Project Window", KeybindCategory.Window, Key.F1);
        public KeyBind ToggleWindow_Help = new("Toggle Help Window", KeybindCategory.Window, Key.F2);
        public KeyBind ToggleWindow_Keybind = new("Toggle Keybind Window", KeybindCategory.Window, Key.F3);
        public KeyBind ToggleWindow_Memory = new("Toggle Memory Window", KeybindCategory.Window, Key.F4);
        public KeyBind ToggleWindow_Settings = new("Toggle Settings Window", KeybindCategory.Window, Key.F6);
        public KeyBind ToggleWindow_Alias = new("Toggle Alias Window", KeybindCategory.Window, Key.F7);
        public KeyBind ToggleWindow_QuickTools = new("Toggle Color Picker Window", KeybindCategory.Window, Key.F8);
        public KeyBind ToggleWindow_Debug = new("Toggle Debug Window", KeybindCategory.Window, Key.F9);

        // Map Toolbar
        public KeyBind Toolbar_Rotate_X = new("Rotate X", KeybindCategory.MapEditor, Key.J);
        public KeyBind Toolbar_Rotate_Y = new("Rotate Y", KeybindCategory.MapEditor, Key.K, false, false, true);
        public KeyBind Toolbar_Rotate_Y_Pivot = new("Rotate Y Pivot", KeybindCategory.MapEditor, Key.K);
        public KeyBind Toolbar_Go_to_Selection_in_Object_List = new("Go to Selection in Object List", KeybindCategory.MapEditor, Key.G);
        public KeyBind Toolbar_Move_Selection_to_Camera = new("Move Selection to Camera", KeybindCategory.MapEditor, Key.X);
        public KeyBind Toolbar_Frame_Selection_in_Viewport = new("Frame Selection in Viewport", KeybindCategory.MapEditor, Key.F);
        public KeyBind Toolbar_Toggle_Selection_Visibility_Flip = new("Toggle Selection Visibility: Flip", KeybindCategory.MapEditor, Key.H, true);
        public KeyBind Toolbar_Toggle_Map_Visibility_Flip = new("Toggle Map Visibility: Flip", KeybindCategory.MapEditor, Key.B, true);
        public KeyBind Toolbar_Toggle_Selection_Visibility_Enabled = new("Toggle Selection Visibility: Enable", KeybindCategory.MapEditor, Key.H, true, true);
        public KeyBind Toolbar_Toggle_Map_Visibility_Enabled = new("Toggle Map Visibility: Enable", KeybindCategory.MapEditor, Key.B, true, true);
        public KeyBind Toolbar_Toggle_Selection_Visibility_Disabled = new("Toggle Selection Visibility: Disable", KeybindCategory.MapEditor, Key.H, true, true, true);
        public KeyBind Toolbar_Toggle_Map_Visibility_Disabled = new("Toggle Map Visibility: Disable", KeybindCategory.MapEditor, Key.B, true, true, true);
        public KeyBind Toolbar_Reset_Rotation = new("Reset Rotation", KeybindCategory.MapEditor, Key.L);
        public KeyBind Toolbar_Dummify = new("Dummify", KeybindCategory.MapEditor, Key.Comma, false, false, true);
        public KeyBind Toolbar_Undummify = new("Undummify", KeybindCategory.MapEditor, Key.Period, false, false, true);
        public KeyBind Toolbar_Scramble = new("Scramble", KeybindCategory.MapEditor, Key.S, false, true);
        public KeyBind Toolbar_Replicate = new("Replicate", KeybindCategory.MapEditor, Key.R, false, true);
        public KeyBind Toolbar_Set_to_Grid = new("Set to Grid", KeybindCategory.MapEditor, Key.G, false, true);
        public KeyBind Toolbar_Create = new("Create", KeybindCategory.MapEditor, Key.C, false, true);
        public KeyBind Toolbar_RenderEnemyPatrolRoutes = new("Render Patrol Routes", KeybindCategory.MapEditor, Key.P, true);

        public KeyBind Toolbar_ExportPrefab = new("Export Prefab", KeybindCategory.MapEditor, Key.E, true, true);
        public KeyBind Toolbar_ImportPrefab = new("Import Prefab", KeybindCategory.MapEditor, Key.I, true, true);

        // Map Editor
        public KeyBind Map_DuplicateToMap = new("Duplicate to Map", KeybindCategory.MapEditor, Key.D, false, false, true);
        public KeyBind Map_PropSearch = new("Property Search", KeybindCategory.MapEditor, Key.F, true);
        public KeyBind Map_RenderGroup_GetDisp = new("Render Group: Get Display Group", KeybindCategory.MapEditor, Key.G, true);
        public KeyBind Map_RenderGroup_GetDraw = new("Render Group: Get Draw Group", KeybindCategory.MapEditor);
        public KeyBind Map_RenderGroup_GiveDisp = new("Render Group: Give Display Group", KeybindCategory.MapEditor);
        public KeyBind Map_RenderGroup_GiveDraw = new("Render Group: Give Draw Group", KeybindCategory.MapEditor);
        public KeyBind Map_RenderGroup_HideAll = new("Render Group: Hide All", KeybindCategory.MapEditor);
        public KeyBind Map_RenderGroup_ShowAll = new("Render Group: Show All", KeybindCategory.MapEditor, Key.R, true);
        public KeyBind Map_RenderGroup_SelectHighlights = new("Render Group: Select Highlights", KeybindCategory.MapEditor);

        public KeyBind Map_CreateSelectionGroup = new("Create Selection Group", KeybindCategory.MapEditor, Key.L, false, true);

        public KeyBind Map_QuickSelect_SelectionGroup_0 = new("Select Selection Group 0", KeybindCategory.MapEditor, Key.Keypad0, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_1 = new("Select Selection Group 1", KeybindCategory.MapEditor, Key.Keypad1, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_2 = new("Select Selection Group 2", KeybindCategory.MapEditor, Key.Keypad2, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_3 = new("Select Selection Group 3", KeybindCategory.MapEditor, Key.Keypad3, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_4 = new("Select Selection Group 4", KeybindCategory.MapEditor, Key.Keypad4, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_5 = new("Select Selection Group 5", KeybindCategory.MapEditor, Key.Keypad5, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_6 = new("Select Selection Group 6", KeybindCategory.MapEditor, Key.Keypad6, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_7 = new("Select Selection Group 7", KeybindCategory.MapEditor, Key.Keypad7, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_8 = new("Select Selection Group 9", KeybindCategory.MapEditor, Key.Keypad8, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_9 = new("Select Selection Group 0", KeybindCategory.MapEditor, Key.Keypad9, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_10 = new("Select Selection Group 10", KeybindCategory.MapEditor, Key.KeypadAdd, false, false);

        public KeyBind MapEditor_MoveOrderUp = new("Move Map Object Up in List", KeybindCategory.MapEditor, Key.U, true, false);
        public KeyBind MapEditor_MoveOrderDown = new("Move Map Object Down in List", KeybindCategory.MapEditor, Key.J, true, false);
        public KeyBind MapEditor_MoveOrderTop = new("Move Map Object to Top in List", KeybindCategory.MapEditor, Key.U, true, true);
        public KeyBind MapEditor_MoveOrderBottom = new("Move Map Object to Bottom in List", KeybindCategory.MapEditor, Key.J, true, true);

        public KeyBind Map_WorldMap_Vanilla = new("Toggle Lands Between Map", KeybindCategory.MapEditor, Key.M, true, false, false);
        public KeyBind Map_WorldMap_SOTE = new("Toggle Shadow of the Erdtree Map", KeybindCategory.MapEditor, Key.M, true, true, false);
        public KeyBind Map_WorldMap_DragMap = new("Drag Map", KeybindCategory.MapEditor, Key.C, false, false, false);

        // Model Editor
        public KeyBind ModelEditor_ToggleVisibilitySection = new("Toggle Section (when clicking Visibility Icon)", KeybindCategory.ModelEditor, Key.A);
        public KeyBind ModelEditor_Multiselect = new("Multi-Select Row (hold)", KeybindCategory.ModelEditor, Key.Z);
        public KeyBind ModelEditor_Multiselect_Range = new("Multi-Select Row Range (hold then click start and end row)", KeybindCategory.ModelEditor, Key.LShift, false, false, false, true);
        public KeyBind ModelEditor_ExportModel = new("Export Model", KeybindCategory.ModelEditor, Key.K, true);

        // Param
        public KeyBind Param_Copy = new("Copy", KeybindCategory.ParamEditor, Key.C, true);
        public KeyBind Param_ExportCSV = new("Export CSV", KeybindCategory.ParamEditor);
        public KeyBind Param_GotoBack = new("Go to Back", KeybindCategory.ParamEditor, Key.Escape);
        public KeyBind Param_GotoRowID = new("Go to Row ID", KeybindCategory.ParamEditor, Key.G, true);
        public KeyBind Param_GotoSelectedRow = new("Go to Selected Row", KeybindCategory.ParamEditor, Key.G);
        public KeyBind Param_HotReload = new("Hot Reload", KeybindCategory.ParamEditor, Key.F5);
        public KeyBind Param_HotReloadAll = new("Hot Reload All", KeybindCategory.ParamEditor, Key.F5, false, false, true);
        public KeyBind Param_ImportCSV = new("Import CSV", KeybindCategory.ParamEditor);
        public KeyBind Param_MassEdit = new("Mass Edit", KeybindCategory.ParamEditor);
        public KeyBind Param_Paste = new("Paste", KeybindCategory.ParamEditor, Key.V, true);
        public KeyBind Param_SearchField = new("Search Field", KeybindCategory.ParamEditor, Key.N, true);
        public KeyBind Param_SearchParam = new("Search Param", KeybindCategory.ParamEditor, Key.P, true);
        public KeyBind Param_SearchRow = new("Search Row", KeybindCategory.ParamEditor, Key.F, true);
        public KeyBind Param_SelectAll = new("Select All", KeybindCategory.ParamEditor, Key.A, true);

        // Text FMG
        public KeyBind TextFMG_Sync = new("Sync Description", KeybindCategory.TextEditor, Key.K, true);
        public KeyBind TextFMG_Search = new("Search", KeybindCategory.TextEditor, Key.F, true);

        // Viewport
        public KeyBind Viewport_Cam_Back = new("Back", KeybindCategory.Viewport, Key.S);
        public KeyBind Viewport_Cam_Down = new("Down", KeybindCategory.Viewport, Key.Q);
        public KeyBind Viewport_Cam_Forward = new("Forward", KeybindCategory.Viewport, Key.W);
        public KeyBind Viewport_Cam_Left = new("Left", KeybindCategory.Viewport, Key.A);
        public KeyBind Viewport_Cam_Reset = new("Reset", KeybindCategory.Viewport, Key.R);
        public KeyBind Viewport_Cam_Right = new("Right", KeybindCategory.Viewport, Key.D);
        public KeyBind Viewport_Cam_Up = new("Up", KeybindCategory.Viewport, Key.E);
        public KeyBind Viewport_RotationMode = new("Gizmo Rotation Mode", KeybindCategory.Viewport, Key.E);
        public KeyBind Viewport_ToggleGizmoOrigin = new("Toggle Gizmo Origin", KeybindCategory.Viewport, Key.Home);
        public KeyBind Viewport_ToggleGizmoSpace = new("Toggle Gizmo Space", KeybindCategory.Viewport);
        public KeyBind Viewport_TranslateMode = new("Gizmo Translate Mode", KeybindCategory.Viewport, Key.W);
        public KeyBind Map_ViewportGrid_Lower = new("Map Grid: Lower", KeybindCategory.Viewport, Key.Q, true);
        public KeyBind Map_ViewportGrid_Raise = new("Map Grid: Raise", KeybindCategory.Viewport, Key.E, true);
        public KeyBind Map_ViewportGrid_Bring_to_Selection = new("Map Grid: Bring to Selection", KeybindCategory.Viewport, Key.K, true);
        public KeyBind Map_ToggleRenderOutline = new("Toggle Selection Outline", KeybindCategory.Viewport);

        // TAE Editor
        public KeyBind TimeActEditor_Multiselect = new("Multi-Select Row (hold)", KeybindCategory.TimeActEditor, Key.Z);
        public KeyBind TimeActEditor_Multiselect_Range = new("Multi-Select Row Range (hold then click start and end row)", KeybindCategory.TimeActEditor, Key.LShift, false, false, false, true);

        // Texture Viewer
        public KeyBind TextureViewer_ExportTexture = new("Export Texture", KeybindCategory.TextureViewer, Key.X, true);
        public KeyBind TextureViewer_ZoomMode = new("Zoom Mode", KeybindCategory.TextureViewer, Key.LControl, false, false, false, true);
        public KeyBind TextureViewer_ZoomReset = new("Reset Zoom", KeybindCategory.TextureViewer, Key.R);

#pragma warning disable IDE0051
        // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
        [JsonExtensionData] internal IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051
    }
}
