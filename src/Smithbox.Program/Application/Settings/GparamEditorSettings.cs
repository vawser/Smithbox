using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ProjectileEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static StudioCore.Editors.MapEditor.DuplicateAction;

namespace StudioCore.Application;

/// <summary>
///     The Graphics Param Editor's settings (<c>CFG.Current.GparamEditor</c>). Saved in Configuration.json under the same GparamEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class GparamEditorSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// Graphics Param Editor
    ///------------------------------------------------------------
    public string GparamEditor_Annotation_Language = "English";

    public bool GparamEditor_File_List_Display_BB_BND_Files = true;
    public bool GparamEditor_File_List_Display_Aliases = true;

    public bool GparamEditor_Group_List_Display_Descriptions = true;
    public bool GparamEditor_Group_List_Display_Empty_Group = true;

    public bool GparamEditor_Field_List_Display_Descriptions = true;
    public bool GparamEditor_Highlight_Viewport_Fields = true;

    // Gparam Reloader (Game Memory)
    public bool GparamEditor_Reloader_AutoRefresh = true;

    public bool GparamEditor_Value_List_Display_Color_Edit_V4 = true;
    public bool GparamEditor_Value_List_Display_Time_Of_Day_Column = true;
    public bool GparamEditor_Value_List_Display_Information_Column = true;

    public ColorEditDisplayMode GparamEditor_Color_Edit_Mode = ColorEditDisplayMode.RGB;

    // Transfer: the folder the last export or import used, where the file dialogs open
    public string GparamEditor_Transfer_Last_Folder = "";

    // Transfer: the paste mode for each payload kind (see GparamPasteModes)
    public GparamFilePasteMode GparamEditor_Transfer_File_Mode = GparamFilePasteMode.ReplaceAll;
    public GparamKeyedPasteMode GparamEditor_Transfer_Group_Mode = GparamKeyedPasteMode.MatchKey;
    public GparamKeyedPasteMode GparamEditor_Transfer_Field_Mode = GparamKeyedPasteMode.MatchKey;
    public GparamValuePasteMode GparamEditor_Transfer_Values_Mode = GparamValuePasteMode.InsertAfter;

    // Merge (Claude/Documentation/Gparam.md §7.4): the source kind (GparamMergeSourceType), the starting
    // choice for conflicts (GparamMergeConflictMode), whether Removed by Them items start ticked, and the folder the
    // last mod folder or file was picked from
    public int GparamEditor_Merge_Source_Type = 0;
    public int GparamEditor_Merge_Conflict_Mode = 0;
    public bool GparamEditor_Merge_Include_Removals = false;
    public string GparamEditor_Merge_Last_Folder = "";

    // Viewport: the map shown is the selected GPARAM's (or the one chosen), lit by the editor's copy of the GPARAM.
    // The time of day and weather are the Map Editor's (Viewport Rendering: Map Lighting), unless following the value.
    public bool GparamEditor_Viewport_Auto_Load_Map = false;
    public bool GparamEditor_Viewport_Load_Assets = true;
    public bool GparamEditor_Viewport_Follow_Value_Time = false;

    public bool GparamEditor_Allow_Unmodified_Save = false;
    public bool GparamEditor_Allow_Automatic_Save = true;

    public ViewportGridSet GparamEditor_Grids = new();
}
