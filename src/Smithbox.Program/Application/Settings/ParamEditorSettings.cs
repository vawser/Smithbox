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
///     The Param Editor's settings (<c>CFG.Current.ParamEditor</c>). Saved in Configuration.json under the same
///     ParamEditor_ keys as before they moved here.
/// </summary>
public sealed class ParamEditorSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// Param Editor
    ///------------------------------------------------------------
    // General
    public string ParamEditor_Annotation_Language = "English";
    public bool ParamEditor_Enable_Compact_Mode = false;
    public bool ParamEditor_Enable_Table_Borders = true;

    public string ParamEditor_Import_Language = "English";

    // Regulation
    public bool ParamEditor_Loose_Param_Mode_DS2 = false;
    public bool ParamEditor_Loose_Param_Mode_DS3 = false;
    public bool ParamEditor_Repack_Loose_Params_DS2 = false;
    public ParamSaveCompressionType ParamEditor_CompressionOverride = ParamSaveCompressionType.Default;

    // Param Upgrader: keep a local copy of the game's regulation.bin while no upgrade diff leads on from its version
    public bool ParamEditor_Upgrader_Keep_Regulation_Snapshots = true;

    public bool ParamEditor_Row_Name_Strip_DES = false;
    public bool ParamEditor_Row_Name_Strip_DS1 = false;
    public bool ParamEditor_Row_Name_Strip_DS2 = true;
    public bool ParamEditor_Row_Name_Strip_BB = false;
    public bool ParamEditor_Row_Name_Strip_DS3 = false;
    public bool ParamEditor_Row_Name_Strip_SDT = false;
    public bool ParamEditor_Row_Name_Strip_ER = false;
    public bool ParamEditor_Row_Name_Strip_AC6 = false;
    public bool ParamEditor_Row_Name_Strip_NR = false;
    public bool ParamEditor_Row_Name_Strip_DB = false;

    public bool ParamEditor_Stripped_Row_Name_Load_DES = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DS1 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DS2 = true;
    public bool ParamEditor_Stripped_Row_Name_Load_BB = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DS3 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_SDT = false;
    public bool ParamEditor_Stripped_Row_Name_Load_ER = false;
    public bool ParamEditor_Stripped_Row_Name_Load_AC6 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_NR = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DB = false;

    // Param List
    public bool ParamEditor_Param_List_Pinned_Stay_Visible = true;
    public bool ParamEditor_Param_List_Sort_Alphabetically = true;
    public bool ParamEditor_Param_List_Display_Community_Names = false;
    public bool ParamEditor_Param_List_Display_Categories = true;
    public bool ParamEditor_Param_List_Display_StayParams = true;
    public bool ParamEditor_Param_List_Display_FarModelParams = true;

    // Table List
    public bool ParamEditor_Display_Table_List = true;
    public bool ParamEditor_Table_List_Display_Group_Stats = true;
    public bool ParamEditor_Table_List_Search_Rows = false;

    // Row List
    public bool ParamEditor_Row_List_Pinned_Stay_Visible = true;
    public bool ParamEditor_Row_List_Enable_Line_Wrapping = true;
    public bool ParamEditor_Row_List_Enable_Row_Grouping = false;
    public bool ParamEditor_Row_List_Display_Decorators = true;
    public bool ParamEditor_Row_List_Display_Modified_Row_Bg = false;
    public bool ParamEditor_Row_List_Row_FMG_Prefer_Base = true;

    // Field List
    public ParamFieldNameMode ParamEditor_FieldNameMode = ParamFieldNameMode.Source;
    public bool ParamEditor_Field_List_Allow_Rearrangement = true;
    public bool ParamEditor_Field_List_Display_Offsets = false;
    public bool ParamEditor_Field_List_Pinned_Stay_Visible = true;
    public bool ParamEditor_Field_List_Display_Padding = true;
    public bool ParamEditor_Field_List_Display_Color_Picker = true;
    public bool ParamEditor_Field_List_Display_Graph = true;
    public bool ParamEditor_Field_List_Display_Map_Link = true;
    public bool ParamEditor_Field_List_Display_Model_Link = true;
    public bool ParamEditor_Field_List_Display_Decorators = true;
    public bool ParamEditor_Field_List_Display_Enums = true;
    public bool ParamEditor_Field_List_Display_References = true;
    public bool ParamEditor_Field_List_Display_Field_Attributes = true;
    public bool ParamEditor_Field_List_Display_Icon_Preview = true;
    public float ParamEditor_Field_List_Icon_Preview_Scale = 1.0f;
    public bool ParamEditor_Field_List_Display_Modified_Field_Bg = true;
    public bool ParamEditor_Field_List_GroupReference_DisplayCommunityName = true;

    public bool ParamEditor_Field_List_Enable_Field_Layouts = true;
    public bool ParamEditor_Field_List_Enable_Field_Layout_Category_Names = true;
    public bool ParamEditor_Field_List_Enable_Field_Layout_Chance_Hints = true;
    public FieldLayoutMode ParamEditor_Field_List_Field_Layout_Display_Type = FieldLayoutMode.Header;
    public FieldLayoutUnsortedPlacement ParamEditor_Field_List_Unsorted_Field_Placement = FieldLayoutUnsortedPlacement.Bottom;

    public ParamTooltipMode ParamEditor_Field_List_Tooltip_Mode = ParamTooltipMode.OnFieldName;

    // Mass Edit
    public bool ParamEditor_Show_Advanced_Mass_Edit_Commands = false;

    public bool ParamEditor_Field_Input_Display_Traditional_Percentage = false;

    // Integer fields: a number outside the field's range (type or bitfield) is clamped to the nearest limit when the
    // field is left, instead of being rejected
    public bool ParamEditor_Field_Input_Clamp_Out_Of_Range = false;

    public bool ParamEditor_Row_Context_Display_Row_Name_Input = true;
    public bool ParamEditor_Row_Context_Display_Shortcut_Tools = true;
    public bool ParamEditor_Row_Context_Display_Pin_Options = true;
    public bool ParamEditor_Row_Context_Display_Comparison_Options = true;
    public bool ParamEditor_Row_Context_Display_Reverse_Lookup = true;
    public bool ParamEditor_Row_Context_Display_Proliferate_Name = true;
    public bool ParamEditor_Row_Context_Display_Inherit_Name = true;
    public bool ParamEditor_Row_Context_Display_Row_Name_Tools = true;

    public bool ParamEditor_Field_Context_Split = false;
    public bool ParamEditor_Field_Context_Display_Field_Name = false;
    public bool ParamEditor_Field_Context_Display_Field_Description = false;
    public bool ParamEditor_Field_Context_Display_Field_Attributes = false;
    public bool ParamEditor_Field_Context_Display_Pin_Options = true;
    public bool ParamEditor_Field_Context_Display_Comparison_Options = true;
    public bool ParamEditor_Field_Context_Display_Field_Value_Distribution = true;
    public bool ParamEditor_Field_Context_Display_Searchbar_Options = true;
    public bool ParamEditor_Field_Context_Display_Reference_Search = true;
    public bool ParamEditor_Field_Context_Display_References = true;

    public bool ParamEditor_DeltaPatcher_Import_Display_All_Entries = false;
    public bool ParamEditor_DeltaPatcher_Import_Modified_Rows = true;
    public bool ParamEditor_DeltaPatcher_Import_Added_Rows = true;
    public bool ParamEditor_DeltaPatcher_Import_Deleted_Rows = true;

    public bool ParamEditor_DeltaPatcher_Import_Restrict_Row_Modify = false;
    public bool ParamEditor_DeltaPatcher_Import_Restrict_Row_Add = false;
    public bool ParamEditor_DeltaPatcher_Import_Allow_Row_Overwrite = false;

    public bool ParamEditor_DeltaPatcher_Export_Selected_Rows_Only = false;
    public bool ParamEditor_DeltaPatcher_Export_Ignore_Indexed_Rows = true;

    public List<string> ParamEditor_MassEdit_QuickCommands = new();
    public int ParamEditor_MassEdit_QuickCommands_PageCount = 10;

    public bool ParamEditor_Allow_Unmodified_Save = false;
    public bool ParamEditor_Allow_Automatic_Save = true;

    // Param_
    // Metadata (Param Editor)
    public bool Param_Editor_Enable_Param_Meta_Override = false;
    public bool Param_Editor_Enable_Param_Meta_Addition = true;

    public bool Param_Editor_Enable_Param_Enum_Override = false;
    public bool Param_Editor_Enable_Param_Enum_Addition = true;

    public bool Param_Editor_Enable_Param_Category_Override = false;
    public bool Param_Editor_Enable_Param_Category_Addition = true;

    public bool Param_Editor_Enable_Param_Commutativity_Group_Override = false;
    public bool Param_Editor_Enable_Param_Commutativity_Group_Addition = true;

    public bool Param_Editor_Enable_Param_Field_Reference_Group_Override = false;
    public bool Param_Editor_Enable_Param_Field_Reference_Group_Addition = true;

    public bool Param_Editor_Enable_Param_Field_Layout_Override = false;
    public bool Param_Editor_Enable_Field_Layout_Addition = true;

    public bool Param_Editor_Enable_Icon_Configuration_Override = false;
    public bool Param_Editor_Enable_Icon_Configuration_Addition = true;

    public bool Param_Editor_Enable_Graph_Annotation_Override = false;
    public bool Param_Editor_Enable_Graph_Annotation_Addition = true;

    public bool Param_Editor_Enable_Row_FMG_Annotation_Override = false;
    public bool Param_Editor_Enable_Row_FMG_Annotation_Addition = true;

    public bool Param_PasteAfterSelection = false;
    public bool Param_PasteThenSelect = true;
    public ParamRowCopyBehavior Param_RowCopyBehavior = ParamRowCopyBehavior.ID;

    public bool Param_ShowVanillaColumn = true;
    public bool Param_ShowAuxColumn = true;

    public bool Param_MassEdit_ShowAddButtons = true;

    public bool Param_FieldContextMenu_ImagePreview_ContextMenu = false;

    public int Param_Toolbar_Duplicate_Amount = 1;
    public int Param_Toolbar_Duplicate_Offset = 1;

    public string Param_Toolbar_CommutativeDuplicate_Target = "";
    public int Param_Toolbar_CommutativeDuplicate_Offset = 0;
    public bool Param_Toolbar_CommutativeDuplicate_ReplaceExistingRows = true;

    public bool Param_Toolbar_Duplicate_AffectAttackField = false;
    public bool Param_Toolbar_Duplicate_AffectBehaviorField = false;
    public bool Param_Toolbar_Duplicate_AffectBulletField = false;
    public bool Param_Toolbar_Duplicate_AffectSpEffectField = false;
    public bool Param_Toolbar_Duplicate_AffectSourceField = false;

    public bool Param_Toolbar_FindValueInstances_InitialMatchOnly = false;

    public bool Param_PinGroups_ShowOnlyPinnedParams = false;
    public bool Param_PinGroups_ShowOnlyPinnedRows = false;
    public bool Param_PinGroups_ShowOnlyPinnedFields = false;

    public bool Param_RowNameImport_ReplaceEmptyNamesOnly = false;

    public bool Param_TableGroupView_AllowDuplicateInject = false;

    public string Param_Import_Delimiter = ",";

    public bool Param_CSV_Ignore_Existing_Rows = false;
    public bool Param_CSV_Append_Only = false;
    public bool Param_CSV_Replace_Row = false;

    // ParamReloader_
    public bool ParamReloader_Use_Latest_Offset = true;
    public int ParamReloader_Current_Offsets = 0;
}
