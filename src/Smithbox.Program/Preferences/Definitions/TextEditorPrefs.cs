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
    public static PreferenceItem TextEditor_Primary_Language_DES()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.DES
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_DES);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_DES = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }

    public static PreferenceItem TextEditor_Primary_Language_DS1()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.DS1
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_DS1);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_DS1 = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_Primary_Language_DS1R()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.DS1R
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_DS1R);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_DS1R = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_Primary_Language_DS2()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.DS2
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_DS2);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_DS2 = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_Primary_Language_DS2S()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.DS2S
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_DS2S);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_DS2S = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_Primary_Language_BB()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.BB
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_BB);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_BB = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_Primary_Language_DS3()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.DS3
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_DS3);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_DS3 = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_Primary_Language_SDT()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.SDT
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_SDT);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_SDT = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_Primary_Language_ER()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.ER
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_ER);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_ER = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_Primary_Language_AC6()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.AC6
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_AC6);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_AC6 = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem TextEditor_Primary_Language_NR()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new List<ProjectType>()
            {
                ProjectType.NR
            },

            Section = SectionCategory.General,

            Title = "PREF_TextEditor_Primary_Language",
            Description = "PREF_TextEditor_Primary_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (curProject.Handler.TextData == null)
                    return;

                var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

                var curLanguage = languages.FirstOrDefault(e => e.Language == CFG.Current.TextEditor_Primary_Language_NR);

                var previewName = LOC.Get(curLanguage.LanguageKey);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages)
                    {
                        var displayName = LOC.Get(entry.LanguageKey);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.TextEditor_Primary_Language_NR = entry.Language;

                            // Refresh the param editor FMG decorators when the category changes.
                            if (curProject.Handler.ParamEditor != null)
                            {
                                var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;
                                activeView.RowDecorators.SetupFmgDecorators();
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

            Title = "PREF_TextEditor_Include_Vanilla_Cache",
            Description = "PREF_TextEditor_Include_Vanilla_Cache_TT",

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

            Title = "PREF_TextEditor_Container_List_Display_Obsolete_Containers",
            Description = "PREF_TextEditor_Container_List_Display_Obsolete_Containers_TT",

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

            Title = "PREF_TextEditor_Container_List_Display_Primary_Category_Only",
            Description = "PREF_TextEditor_Container_List_Display_Primary_Category_Only_TT",

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

            Title = "PREF_TextEditor_Container_List_Hide_Unused_Containers",
            Description = "PREF_TextEditor_Container_List_Hide_Unused_Containers_TT",

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

            Title = "PREF_TextEditor_Container_List_Display_Community_Names",
            Description = "PREF_TextEditor_Container_List_Display_Community_Names_TT",

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

            Title = "PREF_TextEditor_Container_List_Display_Source_Path",
            Description = "PREF_TextEditor_Container_List_Display_Source_Path_TT",

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

            Title = "PREF_TextEditor_Text_File_List_Grouped_Display",
            Description = "PREF_TextEditor_Text_File_List_Grouped_Display_TT",

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

            Title = "PREF_TextEditor_Text_File_List_Display_ID",
            Description = "PREF_TextEditor_Text_File_List_Display_ID_TT",

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

            Title = "PREF_TextEditor_Text_File_List_Display_Community_Names",
            Description = "PREF_TextEditor_Text_File_List_Display_Community_Names_TT",

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

            Title = "PREF_TextEditor_Text_Entry_List_Display_Null_Text",
            Description = "PREF_TextEditor_Text_Entry_List_Display_Null_Text_TT",

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

            Title = "PREF_TextEditor_Text_Entry_List_Truncate_Name",
            Description = "PREF_TextEditor_Text_Entry_List_Truncate_Name_TT",

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

            Title = "PREF_TextEditor_Text_Entry_List_Ignore_ID_Check",
            Description = "PREF_TextEditor_Text_Entry_List_Ignore_ID_Check_TT",

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

            Title = "PREF_TextEditor_Text_Entry_Enable_Grouped_Entries",
            Description = "PREF_TextEditor_Text_Entry_Enable_Grouped_Entries_TT",

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

            Title = "PREF_TextEditor_Text_Entry_Allow_Duplicate_ID",
            Description = "PREF_TextEditor_Text_Entry_Allow_Duplicate_ID_TT",

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

            Title = "PREF_TextEditor_Text_Export_Include_Grouped_Entries",
            Description = "PREF_TextEditor_Text_Export_Include_Grouped_Entries_TT",

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

            Title = "PREF_TextEditor_Text_Export_Enable_Quick_Export",
            Description = "PREF_TextEditor_Text_Export_Enable_Quick_Export_TT",

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

            Title = "PREF_TextEditor_Language_Sync_Display_Primary_Only",
            Description = "PREF_TextEditor_Language_Sync_Display_Primary_Only_TT",

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

            Title = "PREF_TextEditor_Language_Sync_Apply_Prefix",
            Description = "PREF_TextEditor_Language_Sync_Apply_Prefix_TT",

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

            Title = "PREF_TextEditor_Language_Sync_Prefix",
            Description = "PREF_TextEditor_Language_Sync_Prefix_TT",

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

            Title = "PREF_TextEditor_Text_Clipboard_Include_ID",
            Description = "PREF_TextEditor_Text_Clipboard_Include_ID_TT",

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

            Title = "PREF_TextEditor_Text_Clipboard_Escape_New_Lines",
            Description = "PREF_TextEditor_Text_Clipboard_Escape_New_Lines_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextEditor_Text_Clipboard_Escape_New_Lines);
            }
        };
    }

    #endregion
}
