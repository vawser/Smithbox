using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Veldrid;

namespace StudioCore;

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
    public string Description;

    [JsonConstructor]
    public KeyBind()
    {
        PresentationName = "";
        Description = "";
    }

    public KeyBind(KeyBind existingKeybind)
    {
        PresentationName = existingKeybind.PresentationName;
        Description = existingKeybind.Description;

        PrimaryKey = existingKeybind.PrimaryKey;
        Ctrl_Pressed = existingKeybind.Ctrl_Pressed;
        Alt_Pressed = existingKeybind.Alt_Pressed;
        Shift_Pressed = existingKeybind.Shift_Pressed;
        FixedKey = existingKeybind.FixedKey;
    }

    public KeyBind(string name, string description, Key primaryKey = Key.Unknown, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool fixedKey = false)
    {
        PresentationName = name;
        Description = description;

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
    public static Bindings Default { get; set; } = new();

    public static void Setup()
    {
        Current = new Bindings();
    }

    public static void Load()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Key Bindings.json");

        if (!File.Exists(file))
        {
            Current = new Bindings();
            Save();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.Bindings);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Key Bindings failed to load, default key binding has been restored.", LogLevel.Error, Tasks.LogPriority.High, e);

                Current = new Bindings();
                Save();
            }
        }
    }

    public static void Save()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Key Bindings.json");

        var json = JsonSerializer.Serialize(Current, SmithboxSerializerContext.Default.Bindings);

        File.WriteAllText(file, json);
    }

    public static void ResetToDefault()
    {
        foreach (var field in typeof(Bindings).GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            field.SetValue(Current, field.GetValue(Default));
        }
    }

    public static void ResetKeyBinds()
    {
        Current = new Bindings();
    }

    public class Bindings
    {
        //-----------------------------
        // Core
        //-----------------------------
        // Core
        public KeyBind CORE_CreateNewEntry = new(
            "Create New Entry", 
            "Creates a new default entry based on the current selection context.", 
            Key.Insert);

        public KeyBind CORE_DeleteSelectedEntry = new(
            "Delete Selected Entry", 
            "Deletes the selected entry or entries based on the current selection context.", 
            Key.Delete);

        public KeyBind CORE_DuplicateSelectedEntry = new(
            "Duplicate",
            "Duplicates the selected entry or entries based on the current selection context.",
            Key.D, 
            true);

        public KeyBind CORE_RedoAction = new(
            "Redo", 
            "Re-executes a previously un-done action.", 
            Key.Y, 
            true);

        public KeyBind CORE_RedoContinuousAction = new(
            "Redo (Continuous)",
            "Re-executes previously un-done actions whilst held.",
            Key.Y,
            true,
            true);

        public KeyBind CORE_UndoAction = new(
            "Undo",
            "Undoes a previously executed action.",
            Key.Z,
            true);

        public KeyBind CORE_UndoContinuousAction = new(
            "Undo (Continuous)",
            "Undo previously executed actions whilst held.",
            Key.Z,
            true,
            true);

        public KeyBind CORE_SaveAll = new(
            "Save All", 
            "Saves all modified files within the focused editor.",
            Key.Unknown);

        public KeyBind CORE_Save = new(
            "Save", 
            "Save the current file-level selection within the focused editor.", 
            Key.S, 
            true);

        //-----------------------------
        // Viewport
        //-----------------------------
        // Core
        public KeyBind VIEWPORT_CameraForward = new(
            "Move Forward",
            "Moves the camera forward.",
            Key.W);

        public KeyBind VIEWPORT_CameraBack = new(
            "Move Back",
            "Moves the camera backwards.", 
            Key.S);

        public KeyBind VIEWPORT_CameraUp = new(
            "Move Up",
            "Moves the camera upwards.",
            Key.E);

        public KeyBind VIEWPORT_CameraDown = new(
            "Move Down",
            "Moves the camera downwards.", 
            Key.Q);

        public KeyBind VIEWPORT_CameraLeft = new(
            "Move Left",
            "Moves the camera leftwards.", 
            Key.A);

        public KeyBind VIEWPORT_CameraRight = new(
            "Move Right",
            "Moves the camera rightwards.",
            Key.D);

        public KeyBind VIEWPORT_CameraReset = new(
            "Reset Position", 
            "Resets the camera's position to (0,0,0)", 
            Key.R);

        // Information
        public KeyBind VIEWPORT_DisplayInformationPanel = new(
            "Toggle Information Panel",
            "Toggles the appearance of the transparent information panel, which displays the status of various tools.",
            Key.BracketLeft,
            true);

        // Gizmos
        public KeyBind VIEWPORT_GizmoRotationMode = new(
            "Cycle Gizmo Rotation Mode", 
            "Cycles through the gizmo rotation modes.", 
            Key.E);

        public KeyBind VIEWPORT_GizmoOriginMode = new(
            "Cycle Gizmo Origin Mode",
            "Cycles through the gizmo origin modes.",
            Key.Home);

        public KeyBind VIEWPORT_GizmoSpaceMode = new(
            "Cycle Gizmo Space Mode",
            "Cycles through the gizmo space modes.",
            Key.Unknown);

        public KeyBind VIEWPORT_GizmoTranslationMode = new(
            "Cycle Gizmo Translation Mode",
            "Cycles through the gizmo translation modes.", 
            Key.W);

        // Grid
        public KeyBind VIEWPORT_LowerGrid = new(
            "Lower Grid",
            "Lowers the viewport grid height by the specified unit increment.", 
            Key.Q, 
            true);

        public KeyBind VIEWPORT_RaiseGrid = new(
            "Raise Grid",
            "Raises the viewport grid height by the specified unit increment.",
            Key.E, 
            true);

        public KeyBind VIEWPORT_SetGridToSelectionHeight = new(
            "Move Grid to Selection Height",
            "Set the viewport grid height to the height of the current selection.",
            Key.K, 
            true);

        // Selection
        public KeyBind VIEWPORT_RenderOutline = new(
            "Toggle Selection Outline", 
            "Toggles the appearance of the selection outline.",
            Key.Unknown);

        public KeyBind VIEWPORT_ToggleRenderType = new(
            "Toggle Render Type",
            "Toggles the rendering type for the selection (if supported) between wireframe and solid.",
            Key.M);

        //-----------------------------
        // Map Editor
        //-----------------------------
        // Core
        public KeyBind MAP_GoToInList = new(
            "Go to in List",
            "Go to the selection within the Map Object List.",
            Key.G);

        public KeyBind MAP_MoveToCamera = new(
            "Move Selection to Camera",
            "Moves the current selection to the camera's position.",
            Key.X);

        public KeyBind MAP_FrameSelection = new(
            "Frame Selection",
            "Frames the current selection within the viewport.",
            Key.F);


        public KeyBind MAP_RotateFixedAngle = new(
            "Rotate to Fixed Increment for Selection",
            "Increment the rotation of the current selection to the fixed angle defined in the tool window.",
            Key.Unknown);

        public KeyBind MAP_ResetRotation = new(
            "Reset Rotation for Selection",
            "Resets the rotation of the current selection to (0,0,0).",
            Key.R,
            true,
            true);

        public KeyBind MAP_FlipSelectionVisibility = new(
            "Flip Editor Visibility of Selection", 
            "Flip the editor visibility state for the current selection.", 
            Key.H, 
            true);

        public KeyBind MAP_FlipAllVisibility = new(
            "Flip Editor Visibility of All",
            "Flip the editor visibility state for all map objects.",
            Key.B, 
            true);

        public KeyBind MAP_EnableSelectionVisibility = new(
            "Enable Editor Visibility of Selection",
            "Enable the editor visibility for the current selection.",
            Key.H,
            true, 
            true);

        public KeyBind MAP_EnableAllVisibility = new(
            "Enable Editor Visibility of All",
            "Enable the editor visibility for all map objects.",
            Key.B, 
            true, 
            true);

        public KeyBind MAP_DisableSelectionVisibility = new(
            "Disable Editor Visibility of Selection",
            "Disable the editor visibility for the current selection.",
            Key.H, 
            true, 
            true, 
            true);

        public KeyBind MAP_DisableAllVisibility = new(
            "Disable Editor Visibility of All",
            "Disable the editor visibility for all map objects.",
            Key.B, 
            true, 
            true, 
            true);

        public KeyBind MAP_MakeDummyObject = new(
            "Make Selection into Dummy Object", 
            "Changes the current selection into the equivalent Dummy map object type (if possible).",
            Key.Comma, 
            false, 
            false, 
            true);

        public KeyBind MAP_MakeNormalObject = new(
            "Make Selection into Normal Object",
            "Changes the current selection (if Dummy objects) into the equivalent normal map object type.",
            Key.Period, 
            false, 
            false, 
            true);

        public KeyBind MAP_DisableGamePresence = new(
            "Disable Game Presence of Selection",
            "Changes the current selection GameEditionDisable to 0, hiding it in-game.",
            Key.Unknown);

        public KeyBind MAP_EnableGamePresence = new(
            "Enable Game Presence of Selection",
            "Changes the current selection GameEditionDisable to 1, display it in-game.",
            Key.Unknown);

        public KeyBind MAP_ScrambleSelection = new(
            "Scramble Selection", 
            "Scrambles the position, rotation and scale (depending on Scramble tool settings) of the current selection.", 
            Key.S, 
            false, 
            true);

        public KeyBind MAP_ReplicateSelection = new(
            "Replicate Selection", 
            "Replicates the current selection (based on the Replicate tool settings).", 
            Key.R, 
            false, 
            true);

        public KeyBind MAP_SetSelectionToGrid = new(
            "Set Selection Height to Grid Height", 
            "Moves the current selection's height to the height of the viewport grid.", 
            Key.G, 
            false, 
            true);

        public KeyBind MAP_CreateMapObject = new(
            "Create Map Object", 
            "Create a new map object of the selected type with default values.", 
            Key.C, 
            false, 
            true);

        public KeyBind MAP_TogglePatrolRouteRendering = new(
            "Toggle Patrol Route Connections", 
            "Toggles the rendering of patrol route connections.", 
            Key.P, 
            true);

        public KeyBind MAP_DuplicateToMap = new(
            "Duplicate Selection to Map", 
            "Duplicates the current selection into the targeted map.", 
            Key.D, 
            false, 
            false, 
            true);

        // Rotation Increment
        public KeyBind MAP_RotateSelectionXAxis = new(
            "Rotate Selection on X-axis (Positive)",
            "Rotates the current selection on the X-axis in the positive direction by the specified increment.",
            Key.R,
            true);

        public KeyBind MAP_NegativeRotateSelectionXAxis = new(
            "Rotate Selection on X-axis (Negative)",
            "Rotates the current selection on the X-axis in the negative direction by the specified increment.",
            Key.Unknown);

        public KeyBind MAP_RotateSelectionYAxis = new(
            "Rotate Selection on Y-axis (Positive)",
            "Rotates the current selection on the Y-axis in the positive direction by the specified increment.",
            Key.Unknown);

        public KeyBind MAP_NegativeRotateSelectionYAxis = new(
            "Rotate Selection on Y-axis (Negative)",
            "Rotates the current selection on the Y-axis in the negative direction by the specified increment.",
            Key.Unknown);

        public KeyBind MAP_PivotSelectionYAxis = new(
            "Pivot Selection on Y-axis (Positive)",
            "Pivots the current selection on the Y-axis in the positive direction by the specified increment.",
            Key.R,
            false,
            false,
            true);

        public KeyBind MAP_NegativePivotSelectionYAxis = new(
            "Pivot Selection on Y-axis (Negative)",
            "Pivots the current selection on the Y-axis in the negative direction by the specified increment.",
            Key.Unknown);

        public KeyBind MAP_SwitchDegreeIncrementType = new(
            "Cycle Rotation Increment Type (Forward)",
            "Changes the degree increment used by Rotate Selection on X/Y-axis. Up to 5 increments can be stored.",
            Key.C,
            true);

        public KeyBind MAP_SwitchDegreeIncrementTypeBackward = new(
            "Cycle Rotation Increment Type (Backward)",
            "Changes the degree increment used by Rotate Selection on X/Y-axis. Up to 5 increments can be stored.",
            Key.Unknown);

        // Movement Increment
        public KeyBind MAP_KeyboardMove_PositiveX = new(
            "Movement Increment: X (Positive)",
            "Moves the selection on the x-axis.",
            Key.Right,
            true);

        public KeyBind MAP_KeyboardMove_NegativeX = new(
            "Movement Increment: X (Negative)",
            "Moves the selection on the x-axis.",
            Key.Left,
            true);

        public KeyBind MAP_KeyboardMove_PositiveY = new(
            "Movement Increment: Y (Positive)",
            "Moves the selection on the y-axis.",
            Key.Up,
            true);

        public KeyBind MAP_KeyboardMove_NegativeY = new(
            "Movement Increment: Y (Negative)",
            "Moves the selection on the y-axis.",
            Key.Down,
            true);

        public KeyBind MAP_KeyboardMove_PositiveZ = new(
            "Movement Increment: Z (Positive)",
            "Moves the selection on the z-axis.",
            Key.Up,
            true,
            true);

        public KeyBind MAP_KeyboardMove_NegativeZ = new(
            "Movement Increment: Z (Negative)",
            "Moves the selection on the z-axis.",
            Key.Down,
            true,
            true);

        public KeyBind MAP_KeyboardMove_CycleIncrement = new(
            "Keyboard Move: Cycle Increment (Forward)",
            "Cycles the increment used when moving via Keyboard Move.",
            Key.V,
            true);

        public KeyBind MAP_KeyboardMove_CycleIncrementBackward = new(
            "Keyboard Move: Cycle Increment (Backward)",
            "Cycles the increment used when moving via Keyboard Move.",
            Key.Unknown);

        // Render Groups
        public KeyBind MAP_GetDisplayGroup = new(
            "View Display Group", 
            "Display the display group for the current selection.", 
            Key.G, 
            true);

        public KeyBind MAP_GetDrawGroup = new(
            "View Draw Group",
            "Display the draw group for the current selection.",
            Key.Unknown);

        public KeyBind MAP_SetDisplayGroup = new(
            "Set Display Group", 
            "Set the display group (as appears in the Render Groups tab) to the current selection.",
            Key.Unknown);

        public KeyBind MAP_SetDrawGroup = new(
            "Render Group: Give Draw Group",
            "Set the draw group (as appears in the Render Groups tab) to the current selection.",
            Key.Unknown);

        public KeyBind MAP_HideAllDisplayGroups = new(
            "Hide All Display Groups", 
            "Set the current selection display groups to 0.",
            Key.Unknown);

        public KeyBind MAP_ShowAllDisplayGroups = new(
            "Show All Display Groups",
            "Set the current selection display groups to 0xFFFFFFFF.",
            Key.R, 
            true);

        public KeyBind MAP_SelectDisplayGroupHighlights = new(
            "Select Display Group Highlights", 
            "Select the objects that match the current display groups.",
            Key.Unknown);

        // Selection Group
        public KeyBind MAP_CreateSelectionGroup = new(
            "Create Selection Group", 
            "Creates a new selection group from current selection.", 
            Key.L, 
            false, 
            true);

        public KeyBind MAP_SelectionGroup_0 = new(
            "Select Selection Group 0", 
            "Select the contents of Selection Group 0 (if defined).", 
            Key.Keypad0, 
            false,
            false);

        public KeyBind MAP_SelectionGroup_1 = new(
            "Select Selection Group 1",
            "Select the contents of Selection Group 1 (if defined).",
            Key.Keypad1, 
            false, 
            false);

        public KeyBind MAP_SelectionGroup_2 = new(
            "Select Selection Group 2",
            "Select the contents of Selection Group 2 (if defined).",
            Key.Keypad2, 
            false, 
            false);

        public KeyBind MAP_SelectionGroup_3 = new(
            "Select Selection Group 3",
            "Select the contents of Selection Group 3 (if defined).",
            Key.Keypad3, 
            false,
            false);


        public KeyBind MAP_SelectionGroup4 = new(
            "Select Selection Group 4",
            "Select the contents of Selection Group 4 (if defined).",
            Key.Keypad4,
            false,
            false);


        public KeyBind MAP_SelectionGroup5 = new(
            "Select Selection Group 5",
            "Select the contents of Selection Group 5 (if defined).",
            Key.Keypad5,
            false,
            false);


        public KeyBind MAP_SelectionGroup6 = new(
            "Select Selection Group 6",
            "Select the contents of Selection Group 6 (if defined).",
            Key.Keypad6,
            false,
            false);

        public KeyBind MAP_SelectionGroup7 = new(
            "Select Selection Group 7",
            "Select the contents of Selection Group 7 (if defined).",
            Key.Keypad7,
            false,
            false);


        public KeyBind MAP_SelectionGroup8 = new(
            "Select Selection Group 8",
            "Select the contents of Selection Group 8 (if defined).",
            Key.Keypad8,
            false,
            false);


        public KeyBind MAP_SelectionGroup9 = new(
            "Select Selection Group 9",
            "Select the contents of Selection Group 9 (if defined).",
            Key.Keypad9,
            false,
            false);

        public KeyBind MAP_SelectionGroup10 = new(
            "Select Selection Group 10",
            "Select the contents of Selection Group 10 (if defined).",
            Key.KeypadAdd, 
            false, 
            false);

        // Order
        public KeyBind MAP_MoveObjectUp = new(
            "Move Map Object Up in List", 
            "Moves the selected map object up on the Map Object List order.", 
            Key.U, 
            true, 
            false);

        public KeyBind MAP_MoveObjectDown = new(
            "Move Map Object Down in List",
            "Moves the selected map object down on the Map Object List order.",
            Key.J,
            true, 
            false);

        public KeyBind MAP_MoveObjectTop = new(
            "Move Map Object to Top in List",
            "Moves the selected map object to the top of the Map Object List order.",
            Key.U, 
            true, 
            true);

        public KeyBind MAP_MoveObjectBottom = new(
            "Move Map Object to Bottom in List",
            "Moves the selected map object to the bottom of the Map Object List order.",
            Key.J, 
            true, 
            true);

        // World Map
        public KeyBind MAP_ToggleWorldMap = new(
            "Toggle World Map", 
            "Toggles the visibility of the world map.",
            Key.M, 
            true, 
            false, 
            false);

        //-----------------------------
        // Model Editor
        //-----------------------------
        // Core
        public KeyBind MODEL_ToggleVisibility = new(
            "Toggle Section Visibility", 
            "Applies visibility change to all members of the section when clicking the visibility eye icon.", 
            Key.A);

        public KeyBind MODEL_Multiselect = new(
            "Multi-Select Row", 
            "When held, multiple rows may be selected.", 
            Key.Z);

        public KeyBind MODEL_MultiselectRange = new(
            "Multi-Select Row Range", 
            "When held, the next row selected will be considered the 'start', and the next row after that the 'end'. All rows between them will be selected.",
            Key.LShift, 
            false, 
            false, 
            false, 
            true);

        public KeyBind MODEL_ExportModel = new(
            "Export Model", 
            "Export the currently loaded model.", 
            Key.K, 
            true);

        //-----------------------------
        // Param Editor
        //-----------------------------
        // Core
        public KeyBind PARAM_SelectAll = new(
            "Select All",
            "Select all rows.",
            Key.A,
            true);

        public KeyBind PARAM_GoToSelectedRow = new(
            "Go to Selected Row",
            "Change the list view to the currently selected row.",
            Key.G);

        public KeyBind PARAM_GoToRowID = new(
            "Go to Row ID",
            "Trigger the Row ID search prompt.",
            Key.G,
            true);

        public KeyBind PARAM_SortRows = new(
            "Sort Rows",
            "Sort the rows of the currently selected param",
            Key.Unknown);

        public KeyBind PARAM_CopyToClipboard = new(
            "Copy Selection to Clipboard", 
            "Copies the current param row to the clipboard.",
            Key.C, 
            true);

        public KeyBind PARAM_PasteClipboard = new(
            "Paste Clipboard",
            "Paste the current param row in the clipboard.",
            Key.V,
            true);

        public KeyBind PARAM_SearchParam = new(
            "Focus Param Search",
            "Moves focus to the param search input.",
            Key.P,
            true);

        public KeyBind PARAM_SearchRow = new(
            "Focus Row Search",
            "Moves focus to the row search input.",
            Key.F,
            true);

        public KeyBind PARAM_SearchField = new(
            "Focus Field Search",
            "Moves focus to the field search input.",
            Key.N,
            true);

        public KeyBind PARAM_CopyId = new(
            "Copy ID",
            "Copy the selected row ID (will form a list if multiple).",
            Key.C,
            false,
            true);

        public KeyBind PARAM_CopyIdAndName = new(
            "Copy ID and Name",
            "Copy the selected row ID and Name (will form a list if multiple).",
            Key.Q,
            false,
            true);

        // Mass Edit
        public KeyBind PARAM_ViewMassEdit = new(
            "View Mass Edit",
            "Trigger the Mass Edit prompt.",
            Key.Unknown);

        public KeyBind PARAM_ExecuteMassEdit = new(
            "Execute Mass Edit",
            "Execute the current Mass Edit input (if any).",
            Key.Q,
            true);

        // CSV
        public KeyBind PARAM_ImportCSV = new(
            "Import CSV",
            "Trigger the CSV Import prompt.",
            Key.Unknown);

        public KeyBind PARAM_ExportCSV = new(
            "Export CSV", 
            "Trigger the CSV Export prompt.",
            Key.Unknown);

        public KeyBind PARAM_ExportCSV_Names = new(
            "Export CSV - Export Row Names",
            "Export selected row names to window.",
            Key.Unknown);

        public KeyBind PARAM_ExportCSV_Param = new(
            "Export CSV - Export Param",
            "Export selected param to window.",
            Key.Unknown);

        public KeyBind PARAM_ExportCSV_AllRows = new(
            "Export CSV - Export All Rows",
            "Export selected all rows to window.",
            Key.Unknown);

        public KeyBind PARAM_ExportCSV_ModifiedRows = new(
            "Export CSV - Export Modified Rows",
            "Export selected modified rows to window.",
            Key.Unknown);

        public KeyBind PARAM_ExportCSV_SelectedRows = new(
            "Export CSV - Export Selected Rows",
            "Export selected selected rows to window.",
            Key.Unknown);

        // Row Namer
        public KeyBind PARAM_ApplyRowNamer = new(
            "Apply Row Namer (Flat)",
            "Apply the Row Namer to the current row selection, with the Flat configuration.",
            Key.I,
            true);

        // Param Reloader
        public KeyBind PARAM_ReloadParam = new(
            "Reload Current Param", 
            "Reloads the rows of the current Param selection in-game.", 
            Key.F5);

        public KeyBind PARAM_ReloadAllParams = new(
            "Reload All Params",
            "Reloads the rows of all Params in-game.",
            Key.F5, 
            false, 
            false, 
            true);

        // Pin Groups
        public KeyBind PARAM_CreateParamPinGroup = new(
            "Create Param Pin Group",
            "Create a new Param pin group from the currently pinned params.",
            Key.Unknown);

        public KeyBind PARAM_CreateRowPinGroup = new(
            "Create Row Pin Group",
            "Create a new Row pin group from the currently pinned rows.",
            Key.Unknown);

        public KeyBind PARAM_CreateFieldPinGroup = new(
            "Create Field Pin Group",
            "Create a new Field pin group from the currently pinned fields.",
            Key.Unknown,
            true);

        public KeyBind PARAM_ClearCurrentPinnedParams = new(
            "Clear Pinned Params",
            "Clear currently pinned params.",
            Key.Unknown);

        public KeyBind PARAM_ClearCurrentPinnedRows = new(
            "Clear Pinned Rows",
            "Clear currently pinned Rows.",
            Key.Unknown);

        public KeyBind PARAM_ClearCurrentPinnedFields = new(
            "Clear Pinned Fields",
            "Clear currently pinned Fields.",
            Key.Unknown);

        public KeyBind PARAM_OnlyShowPinnedParams = new(
            "Show Pinned Params Only",
            "Toggle the setting to show only pinned params in the param list.",
            Key.Unknown);

        public KeyBind PARAM_OnlyShowPinnedRows = new(
            "Show Pinned Rows Only",
            "Toggle the setting to show only pinned rows in the param list.",
            Key.Unknown);

        public KeyBind PARAM_OnlyShowPinnedFields = new(
            "Show Pinned Fields Only",
            "Toggle the setting to show only pinned fields in the param list.",
            Key.Unknown);

        public KeyBind PARAM_InheritReferencedRowName = new(
            "Inherit Referenced Row's Name",
            "Set the name of the selected row to the referenced row's name.",
            Key.F9);

        //-----------------------------
        // Text Editor
        //-----------------------------
        // Core
        public KeyBind TEXT_SelectAll = new(
            "Select All",
            "Select all rows.",
            Key.A,
            true,
            true);

        public KeyBind TEXT_Multiselect = new(
            "Multi-Select Row",
            "When held, multiple rows may be selected.",
            Key.Z);

        public KeyBind TEXT_MultiselectRange = new(
            "Multi-Select Row Range",
            "When held, the next row selected will be considered the 'start', and the next row after that the 'end'. All rows between them will be selected.",
            Key.LShift,
            false,
            false,
            false,
            true);

        public KeyBind TEXT_CopyEntrySelection = new(
            "Copy Selection",
            "Copy the selected Text Entries to the clipboard.",
            Key.C,
            true);

        public KeyBind TEXT_PasteEntrySelection = new(
            "Paste Selection",
            "Paste the saved Text Entries from the clipboard at the currently selected Text Entry.",
            Key.P,
            true);

        public KeyBind TEXT_CopyEntryContents = new(
            "Copy Entry Contents",
            "Copy the selected Text Entries contents to the clipboard.",
            Key.C,
            false,
            true);


        public KeyBind TEXT_FocusSelectedEntry = new(
            "Focus Selected Entry",
            "Focus the selected entry in the FMG entry list.",
            Key.F,
            false,
            true);

        //-----------------------------
        // GPARAM Editor
        //-----------------------------
        // Core
        public KeyBind GPARAM_ExecuteQuickEdit = new(
            "Execute Quick Edit Commands",
            "Execute the current quick edit commands.",
            Key.E,
            true);

        public KeyBind GPARAM_GenerateQuickEdit = new(
            "Generate Quick Edit Commands",
            "Generate quick edit commands from current selection.",
            Key.K,
            true);

        public KeyBind GPARAM_ClearQuickEdit = new(
            "Clear Quick Edit Commands",
            "Clear current quick edit commands.",
            Key.L,
            true);

        // Param Reloader
        public KeyBind GPARAM_ReloadParam = new(
            "Reload Current Gparam",
            "Reloads the conents of the current Gparam selection in-game.",
            Key.F5);

        //-----------------------------
        // Time Act Editor
        //-----------------------------
        // Core
        public KeyBind TIMEACT_Multiselect = new(
            "Multi-Select Row",
            "When held, multiple rows may be selected.",
            Key.Z);

        public KeyBind TIMEACT_MultiselectRange = new(
            "Multi-Select Row Range", 
            "When held, the next row selected will be considered the 'start', and the next row after that the 'end'. All rows between them will be selected.",
            Key.LShift, 
            false, 
            false, 
            false, 
            true);

        //-----------------------------
        // Texture Viewer
        //-----------------------------
        // Core
        public KeyBind TEXTURE_ExportTexture = new(
            "Export Texture", 
            "Export the currently viewed texture.", 
            Key.X, 
            true);

        public KeyBind TEXTURE_ZoomMode = new(
            "Zoom Mode", 
            "When held, the texture may be zoomed in/out with the mouse wheel.", 
            Key.LControl, 
            false, 
            false, 
            false, 
            true);

        public KeyBind TEXTURE_ResetZoomLevel = new(
            "Reset Zoom Level", 
            "Resets the zoom level to default.", 
            Key.R);

        //-----------------------------
        // Misc
        //-----------------------------
#pragma warning disable IDE0051
        // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
        [JsonExtensionData] internal IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051
    }
}
