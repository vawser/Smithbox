using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class ParamEditorPrefs
{
    public static Type GetPrefType()
    {
        return typeof(ParamEditorPrefs);
    }

    #region General
    public static PreferenceItem ParamEditor_Annotation_Language()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "PREF_ParamEditor_Annotation_Language",
            Description = "PREF_ParamEditor_Annotation_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                var languages = curProject.Handler.ParamData.ParamAnnotationLanguages;

                var curLanguage = languages.Languages.FirstOrDefault(e => e.Name == CFG.Current.ParamEditor_Annotation_Language);

                var previewName = LOC.Get(curLanguage.Key);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages.Languages)
                    {
                        var displayName = LOC.Get(entry.Key);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.ParamEditor_Annotation_Language = entry.Name;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem ParamEditor_Import_Language()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "PREF_ParamEditor_Import_Language",
            Description = "PREF_ParamEditor_Import_Language_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                var languages = curProject.Handler.ParamData.RowImportLanguages;

                var curLanguage = languages.Options.FirstOrDefault(e => e.Name == CFG.Current.ParamEditor_Import_Language);

                var previewName = LOC.Get(curLanguage.Key);

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in languages.Options)
                    {
                        var displayName = LOC.Get(entry.Key);

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.ParamEditor_Import_Language = entry.Name;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }

    public static PreferenceItem ParamEditor_Enable_Compact_Mode()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_ParamEditor_Enable_Compact_Mode",
            Description = "PREF_ParamEditor_Enable_Compact_Mode_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Enable_Compact_Mode);
            }
        };
    }

    public static PreferenceItem ParamEditor_Enable_Table_Borders()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_ParamEditor_Enable_Table_Borders",
            Description = "PREF_ParamEditor_Enable_Table_Borders_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Enable_Table_Borders);
            }
        };
    }
    #endregion

    #region Regulation

    public static PreferenceItem ParamEditor_Loose_Param_Mode_DS2()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DS2,
                ProjectType.DS2S
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Loose_Param_Mode_DS2",
            Description = "PREF_ParamEditor_Loose_Param_Mode_DS2_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Loose_Param_Mode_DS2);
            }
        };
    }
    public static PreferenceItem ParamEditor_Repack_Loose_Params_DS2()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DS2,
                ProjectType.DS2S
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Repack_Loose_Params_DS2",
            Description = "PREF_ParamEditor_Repack_Loose_Params_DS2_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Repack_Loose_Params_DS2);
            }
        };
    }

    public static PreferenceItem ParamEditor_Loose_Param_Mode_DS3()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DS3
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Loose_Param_Mode_DS3",
            Description = "PREF_ParamEditor_Loose_Param_Mode_DS3_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Loose_Param_Mode_DS3);
            }
        };
    }

    public static PreferenceItem ParamEditor_Row_Name_Strip_DES()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DES
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Row_Name_Strip_DES",
            Description = "PREF_ParamEditor_Row_Name_Strip_DES_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Name_Strip_DES);
            }
        };
    }

    public static PreferenceItem ParamEditor_Row_Name_Strip_DS1()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DS1,
                ProjectType.DS1R
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Row_Name_Strip_DS1",
            Description = "PREF_ParamEditor_Row_Name_Strip_DS1_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Name_Strip_DS1);
            }
        };
    }

    public static PreferenceItem ParamEditor_Row_Name_Strip_DS2()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DS2,
                ProjectType.DS2S
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Row_Name_Strip_DS2",
            Description = "PREF_ParamEditor_Row_Name_Strip_DS2_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Name_Strip_DS2);
            }
        };
    }

    public static PreferenceItem ParamEditor_Row_Name_Strip_BB()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.BB
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Row_Name_Strip_BB",
            Description = "PREF_ParamEditor_Row_Name_Strip_BB_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Name_Strip_BB);
            }
        };
    }

    public static PreferenceItem ParamEditor_Row_Name_Strip_DS3()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DS3
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Row_Name_Strip_DS3",
            Description = "PREF_ParamEditor_Row_Name_Strip_DS3_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Name_Strip_DS3);
            }
        };
    }

    public static PreferenceItem ParamEditor_Row_Name_Strip_SDT()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.SDT
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Row_Name_Strip_SDT",
            Description = "PREF_ParamEditor_Row_Name_Strip_SDT_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Name_Strip_SDT);
            }
        };
    }

    public static PreferenceItem ParamEditor_Row_Name_Strip_ER()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.ER
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Row_Name_Strip_ER",
            Description = "PREF_ParamEditor_Row_Name_Strip_ER_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Name_Strip_ER);
            }
        };
    }

    public static PreferenceItem ParamEditor_Row_Name_Strip_AC6()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.AC6
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Row_Name_Strip_AC6e",
            Description = "PREF_ParamEditor_Row_Name_Strip_AC6_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Name_Strip_AC6);
            }
        };
    }

    public static PreferenceItem ParamEditor_Row_Name_Strip_NR()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.NR
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Row_Name_Strip_NR",
            Description = "PREF_ParamEditor_Row_Name_Strip_NR_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Name_Strip_NR);
            }
        };
    }

    public static PreferenceItem ParamEditor_Stripped_Row_Name_Load_DES()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DES
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Stripped_Row_Name_Load_DES",
            Description = "PREF_ParamEditor_Stripped_Row_Name_Load_DES_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_DES);
            }
        };
    }

    public static PreferenceItem ParamEditor_Stripped_Row_Name_Load_DS1()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DS1,
                ProjectType.DS1R
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Stripped_Row_Name_Load_DS1",
            Description = "PREF_ParamEditor_Stripped_Row_Name_Load_DS1_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_DS1);
            }
        };
    }

    public static PreferenceItem ParamEditor_Stripped_Row_Name_Load_DS2()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DS2,
                ProjectType.DS2S
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Stripped_Row_Name_Load_DS2",
            Description = "PREF_ParamEditor_Stripped_Row_Name_Load_DS2_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_DS2);
            }
        };
    }

    public static PreferenceItem ParamEditor_Stripped_Row_Name_Load_BB()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.BB
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Stripped_Row_Name_Load_BB",
            Description = "PREF_ParamEditor_Stripped_Row_Name_Load_BB_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_BB);
            }
        };
    }

    public static PreferenceItem ParamEditor_Stripped_Row_Name_Load_DS3()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.DS3
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Stripped_Row_Name_Load_DS3",
            Description = "PREF_ParamEditor_Stripped_Row_Name_Load_DS3_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_DS3);
            }
        };
    }

    public static PreferenceItem ParamEditor_Stripped_Row_Name_Load_SDT()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.SDT
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Stripped_Row_Name_Load_SDT",
            Description = "PREF_ParamEditor_Stripped_Row_Name_Load_SDT_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_SDT);
            }
        };
    }

    public static PreferenceItem ParamEditor_Stripped_Row_Name_Load_ER()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.ER
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Stripped_Row_Name_Load_ER",
            Description = "PREF_ParamEditor_Stripped_Row_Name_Load_ER_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_ER);
            }
        };
    }

    public static PreferenceItem ParamEditor_Stripped_Row_Name_Load_AC6()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.AC6
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Stripped_Row_Name_Load_AC6",
            Description = "PREF_ParamEditor_Stripped_Row_Name_Load_AC6_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_AC6);
            }
        };
    }

    public static PreferenceItem ParamEditor_Stripped_Row_Name_Load_NR()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            DisplayRestrictions = new()
            {
                ProjectType.NR
            },

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_Stripped_Row_Name_Load_NR",
            Description = "PREF_ParamEditor_Stripped_Row_Name_Load_NR_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_NR);
            }
        };
    }
    public static PreferenceItem ParamEditor_CompressionOverride()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ParamEditor_Regulation,

            Title = "PREF_ParamEditor_CompressionOverride",
            Description = "PREF_ParamEditor_CompressionOverride_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", CFG.Current.ParamEditor_CompressionOverride.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(ParamSaveCompressionType)))
                    {
                        var type = (ParamSaveCompressionType)entry;

                        if (ImGui.Selectable(type.GetDisplayName()))
                        {
                            CFG.Current.ParamEditor_CompressionOverride = (ParamSaveCompressionType)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }

    #endregion

    #region Param Reloader
    public static PreferenceItem UseLatestGameOffset()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            DisplayRestrictions = new()
            {
                ProjectType.DS1,
                ProjectType.DS1R,
                ProjectType.DS3,
                ProjectType.SDT,
                ProjectType.ER,
                ProjectType.AC6,
                ProjectType.NR
            },

            Section = SectionCategory.ParamEditor_Param_Reloader,

            Title = "PREF_ParamReloader_Use_Latest_Offset",
            Description = "PREF_ParamReloader_Use_Latest_Offset_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamReloader_Use_Latest_Offset);
            }
        };
    }

    public static PreferenceItem SelectedGameOffsetData()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,
            DisplayRestrictions = new()
            {
                ProjectType.DS1,
                ProjectType.DS1R,
                ProjectType.DS3,
                ProjectType.SDT,
                ProjectType.ER,
                ProjectType.AC6,
                ProjectType.NR
            },

            Section = SectionCategory.ParamEditor_Param_Reloader,

            Title = "PREF_ParamReloader_Current_Offsets",
            Description = "PREF_ParamReloader_Current_Offsets_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                var paramData = curProject.Handler.ParamData;

                var index = CFG.Current.ParamReloader_Current_Offsets;

                if (curProject.Descriptor.ProjectType is ProjectType.Undefined)
                    return;

                if (paramData == null)
                    return;

                if (paramData.ParamReloaderOffsets == null)
                    return;

                if (paramData.ParamReloaderOffsets.Groups == null)
                    return;

                string[] options = curProject.Handler.ParamData.ParamReloaderOffsets.Groups.Select(entry => entry.exeVersion).ToArray();

                DPI.ApplyInputWidth();
                if (ImGui.Combo("##GameOffsetVersion", ref index, options, options.Length))
                {
                    CFG.Current.ParamReloader_Current_Offsets = index;
                }
            }
        };
    }

    #endregion

    #region Param List
    public static PreferenceItem ParamEditor_Param_List_Pinned_Stay_Visible()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Param_List,

            Title = "PREF_ParamEditor_Param_List_Pinned_Stay_Visible",
            Description = "PREF_ParamEditor_Param_List_Pinned_Stay_Visible_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Param_List_Pinned_Stay_Visible);
            }
        };
    }
    public static PreferenceItem ParamEditor_Param_List_Sort_Alphabetically()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Param_List,

            Title = "PREF_ParamEditor_Param_List_Sort_Alphabetically",
            Description = "PREF_ParamEditor_Param_List_Sort_Alphabetically_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Param_List_Sort_Alphabetically);
            }
        };
    }
    public static PreferenceItem ParamEditor_Param_List_Display_Community_Names()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Param_List,

            Title = "PREF_ParamEditor_Param_List_Display_Community_Names",
            Description = "PREF_ParamEditor_Param_List_Display_Community_Names_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue_param", ref CFG.Current.ParamEditor_Param_List_Display_Community_Names);
            }
        };
    }
    public static PreferenceItem ParamEditor_Param_List_Display_Categories()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Param_List,

            Title = "PREF_ParamEditor_Param_List_Display_Categories",
            Description = "PREF_ParamEditor_Param_List_Display_Categories_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Param_List_Display_Categories);
            }
        };
    }
    #endregion

    #region Row List
    public static PreferenceItem ParamEditor_Row_List_Pinned_Stay_Visible()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_List,

            Title = "PREF_ParamEditor_Row_List_Pinned_Stay_Visible",
            Description = "PREF_ParamEditor_Row_List_Pinned_Stay_Visible_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue_row", ref CFG.Current.ParamEditor_Row_List_Pinned_Stay_Visible);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_List_Enable_Line_Wrapping()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_List,

            Title = "PREF_ParamEditor_Row_List_Enable_Line_Wrapping",
            Description = "PREF_ParamEditor_Row_List_Enable_Line_Wrapping_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_List_Enable_Line_Wrapping);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_List_Enable_Row_Grouping()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_List,

            Title = "PREF_ParamEditor_Row_List_Enable_Row_Grouping",
            Description = "PREF_ParamEditor_Row_List_Enable_Row_Grouping_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_List_Enable_Row_Grouping);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_List_Display_Decorators()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_List,

            Title = "PREF_ParamEditor_Row_List_Display_Decorators",
            Description = "PREF_ParamEditor_Row_List_Display_Decorators_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue_row", ref CFG.Current.ParamEditor_Row_List_Display_Decorators);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_List_Row_FMG_Prefer_Base()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_List,

            Title = "PREF_ParamEditor_Row_List_Row_FMG_Prefer_Base",
            Description = "PREF_ParamEditor_Row_List_Row_FMG_Prefer_Base_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_List_Row_FMG_Prefer_Base);
            }
        };
    }

    #endregion

    #region Field List
    public static PreferenceItem ParamEditor_Field_List_Pinned_Stay_Visible()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Pinned_Stay_Visible",
            Description = "PREF_ParamEditor_Field_List_Pinned_Stay_Visible_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue_field", ref CFG.Current.ParamEditor_Field_List_Pinned_Stay_Visible);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Allow_Reordering()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Allow_Rearrangement",
            Description = "PREF_ParamEditor_Field_List_Allow_Rearrangement_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Allow_Rearrangement);
            }
        };
    }
    public static PreferenceItem ParamEditor_FieldNameMode()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_FieldNameMode",
            Description = "PREF_ParamEditor_FieldNameMode_TT",

            Draw = () => {
                DPI.ApplyInputWidth();

                var previewName = LOC.Get(CFG.Current.ParamEditor_FieldNameMode.GetDisplayName());

                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in Enum.GetValues(typeof(ParamFieldNameMode)))
                    {
                        var type = (ParamFieldNameMode)entry;

                        var displayName = LOC.Get(type.GetDisplayName());

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.ParamEditor_FieldNameMode = (ParamFieldNameMode)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Display_Offsets()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Offsets",
            Description = "PREF_ParamEditor_Field_List_Display_Offsets_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Display_Offsets);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Display_Color_Picker()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Color_Picker",
            Description = "PREF_ParamEditor_Field_List_Display_Color_Picker_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Display_Color_Picker);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Display_Graph()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Graph",
            Description = "PREF_ParamEditor_Field_List_Display_Graph_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Display_Graph);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Display_Map_Link()
    {
        return new PreferenceItem
        {
            OrderID = 6,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Map_Link",
            Description = "PREF_ParamEditor_Field_List_Display_Map_Link_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Display_Map_Link);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Display_Model_Link()
    {
        return new PreferenceItem
        {
            OrderID = 7,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Model_Link",
            Description = "PREF_ParamEditor_Field_List_Display_Model_Link_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Display_Model_Link);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Display_Padding()
    {
        return new PreferenceItem
        {
            OrderID = 8,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Padding",
            Description = "PREF_ParamEditor_Field_List_Display_Padding_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Display_Padding);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Display_Decorators()
    {
        return new PreferenceItem
        {
            OrderID = 9,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Decorators",
            Description = "PREF_ParamEditor_Field_List_Display_Decorators_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue_field", ref CFG.Current.ParamEditor_Field_List_Display_Decorators);
            }
        };
    }

    public static PreferenceItem ParamEditor_Field_List_Display_References()
    {
        return new PreferenceItem
        {
            OrderID = 10,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_References",
            Description = "PREF_ParamEditor_Field_List_Display_References_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue_field", ref CFG.Current.ParamEditor_Field_List_Display_References);
            }
        };
    }

    public static PreferenceItem ParamEditor_Field_List_Display_Enums()
    {
        return new PreferenceItem
        {
            OrderID = 11,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Enums",
            Description = "PREF_ParamEditor_Field_List_Display_Enums_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue_field", ref CFG.Current.ParamEditor_Field_List_Display_Enums);
            }
        };
    }

    public static PreferenceItem ParamEditor_Field_List_Display_Field_Attributes()
    {
        return new PreferenceItem
        {
            OrderID = 12,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Field_Attributes",
            Description = "PREF_ParamEditor_Field_List_Display_Field_Attributes_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Display_Field_Attributes);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Display_Icon_Preview()
    {
        return new PreferenceItem
        {
            OrderID = 13,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Display_Icon_Preview",
            Description = "PREF_ParamEditor_Field_List_Display_Icon_Preview_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Display_Icon_Preview);
            }
        };
    }
    public static PreferenceItem Param_FieldContextMenu_ImagePreviewScale()
    {
        return new PreferenceItem
        {
            OrderID = 14,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Icon_Preview_Scale",
            Description = "PREF_ParamEditor_Field_List_Icon_Preview_Scale_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.DragFloat("##inputValue", ref CFG.Current.ParamEditor_Field_List_Icon_Preview_Scale, 0.1f, 0.1f, 10.0f);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Tooltip_Mode()
    {
        return new PreferenceItem
        {
            OrderID = 15,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_Tooltip_Mode",
            Description = "PREF_ParamEditor_Field_List_Tooltip_Mode_TT",

            Draw = () => {
                DPI.ApplyInputWidth();

                var previewName = LOC.Get(CFG.Current.ParamEditor_Field_List_Tooltip_Mode.GetDisplayName());

                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in Enum.GetValues(typeof(ParamTooltipMode)))
                    {
                        var type = (ParamTooltipMode)entry;

                        var displayName = LOC.Get(type.GetDisplayName());

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.ParamEditor_Field_List_Tooltip_Mode = (ParamTooltipMode)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_GroupReference_DisplayCommunityName()
    {
        return new PreferenceItem
        {
            OrderID = 16,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "PREF_ParamEditor_Field_List_GroupReference_DisplayCommunityName",
            Description = "PREF_ParamEditor_Field_List_GroupReference_DisplayCommunityName_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_GroupReference_DisplayCommunityName);
            }
        };
    }

    #endregion

    #region Field Layouts

    public static PreferenceItem ParamEditor_Field_List_Enable_Field_Grouping()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Layouts,

            Title = "PREF_ParamEditor_Field_List_Enable_Field_Layouts",
            Description = "PREF_ParamEditor_Field_List_Enable_Field_Layouts_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Enable_Field_Layouts);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Enable_Field_Layout_Category_Names()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Layouts,

            Title = "PREF_ParamEditor_Field_List_Enable_Field_Layout_Category_Names",
            Description = "PREF_ParamEditor_Field_List_Enable_Field_Layout_Category_Names_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Enable_Field_Layout_Category_Names);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Enable_Field_Layout_Chance_Hints()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Layouts,

            Title = "PREF_ParamEditor_Field_List_Enable_Field_Layout_Chance_Hints",
            Description = "PREF_ParamEditor_Field_List_Enable_Field_Layout_Chance_Hints_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Enable_Field_Layout_Chance_Hints);
            }
        };
    }

    public static PreferenceItem ParamEditor_Field_List_Enable_Field_Group_Type()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ParamEditor_Field_Layouts,

            Title = "PREF_ParamEditor_Field_List_Field_Layout_Display_Type",
            Description = "PREF_ParamEditor_Field_List_Field_Layout_Display_Type_TT",

            Draw = () => {
                DPI.ApplyInputWidth();

                var previewName = LOC.Get(CFG.Current.ParamEditor_Field_List_Field_Layout_Display_Type.GetDisplayName());

                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in Enum.GetValues(typeof(FieldLayoutMode)))
                    {
                        var type = (FieldLayoutMode)entry;

                        var displayName = LOC.Get(type.GetDisplayName());

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.ParamEditor_Field_List_Field_Layout_Display_Type = (FieldLayoutMode)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Unsorted_Field_Placement()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ParamEditor_Field_Layouts,

            Title = "PREF_ParamEditor_Field_List_Unsorted_Field_Placement",
            Description = "PREF_ParamEditor_Field_List_Unsorted_Field_Placement_TT",

            Draw = () => {
                DPI.ApplyInputWidth();

                var previewName = LOC.Get(CFG.Current.ParamEditor_Field_List_Unsorted_Field_Placement.GetDisplayName());

                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in Enum.GetValues(typeof(FieldLayoutUnsortedPlacement)))
                    {
                        var type = (FieldLayoutUnsortedPlacement)entry;

                        var displayName = LOC.Get(type.GetDisplayName());

                        if (ImGui.Selectable(displayName))
                        {
                            CFG.Current.ParamEditor_Field_List_Unsorted_Field_Placement = (FieldLayoutUnsortedPlacement)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
        };
    }
    #endregion

    #region Field Input
    public static PreferenceItem ParamEditor_Field_Input_Display_Traditional_Percentage()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Input,

            Title = "PREF_ParamEditor_Field_Input_Display_Traditional_Percentage",
            Description = "PREF_ParamEditor_Field_Input_Display_Traditional_Percentage_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Input_Display_Traditional_Percentage);
            }
        };
    }
    #endregion

    #region Row Context Menu
    public static PreferenceItem ParamEditor_Row_Context_Display_Row_Name_Input()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "PREF_ParamEditor_Row_Context_Display_Row_Name_Input",
            Description = "PREF_ParamEditor_Row_Context_Display_Row_Name_Input_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Row_Name_Input);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_Context_Display_Shortcut_Tools()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "PREF_ParamEditor_Row_Context_Display_Shortcut_Tools",
            Description = "PREF_ParamEditor_Row_Context_Display_Shortcut_Tools_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Shortcut_Tools);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_Context_Display_Pin_Options()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "PREF_ParamEditor_Row_Context_Display_Pin_Options",
            Description = "PREF_ParamEditor_Row_Context_Display_Pin_Options_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Pin_Options);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_Context_Display_Comparison_Options()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "PREF_ParamEditor_Row_Context_Display_Comparison_Options",
            Description = "PREF_ParamEditor_Row_Context_Display_Comparison_Options_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Comparison_Options);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_Context_Display_Reverse_Lookup()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "PREF_ParamEditor_Row_Context_Display_Reverse_Lookup",
            Description = "PREF_ParamEditor_Row_Context_Display_Reverse_Lookup_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Reverse_Lookup);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_Context_Display_Proliferate_Name()
    {
        return new PreferenceItem
        {
            OrderID = 6,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "PREF_ParamEditor_Row_Context_Display_Proliferate_Name",
            Description = "PREF_ParamEditor_Row_Context_Display_Proliferate_Name_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Proliferate_Name);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_Context_Display_Inherit_Name()
    {
        return new PreferenceItem
        {
            OrderID = 7,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "PREF_ParamEditor_Row_Context_Display_Inherit_Name",
            Description = "PREF_ParamEditor_Row_Context_Display_Inherit_Name_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Inherit_Name);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_Context_Display_Row_Name_Tools()
    {
        return new PreferenceItem
        {
            OrderID = 8,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "PREF_ParamEditor_Row_Context_Display_Row_Name_Tools",
            Description = "PREF_ParamEditor_Row_Context_Display_Row_Name_Tools_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Row_Name_Tools);
            }
        };
    }

    #endregion

    #region Field Context Menu
    public static PreferenceItem ParamEditor_Field_Context_Display_Field_Name()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "PREF_ParamEditor_Field_Context_Display_Field_Name",
            Description = "PREF_ParamEditor_Field_Context_Display_Field_Name_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_Field_Name);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_Context_Display_Field_Description()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "PREF_ParamEditor_Field_Context_Display_Field_Description",
            Description = "PREF_ParamEditor_Field_Context_Display_Field_Description_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_Field_Description);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_Context_Display_Field_Attributes()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "PREF_ParamEditor_Field_Context_Display_Field_Attributes",
            Description = "PREF_ParamEditor_Field_Context_Display_Field_Attributes_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_Field_Attributes);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_Context_Display_Pin_Options()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "PREF_ParamEditor_Field_Context_Display_Pin_Options",
            Description = "PREF_ParamEditor_Field_Context_Display_Pin_Options_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_Pin_Options);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_Context_Display_Comparison_Options()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "PREF_ParamEditor_Field_Context_Display_Comparison_Options",
            Description = "PREF_ParamEditor_Field_Context_Display_Comparison_Options_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_Comparison_Options);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_Context_Display_Field_Value_Distribution()
    {
        return new PreferenceItem
        {
            OrderID = 6,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "PREF_ParamEditor_Field_Context_Display_Field_Value_Distribution",
            Description = "PREF_ParamEditor_Field_Context_Display_Field_Value_Distribution_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_Field_Value_Distribution);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_Context_Display_Searchbar_Options()
    {
        return new PreferenceItem
        {
            OrderID = 7,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "PREF_ParamEditor_Field_Context_Display_Searchbar_Options",
            Description = "PREF_ParamEditor_Field_Context_Display_Searchbar_Options_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_Searchbar_Options);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_Context_Display_References()
    {
        return new PreferenceItem
        {
            OrderID = 8,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "PREF_ParamEditor_Field_Context_Display_References",
            Description = "PREF_ParamEditor_Field_Context_Display_References_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_References);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_Context_Display_Reference_Search()
    {
        return new PreferenceItem
        {
            OrderID = 9,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "PREF_ParamEditor_Field_Context_Display_Reference_Search",
            Description = "PREF_ParamEditor_Field_Context_Display_Reference_Search_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_Reference_Search);
            }
        };
    }
    #endregion

    #region Mass Edit
    public static PreferenceItem ParamEditor_Show_Advanced_Mass_Edit_Commands()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Mass_Edit,

            Title = "PREF_ParamEditor_Show_Advanced_Mass_Edit_Commands",
            Description = "PREF_ParamEditor_Show_Advanced_Mass_Edit_Commands_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Show_Advanced_Mass_Edit_Commands);
            }
        };
    }
    #endregion


    #region Metadata
    public static PreferenceItem Project_Enable_Param_Meta_Override()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Meta_Override",
            Description = "PREF_Param_Editor_Enable_Param_Meta_Override_TT",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (curProject == null)
                    return;

                if (curProject.Handler == null)
                    return;

                if (ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Meta_Override))
                {
                    if (curProject != null)
                    {
                        curProject.Handler.ParamData.ParamMeta.Clear();
                        curProject.Handler.ParamData.ReloadMeta();
                    }
                }
            }
        };
    }

    public static PreferenceItem Project_Enable_Param_Enum_Addition()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Enum_Addition",
            Description = "PREF_Param_Editor_Enable_Param_Enum_Addition_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Enum_Addition);
            }
        };
    }

    public static PreferenceItem Project_Enable_Param_Enum_Override()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Enum_Override",
            Description = "PREF_Param_Editor_Enable_Param_Enum_Override_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Enum_Override);
            }
        };
    }

    public static PreferenceItem Project_Enable_Param_Category_Addition()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Category_Addition",
            Description = "PREF_Param_Editor_Enable_Param_Category_Addition_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Category_Addition);
            }
        };
    }

    public static PreferenceItem Project_Enable_Param_Category_Override()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Category_Override",
            Description = "PREF_Param_Editor_Enable_Param_Category_Override_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Category_Override);
            }
        };
    }

    public static PreferenceItem Project_Enable_Param_Commutativity_Group_Addition()
    {
        return new PreferenceItem
        {
            OrderID = 6,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Commutativity_Group_Addition",
            Description = "PREF_Param_Editor_Enable_Param_Commutativity_Group_Addition_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Commutativity_Group_Addition);
            }
        };
    }

    public static PreferenceItem Project_Enable_Param_Commutativity_Group_Override()
    {
        return new PreferenceItem
        {
            OrderID = 7,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Commutativity_Group_Override",
            Description = "PREF_Param_Editor_Enable_Param_Commutativity_Group_Override_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Commutativity_Group_Override);
            }
        };
    }

    public static PreferenceItem Project_Enable_Field_Reference_Group_Addition()
    {
        return new PreferenceItem
        {
            OrderID = 8,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Field_Reference_Group_Addition",
            Description = "PREF_Param_Editor_Enable_Param_Field_Reference_Group_Addition_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Field_Reference_Group_Addition);
            }
        };
    }

    public static PreferenceItem Project_Enable_Field_Reference_Group_Override()
    {
        return new PreferenceItem
        {
            OrderID = 9,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Field_Reference_Group_Override",
            Description = "PREF_Param_Editor_Enable_Param_Field_Reference_Group_Override_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Field_Reference_Group_Override);
            }
        };
    }

    public static PreferenceItem Project_Enable_Field_Layout_Addition()
    {
        return new PreferenceItem
        {
            OrderID = 10,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Field_Layout_Addition",
            Description = "PREF_Param_Editor_Enable_Field_Layout_Addition_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Field_Layout_Addition);
            }
        };
    }

    public static PreferenceItem Project_Enable_Field_Layout_Override()
    {
        return new PreferenceItem
        {
            OrderID = 11,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Param_Field_Layout_Override",
            Description = "PREF_Param_Editor_Enable_Param_Field_Layout_Override_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Param_Field_Layout_Override);
            }
        };
    }

    public static PreferenceItem Project_Enable_Icon_Configuration_Addition()
    {
        return new PreferenceItem
        {
            OrderID = 12,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Icon_Configuration_Addition",
            Description = "PREF_Param_Editor_Enable_Icon_Configuration_Addition_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Icon_Configuration_Addition);
            }
        };
    }

    public static PreferenceItem Project_Enable_Icon_Configuration_Override()
    {
        return new PreferenceItem
        {
            OrderID = 13,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Icon_Configuration_Override",
            Description = "PREF_Param_Editor_Enable_Icon_Configuration_Override_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Icon_Configuration_Override);
            }
        };
    }

    public static PreferenceItem Project_Enable_Graph_Annotation_Addition()
    {
        return new PreferenceItem
        {
            OrderID = 14,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Graph_Annotation_Addition",
            Description = "PREF_Param_Editor_Enable_Graph_Annotation_Addition_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Graph_Annotation_Addition);
            }
        };
    }

    public static PreferenceItem Project_Enable_Graph_Annotation_Override()
    {
        return new PreferenceItem
        {
            OrderID = 15,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Metadata,

            Title = "PREF_Param_Editor_Enable_Graph_Annotation_Override",
            Description = "PREF_Param_Editor_Enable_Graph_Annotation_Override_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Param_Editor_Enable_Graph_Annotation_Override);
            }
        };
    }

    #endregion
}
