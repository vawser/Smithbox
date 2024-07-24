using StudioCore.Localization;
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
        public KeyBind Core_Create = new($"{LOC.Get("KEYBIND__CORE__CREATE")}",
            KeybindCategory.Core, Key.Insert);
        public KeyBind Core_Delete = new($"{LOC.Get("KEYBIND__CORE__DELETE")}", 
            KeybindCategory.Core, Key.Delete);
        public KeyBind Core_Duplicate = new($"{LOC.Get("KEYBIND__CORE__DUPLICATE")}", 
            KeybindCategory.Core, Key.D, true);
        public KeyBind Core_Redo = new($"{LOC.Get("KEYBIND__CORE__REDO")}", 
            KeybindCategory.Core, Key.Y, true);
        public KeyBind Core_Undo = new($"{LOC.Get("KEYBIND__CORE__UNDO")}", 
            KeybindCategory.Core, Key.Z, true);
        public KeyBind Core_SaveAllCurrentEditor = new($"{LOC.Get("KEYBIND__CORE__SAVE_ALL")}", 
            KeybindCategory.Core);
        public KeyBind Core_SaveCurrentEditor = new($"{LOC.Get("KEYBIND__CORE__SAVE")}", 
            KeybindCategory.Core, Key.S, true);

        // Windows
        public KeyBind ToggleWindow_Project = new($"{LOC.Get("KEYBIND__WINDOW__PROJECT_WINDOW")}", 
            KeybindCategory.Window, Key.F1);
        public KeyBind ToggleWindow_Help = new($"{LOC.Get("KEYBIND__WINDOW__HELP_WINDOW")}", 
            KeybindCategory.Window, Key.F2);
        public KeyBind ToggleWindow_Keybind = new($"{LOC.Get("KEYBIND__WINDOW__KEYBIND_WINDOW")}", 
            KeybindCategory.Window, Key.F3);
        public KeyBind ToggleWindow_Memory = new($"{LOC.Get("KEYBIND__WINDOW__MEMORY_WINDOW")}", 
            KeybindCategory.Window, Key.F4);
        public KeyBind ToggleWindow_Settings = new($"{LOC.Get("KEYBIND__WINDOW__SETTINGS_WINDOW")}", 
            KeybindCategory.Window, Key.F6);
        public KeyBind ToggleWindow_Alias = new($"{LOC.Get("KEYBIND__WINDOW__ALIAS_WINDOW")}", 
            KeybindCategory.Window, Key.F7);
        public KeyBind ToggleWindow_QuickTools = new($"{LOC.Get("KEYBIND__WINDOW__TOOL_WINDOW")}", 
            KeybindCategory.Window, Key.F8);
        public KeyBind ToggleWindow_Debug = new($"{LOC.Get("KEYBIND__WINDOW__DEBUG_WINDOW")}", 
            KeybindCategory.Window, Key.F9);

        // Map Toolbar
        public KeyBind Toolbar_Rotate_X = new($"{LOC.Get("KEYBIND__MAP_EDITOR__ROTATE_SELECTION_ON_X_AXIS")}", 
            KeybindCategory.MapEditor, Key.J);
        public KeyBind Toolbar_Rotate_Y = new($"{LOC.Get("KEYBIND__MAP_EDITOR__ROTATE_SELECTION_ON_Y_AXIS")}", 
            KeybindCategory.MapEditor, Key.K, false, false, true);
        public KeyBind Toolbar_Rotate_Y_Pivot = new($"{LOC.Get("KEYBIND__MAP_EDITOR__PIVOT_SELECTION_ON_X_AXIS")}", 
            KeybindCategory.MapEditor, Key.K);
        public KeyBind Toolbar_Go_to_Selection_in_Object_List = new($"{LOC.Get("KEYBIND__MAP_EDITOR__GO_TO_SELECTION_IN_MAP_OBJECT_LIST")}", 
            KeybindCategory.MapEditor, Key.G);
        public KeyBind Toolbar_Move_Selection_to_Camera = new($"{LOC.Get("KEYBIND__MAP_EDITOR__MOVE_SELECTION_TO_CAMERA_POSITION")}", 
            KeybindCategory.MapEditor, Key.X);
        public KeyBind Toolbar_Frame_Selection_in_Viewport = new($"{LOC.Get("KEYBIND__MAP_EDITOR__FRAME_SELECTION_IN_VIEWPORT")}", 
            KeybindCategory.MapEditor, Key.F);
        public KeyBind Toolbar_Toggle_Selection_Visibility_Flip = new($"{LOC.Get("KEYBIND__MAP_EDITOR__FLIP_VISIBILITY_OF_SELECTION")}", 
            KeybindCategory.MapEditor, Key.H, true);
        public KeyBind Toolbar_Toggle_Map_Visibility_Flip = new($"{LOC.Get("KEYBIND__MAP_EDITOR__FLIP_VISIBILITY_OF_ALL_ENTITIES")}", 
            KeybindCategory.MapEditor, Key.B, true);
        public KeyBind Toolbar_Toggle_Selection_Visibility_Enabled = new($"{LOC.Get("KEYBIND__MAP_EDITOR__ENABLE_VISIBILITY_OF_SELECTION")}", 
            KeybindCategory.MapEditor, Key.H, true, true);
        public KeyBind Toolbar_Toggle_Map_Visibility_Enabled = new($"{LOC.Get("KEYBIND__MAP_EDITOR__ENABLE_VISIBILITY_OF_ALL_ENTITIES")}", 
            KeybindCategory.MapEditor, Key.B, true, true);
        public KeyBind Toolbar_Toggle_Selection_Visibility_Disabled = new($"{LOC.Get("KEYBIND__MAP_EDITOR__DISABLE_VISIBILITY_OF_SELECTION")}",
            KeybindCategory.MapEditor, Key.H, true, true, true);
        public KeyBind Toolbar_Toggle_Map_Visibility_Disabled = new($"{LOC.Get("KEYBIND__MAP_EDITOR__DISABLE_VISIBILITY_OF_ALL_ENTITIES")}", 
            KeybindCategory.MapEditor, Key.B, true, true, true);
        public KeyBind Toolbar_Reset_Rotation = new($"{LOC.Get("KEYBIND__MAP_EDITOR__RESET_SELECTION_ROTATION")}", 
            KeybindCategory.MapEditor, Key.L);
        public KeyBind Toolbar_Dummify = new($"{LOC.Get("KEYBIND__MAP_EDITOR__MAKE_SELECTION_A_DUMMY_MAP_OBJECT")}", 
            KeybindCategory.MapEditor, Key.Comma, false, false, true);
        public KeyBind Toolbar_Undummify = new($"{LOC.Get("KEYBIND__MAP_EDITOR__MAKE_SELECTION_A_NORMAL_MAP_OBJECT")}", 
            KeybindCategory.MapEditor, Key.Period, false, false, true);
        public KeyBind Toolbar_Scramble = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SCRAMBLE_SELECTION")}", 
            KeybindCategory.MapEditor, Key.S, false, true);
        public KeyBind Toolbar_Replicate = new($"{LOC.Get("KEYBIND__MAP_EDITOR__REPLICATE_SELECTION")}", 
            KeybindCategory.MapEditor, Key.R, false, true);
        public KeyBind Toolbar_Set_to_Grid = new($"{LOC.Get("KEYBIND__MAP_EDITOR__MOVE_SELECTION_TO_VIEWPORT_GRID")}", 
            KeybindCategory.MapEditor, Key.G, false, true);
        public KeyBind Toolbar_Create = new($"{LOC.Get("KEYBIND__MAP_EDITOR__CREATE_MAP_OBJECT")}", 
            KeybindCategory.MapEditor, Key.C, false, true);
        public KeyBind Toolbar_RenderEnemyPatrolRoutes = new($"{LOC.Get("KEYBIND__MAP_EDITOR__RENDER_PATROL_ROUTE_CONNECTIONS")}", 
            KeybindCategory.MapEditor, Key.P, true);

        public KeyBind Toolbar_ExportPrefab = new($"{LOC.Get("KEYBIND__MAP_EDITOR__EXPORT_PREFAB")}", 
            KeybindCategory.MapEditor, Key.E, true, true);
        public KeyBind Toolbar_ImportPrefab = new($"{LOC.Get("KEYBIND__MAP_EDITOR__IMPORT_PREFAB")}", 
            KeybindCategory.MapEditor, Key.I, true, true);

        public KeyBind Map_DuplicateToMap = new($"{LOC.Get("KEYBIND__MAP_EDITOR__COPY_SELECTION_TO_NEW_MAP")}", 
            KeybindCategory.MapEditor, Key.D, false, false, true);
        public KeyBind Map_PropSearch = new($"{LOC.Get("KEYBIND__MAP_EDITOR__PROPERTY_SEARCH")}", 
            KeybindCategory.MapEditor, Key.F, true);
        public KeyBind Map_RenderGroup_GetDisp = new($"{LOC.Get("KEYBIND__MAP_EDITOR__RENDER_GROUP__GET_DISPLAY_GROUP")}", 
            KeybindCategory.MapEditor, Key.G, true);
        public KeyBind Map_RenderGroup_GetDraw = new($"{LOC.Get("KEYBIND__MAP_EDITOR__RENDER_GROUP__GET_DRAW_GROUP")}", 
            KeybindCategory.MapEditor);
        public KeyBind Map_RenderGroup_GiveDisp = new($"{LOC.Get("KEYBIND__MAP_EDITOR__RENDER_GROUP__GIVE_DISPLAY_GROUP")}", 
            KeybindCategory.MapEditor);
        public KeyBind Map_RenderGroup_GiveDraw = new($"{LOC.Get("KEYBIND__MAP_EDITOR__RENDER_GROUP__GIVE_DRAW_GROUP")}", 
            KeybindCategory.MapEditor);
        public KeyBind Map_RenderGroup_HideAll = new($"{LOC.Get("KEYBIND__MAP_EDITOR__RENDER_GROUP__HIDE_ALL")}", 
            KeybindCategory.MapEditor);
        public KeyBind Map_RenderGroup_ShowAll = new($"{LOC.Get("KEYBIND__MAP_EDITOR__RENDER_GROUP__SHOW_ALL")}", 
            KeybindCategory.MapEditor, Key.R, true);
        public KeyBind Map_RenderGroup_SelectHighlights = new($"{LOC.Get("KEYBIND__MAP_EDITOR__RENDER_GROUP__SELECT_HIGHLIGHTS")}", 
            KeybindCategory.MapEditor);

        public KeyBind Map_CreateSelectionGroup = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_CREATE")}", 
            KeybindCategory.MapEditor, Key.L, false, true);
        public KeyBind Map_QuickSelect_SelectionGroup_0 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_0")}", 
            KeybindCategory.MapEditor, Key.Keypad0, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_1 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_1")}", 
            KeybindCategory.MapEditor, Key.Keypad1, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_2 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_2")}", 
            KeybindCategory.MapEditor, Key.Keypad2, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_3 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_3")}", 
            KeybindCategory.MapEditor, Key.Keypad3, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_4 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_4")}", 
            KeybindCategory.MapEditor, Key.Keypad4, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_5 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_5")}", 
            KeybindCategory.MapEditor, Key.Keypad5, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_6 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_6")}", 
            KeybindCategory.MapEditor, Key.Keypad6, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_7 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_7")}", 
            KeybindCategory.MapEditor, Key.Keypad7, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_8 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_8")}", 
            KeybindCategory.MapEditor, Key.Keypad8, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_9 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_9")}", 
            KeybindCategory.MapEditor, Key.Keypad9, false, false);
        public KeyBind Map_QuickSelect_SelectionGroup_10 = new($"{LOC.Get("KEYBIND__MAP_EDITOR__SELECTION_GROUP_SELECT_10")}", 
            KeybindCategory.MapEditor, Key.KeypadAdd, false, false);

        public KeyBind MapEditor_MoveOrderUp = new($"{LOC.Get("KEYBIND__MAP_EDITOR__ORDER_UP")}", 
            KeybindCategory.MapEditor, Key.U, true, false);
        public KeyBind MapEditor_MoveOrderDown = new($"{LOC.Get("KEYBIND__MAP_EDITOR__ORDER_DOWN")}", 
            KeybindCategory.MapEditor, Key.J, true, false);
        public KeyBind MapEditor_MoveOrderTop = new($"{LOC.Get("KEYBIND__MAP_EDITOR__ORDER_TOP")}", 
            KeybindCategory.MapEditor, Key.U, true, true);
        public KeyBind MapEditor_MoveOrderBottom = new($"{LOC.Get("KEYBIND__MAP_EDITOR__ORDER_BOTTOM")}", 
            KeybindCategory.MapEditor, Key.J, true, true);

        public KeyBind Map_WorldMap_Vanilla = new($"{LOC.Get("KEYBIND__MAP_EDITOR__MAP__OPEN_LANDS_BETWEEN_MAP")}", 
            KeybindCategory.MapEditor, Key.M, true, false, false);
        public KeyBind Map_WorldMap_SOTE = new($"{LOC.Get("KEYBIND__MAP_EDITOR__MAP__OPEN_SOTE_MAP")}", 
            KeybindCategory.MapEditor, Key.M, true, true, false);

        // Model Editor
        public KeyBind ModelEditor_ToggleVisibilitySection = new($"{LOC.Get("KEYBIND__MODEL_EDITOR__TOGGLE_VISIBILITY_ALL")}", KeybindCategory.ModelEditor, Key.A);
        public KeyBind ModelEditor_Multiselect = new($"{LOC.Get("KEYBIND__MODEL_EDITOR__MULTISELECT_ROW")}", 
            KeybindCategory.ModelEditor, Key.Z);
        public KeyBind ModelEditor_Multiselect_Range = new($"{LOC.Get("KEYBIND__MODEL_EDITOR__MULTISELECT_ROW_RANGE")}", KeybindCategory.ModelEditor, Key.LShift, false, false, false, true);
        public KeyBind ModelEditor_ExportModel = new($"{LOC.Get("KEYBIND__MODEL_EDITOR__EXPORT_MODEL")}", 
            KeybindCategory.ModelEditor, Key.K, true);

        // Param
        public KeyBind Param_Copy = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__COPY_ROW")}", 
            KeybindCategory.ParamEditor, Key.C, true);
        public KeyBind Param_Paste = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__PASTE_ROW")}",
            KeybindCategory.ParamEditor, Key.V, true);
        public KeyBind Param_ImportCSV = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__IMPORT_CSV")}",
            KeybindCategory.ParamEditor);
        public KeyBind Param_ExportCSV = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__EXPORT_CSV")}", 
            KeybindCategory.ParamEditor);
        public KeyBind Param_GotoBack = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__OVERVIEW_GO_BACK")}", 
            KeybindCategory.ParamEditor, Key.Escape);
        public KeyBind Param_GotoRowID = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__GO_TO_ROW_ID")}", 
            KeybindCategory.ParamEditor, Key.G, true);
        public KeyBind Param_GotoSelectedRow = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__GO_TO_SELECTED_ROW")}", 
            KeybindCategory.ParamEditor, Key.G);
        public KeyBind Param_HotReload = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__HOT_LOAD_PARAM")}", 
            KeybindCategory.ParamEditor, Key.F5);
        public KeyBind Param_HotReloadAll = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__HOT_LOAD_PARAMS")}", 
            KeybindCategory.ParamEditor, Key.F5, false, false, true);
        public KeyBind Param_MassEdit = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__TOGGLE_MASSEDIT_WINDOW")}", 
            KeybindCategory.ParamEditor);
        public KeyBind Param_SearchField = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__SEARCH_FIELD")}", 
            KeybindCategory.ParamEditor, Key.N, true);
        public KeyBind Param_SearchRow = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__SEARCH_ROW")}", 
            KeybindCategory.ParamEditor, Key.F, true);
        public KeyBind Param_SearchParam = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__SEARCH_PARAM")}",
            KeybindCategory.ParamEditor, Key.P, true);
        public KeyBind Param_SelectAll = new($"{LOC.Get("KEYBIND__PARAM_EDITOR__SELECT_ALL_ROWS")}", 
            KeybindCategory.ParamEditor, Key.A, true);

        // Text FMG
        public KeyBind TextFMG_Sync = new($"{LOC.Get("KEYBIND__TEXT_EDITOR__SYNC_DESCRIPTIONS")}", 
            KeybindCategory.TextEditor, Key.K, true);
        public KeyBind TextFMG_Search = new($"{LOC.Get("KEYBIND__TEXT_EDITOR__SEARCH_TEXT")}", 
            KeybindCategory.TextEditor, Key.F, true);

        // Viewport
        public KeyBind Viewport_Cam_Back = new($"{LOC.Get("KEYBIND__VIEWPORT__MOVE_BACKWARD")}", 
            KeybindCategory.Viewport, Key.S);
        public KeyBind Viewport_Cam_Down = new($"{LOC.Get("KEYBIND__VIEWPORT__MOVE_DOWN")}", 
            KeybindCategory.Viewport, Key.Q);
        public KeyBind Viewport_Cam_Forward = new($"{LOC.Get("KEYBIND__VIEWPORT__MOVE_FORWARD")}", 
            KeybindCategory.Viewport, Key.W);
        public KeyBind Viewport_Cam_Left = new($"{LOC.Get("KEYBIND__VIEWPORT__MOVE_LEFT")}", 
            KeybindCategory.Viewport, Key.A);
        public KeyBind Viewport_Cam_Right = new($"{LOC.Get("KEYBIND__VIEWPORT__MOVE_RIGHT")}", 
            KeybindCategory.Viewport, Key.D);
        public KeyBind Viewport_Cam_Up = new($"{LOC.Get("KEYBIND__VIEWPORT__MOVE_UP")}", 
            KeybindCategory.Viewport, Key.E);

        public KeyBind Viewport_Cam_Reset = new($"{LOC.Get("KEYBIND__VIEWPORT__RESET")}",
            KeybindCategory.Viewport, Key.R);

        public KeyBind Viewport_RotationMode = new($"{LOC.Get("KEYBIND__VIEWPORT__GIZMO_ROTATION_MODE")}", 
            KeybindCategory.Viewport, Key.E);
        public KeyBind Viewport_ToggleGizmoOrigin = new($"{LOC.Get("KEYBIND__VIEWPORT__GIZMO_ORIGIN_MODE")}", 
            KeybindCategory.Viewport, Key.Home);
        public KeyBind Viewport_ToggleGizmoSpace = new($"{LOC.Get("KEYBIND__VIEWPORT__GIZMO_SPACE_MODE")}", 
            KeybindCategory.Viewport);
        public KeyBind Viewport_TranslateMode = new($"{LOC.Get("KEYBIND__VIEWPORT__GIZMO_TRANSLATE_MODE")}", 
            KeybindCategory.Viewport, Key.W);

        public KeyBind Map_ViewportGrid_Lower = new($"{LOC.Get("KEYBIND__VIEWPORT__MAP_GRID__LOWER")}", 
            KeybindCategory.Viewport, Key.Q, true);
        public KeyBind Map_ViewportGrid_Raise = new($"{LOC.Get("KEYBIND__VIEWPORT__MAP_GRID__RAISE")}", 
            KeybindCategory.Viewport, Key.E, true);
        public KeyBind Map_ViewportGrid_Bring_to_Selection = new($"{LOC.Get("KEYBIND__VIEWPORT__MAP_GRID__SET_TO_SELECTION")}", 
            KeybindCategory.Viewport, Key.K, true);

        public KeyBind Map_ToggleRenderOutline = new($"{LOC.Get("KEYBIND__VIEWPORT__TOGGLE_SELECTION_OUTLINE")}", 
            KeybindCategory.Viewport);

        // Texture Viewer
        public KeyBind TextureViewer_ZoomMode = new($"{LOC.Get("KEYBIND__TEXTURE_VIEWER__ZOOM_MODE")}", 
            KeybindCategory.TextureViewer, Key.LControl, false, false, false, true);
        public KeyBind TextureViewer_ZoomReset = new($"{LOC.Get("KEYBIND__TEXTURE_VIEWER__RESET_ZOOM")}", 
            KeybindCategory.TextureViewer, Key.R);

#pragma warning disable IDE0051
        // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
        [JsonExtensionData] internal IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051
    }
}
