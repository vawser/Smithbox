using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class TextEditorPrefs
{
    public static Type GetPrefType()
    {
        return typeof(TextEditorPrefs);
    }

    #region
    public static PreferenceItem TextEditor_Primary_Category()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "Primary Category",
            Description = "Determines which language is considered the 'primary' language.",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", CFG.Current.TextEditor_Primary_Category.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(TextContainerCategory)))
                    {
                        var type = (TextContainerCategory)entry;

                        if (TextUtils.IsSupportedLanguage(curProject, (TextContainerCategory)entry))
                        {
                            if (ImGui.Selectable(type.GetDisplayName()))
                            {
                                CFG.Current.TextEditor_Primary_Category = (TextContainerCategory)entry;

                                // Refresh the param editor FMG decorators when the category changes.
                                if (curProject.Handler.ParamEditor != null)
                                {
                                    var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                    activeView.RowDecorators.SetupFmgDecorators();
                                }
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_IncludeVanillaCache()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "Enable Difference Checker",
            Description = "If enabled, unique and modified rows will be highlighted.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Include_Vanilla_Cache);
            }
        };
    }

    #endregion

    #region Container List
    public static PreferenceItem TextEditor_Container_List_Display_Obsolete_Containers()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Container_List,

            Title = "Display Obsolete Containers",
            Description = "If enabled, obsolete containers will be displayed in the list. These are containers the game (for the current project type) no longer reads.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Container_List_Display_Obsolete_Containers);
            }
        };
    }
    public static PreferenceItem TextEditor_Container_List_Display_Primary_Category_Only()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Container_List,

            Title = "Display Primary Category Only",
            Description = "If enabled, only the primary category containers are displayed in the list.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Container_List_Display_Primary_Category_Only);
            }
        };
    }
    public static PreferenceItem TextEditor_Container_List_Hide_Unused_Containers()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Container_List,

            Title = "Hide Unused Containers",
            Description = "If enabled, unused containers are no longer displayed in the list.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Container_List_Hide_Unused_Containers);
            }
        };
    }
    public static PreferenceItem TextEditor_Container_List_Display_Community_Names()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Container_List,

            Title = "Display Community Names",
            Description = "If enabled, the community names for containers are used instead of their raw filenames.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Container_List_Display_Community_Names);
            }
        };
    }
    public static PreferenceItem TextEditor_Container_List_Display_Source_Path()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Container_List,

            Title = "Display Source Path",
            Description = "If enabled, the source path for a container will be displayed withi its tooltip.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Container_List_Display_Source_Path);
            }
        };
    }

    #endregion

    #region Text File List
    public static PreferenceItem TextEditor_Text_File_List_Grouped_Display()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_File_List,

            Title = "Enable Grouped Display",
            Description = "If enabled, non-title files that belong to a entry group will be hidden.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_File_List_Grouped_Display);
            }
        };
    }
    public static PreferenceItem TextEditor_Text_File_List_Display_ID()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_File_List,

            Title = "Display ID",
            Description = "If enabled, the FMG ID of the text file is listed with its name.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_File_List_Display_ID);
            }
        };
    }
    public static PreferenceItem TextEditor_Text_File_List_Display_Community_Names()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_File_List,

            Title = "Display Community Names",
            Description = "If enabled, the community names for a text file are displayed instead of its raw filename.",

            Draw = () => {
                ImGui.Checkbox("##inputValue2", ref CFG.Current.TextEditor_Text_File_List_Display_Community_Names);
            }
        };
    }
    #endregion

    #region Text Entry List
    public static PreferenceItem TextEditor_Text_Entry_List_Display_Null_Text()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_Entry_List,

            Title = "Display Null",
            Description = "If enabled, null text will be represented with the '<null>' text.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Entry_List_Display_Null_Text);
            }
        };
    }
    public static PreferenceItem TextEditor_Text_Entry_List_Truncate_Name()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_Entry_List,

            Title = "Truncate Name",
            Description = "If enabled, the entry name will be truncated if it is too long.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Entry_List_Truncate_Name);
            }
        };
    }
    public static PreferenceItem TextEditor_Text_Entry_List_Ignore_ID_Check()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_Entry_List,

            Title = "Ignore ID Check on Duplicate",
            Description = "If enabled, duplicate will produce entries with the same entry ID as the source.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Entry_List_Ignore_ID_Check);
            }
        };
    }

    #endregion

    #region Text Entries
    public static PreferenceItem TextEditor_Text_Entry_Enable_Grouped_Entries()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_Entries,

            Title = "Enable Grouped Entries",
            Description = "If enabled, entries that form a Title, Summary, Description and Effect group will display all entries within the Text Entry window.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Entry_Enable_Grouped_Entries);
            }
        };
    }
    public static PreferenceItem TextEditor_Text_Entry_Allow_Duplicate_ID()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_Entries,

            Title = "Ignore ID Check",
            Description = "If enabled, an ID change that would result in a duplicate ID will be permitted.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Entry_Allow_Duplicate_ID);
            }
        };
    }

    #endregion

    #region Text Export
    public static PreferenceItem TextEditor_Text_Export_Include_Grouped_Entries()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_Export,

            Title = "Export Grouped Entries",
            Description = "If enabled, the other memebers of a grouped source entry will be included upon exporting.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Export_Include_Grouped_Entries);
            }
        };
    }
    public static PreferenceItem TextEditor_Text_Export_Enable_Quick_Export()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_Export,

            Title = "Enable Quick Export",
            Description = "If enabled, the export file is automatically named instead of displaying the Export Text prompt for the user. Will overwrite the existing quick export file each time.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Export_Enable_Quick_Export);
            }
        };
    }

    #endregion

    #region Language Sync
    public static PreferenceItem TextEditor_Language_Sync_Display_Primary_Only()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Language_Sync,

            Title = "Display Primary Only",
            Description = "If enabled, only show your primary categoryin the selection dropdown.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Language_Sync_Display_Primary_Only);
            }
        };
    }
    public static PreferenceItem TextEditor_Language_Sync_Apply_Prefix()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Language_Sync,

            Title = "Apply Prefix to Synced Text",
            Description = "If enabled, a prefix is added to all text in the synced language container.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Language_Sync_Apply_Prefix);
            }
        };
    }
    public static PreferenceItem TextEditor_Language_Sync_Prefix()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.TextEditor_Language_Sync,

            Title = "Prefix to Apply",
            Description = "The prefix that is added to synced text.",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.TextEditor_Language_Sync_Prefix, 255);
            }
        };
    }

    #endregion

    #region Text Clipboard
    public static PreferenceItem TextEditor_Text_Clipboard_Include_ID()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_Clipboard,

            Title = "Include ID",
            Description = "If enabled, the entry ID is included in the saved clipboard text.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Clipboard_Include_ID);
            }
        };
    }
    public static PreferenceItem TextEditor_Text_Clipboard_Escape_New_Lines()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,

            Section = SectionCategory.TextEditor_Text_Clipboard,

            Title = "Escape New Lines",
            Description = "If enabled, new lines are escaped so the saved clipboard text does not contain actually new lines.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Clipboard_Escape_New_Lines);
            }
        };
    }

    #endregion
}
