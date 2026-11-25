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
///     The Text Editor's settings (<c>CFG.Current.TextEditor</c>), including each game's primary language. Saved in
///     Configuration.json under the same TextEditor_ keys as before they moved here.
/// </summary>
public sealed class TextEditorSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// Text Editor
    ///------------------------------------------------------------
    // Preferences
    public TextContainerCategory TextEditor_Primary_Category = TextContainerCategory.English;

    public string TextEditor_Primary_Language_DES = "English";
    public string TextEditor_Primary_Language_DS1 = "English";
    public string TextEditor_Primary_Language_DS1R = "English";
    public string TextEditor_Primary_Language_DS2 = "English";
    public string TextEditor_Primary_Language_DS2S = "English";
    public string TextEditor_Primary_Language_BB = "English";
    public string TextEditor_Primary_Language_DS3 = "English";
    public string TextEditor_Primary_Language_SDT = "English";
    public string TextEditor_Primary_Language_ER = "English";
    public string TextEditor_Primary_Language_AC6 = "English";
    public string TextEditor_Primary_Language_NR = "English";
    public string TextEditor_Primary_Language_DB_NT = "English";
    public string TextEditor_Primary_Language_DB = "English";

    public bool TextEditor_Include_Vanilla_Cache = true;

    public bool TextEditor_Container_List_Hide_Unused_Containers = true;
    public bool TextEditor_Container_List_Display_Obsolete_Containers = false;
    public bool TextEditor_Container_List_Display_Primary_Category_Only = false;
    public bool TextEditor_Container_List_Display_Community_Names = true;
    public bool TextEditor_Container_List_Display_Source_Path = true;

    public bool TextEditor_Text_File_List_Grouped_Display = true;
    public bool TextEditor_Text_File_List_Display_ID = false;
    public bool TextEditor_Text_File_List_Display_Community_Names = true;

    public bool TextEditor_Text_Entry_List_Display_Null_Text = true;
    public bool TextEditor_Text_Entry_List_Truncate_Name = true;
    public bool TextEditor_Text_Entry_List_Display_Null_Entries = true;
    public bool TextEditor_Text_Entry_List_Allow_Select_All = false;

    public bool TextEditor_Text_Entry_Enable_Grouped_Entries = true;
    public bool TextEditor_Text_Entry_Allow_Duplicate_ID = false;

    // Wrapped, markup-highlighted preview under each Contents text box
    public bool TextEditor_Contents_Show_Text_Preview = true;

    // Read-only reference column in the Contents panel: the same entry from vanilla, or from another language
    public bool TextEditor_Contents_Show_Reference = false;
    public bool TextEditor_Contents_Reference_Use_Vanilla = true;
    public TextContainerCategory TextEditor_Contents_Reference_Category = TextContainerCategory.English;

    // FMG Reloader (ER, Game Memory)
    public bool TextEditor_FmgReloader_AutoRefresh = true;
    public bool TextEditor_FmgReloader_SendOnSave = false;

    public bool TextEditor_Text_Clipboard_Escape_New_Lines = true;
    public bool TextEditor_Text_Clipboard_Include_ID = true;

    public int TextEditor_CreationModal_CreationCount = 1;
    public int TextEditor_CreationModal_IncrementCount = 1;
    public bool TextEditor_CreationModal_UseIncrementalNaming = false;
    public string TextEditor_CreationModal_IncrementalTitling_Prefix = "+";
    public string TextEditor_CreationModal_IncrementalTitling_Postfix = "";

    public bool TextEditor_CreationModal_UseTemplateNaming = false;
    public string TextEditor_CreationModal_IncrementalNaming_Template = "";

    public bool TextEditor_Text_Entry_List_Ignore_ID_Check = false;

    public bool TextEditor_Text_Export_Include_Grouped_Entries = true;
    public bool TextEditor_Text_Export_Enable_Quick_Export = false;
    public string TextEditor_TextExport_QuickExportPrefix = "quick_export";

    public bool TextEditor_Language_Sync_Display_Primary_Only = false;
    public bool TextEditor_Language_Sync_Apply_Prefix = true;
    public string TextEditor_Language_Sync_Prefix = "TRANSLATE: ";

    // Text Merge
    public string TextEditor_Merge_Languages = ""; // Comma-separated TextContainerCategory names; empty = primary language
    public int TextEditor_Merge_Conflict_Mode = 0; // TextMergeConflictMode: 0 Keep Mine, 1 Take Theirs, 2 Ask Each
    public int TextEditor_Merge_Source_Type = 0; // TextMergeSourceType: 0 Project, 1 Mod Folder, 2 JSON Export

    public bool TextEditor_LanguageSync_IncludeDefaultEntries = false;
    public bool TextEditor_LanguageSync_IncludeModifiedEntries = true;
    public bool TextEditor_LanguageSync_IncludeUniqueEntries = true;

    public bool TextEditor_Allow_Unmodified_Save = false;
    public bool TextEditor_Allow_Automatic_Save = true;

    // FMG_
    // OLD
    public string FMG_SearchAndReplace_SearchText = "";
    public string FMG_SearchAndReplace_ReplaceText = "";
    public bool FMG_SearchAndReplace_Regex_IgnoreCase = false;
    public bool FMG_SearchAndReplace_Regex_Multiline = false;
    public bool FMG_SearchAndReplace_Regex_Singleline = false;
    public bool FMG_SearchAndReplace_Regex_IgnorePatternWhitespace = false;
}
