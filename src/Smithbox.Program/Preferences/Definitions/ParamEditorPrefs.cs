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
    public static PreferenceItem ParamEditor_Enable_Compact_Mode()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "Enable Compact Presentation",
            Description = "If enabled, the line height between elements is reduced, allow more params, rows and fields to be visible.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Enable_Compact_Mode);
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

            Title = "Enable Loose Param Mode",
            Description = "If enabled, loose .param files will be considered during the param loading and saving processes.",

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

            Title = "Enable Loose Param Repack",
            Description = "If enabled, any loose .param files will be repacked into the enc_regulation.bin",

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

            Title = "Enable Loose Param Mode",
            Description = "If enabled, loose .param files will be considered during the param loading and saving processes.",

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

            Title = "Strip Row Names on Save",
            Description = "If enabled, row names are stripped upon save, meaning no row names will be stored within the regulation.",

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

            Title = "Strip Row Names on Save",
            Description = "If enabled, row names are stripped upon save, meaning no row names will be stored within the regulation.",

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

            Title = "Strip Row Names on Save",
            Description = "If enabled, row names are stripped upon save, meaning no row names will be stored within the regulation.",

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

            Title = "Strip Row Names on Save",
            Description = "If enabled, row names are stripped upon save, meaning no row names will be stored within the regulation.",

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

            Title = "Strip Row Names on Save",
            Description = "If enabled, row names are stripped upon save, meaning no row names will be stored within the regulation.",

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

            Title = "Strip Row Names on Save",
            Description = "If enabled, row names are stripped upon save, meaning no row names will be stored within the regulation.",

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

            Title = "Strip Row Names on Save",
            Description = "If enabled, row names are stripped upon save, meaning no row names will be stored within the regulation.",

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

            Title = "Strip Row Names on Save",
            Description = "If enabled, row names are stripped upon save, meaning no row names will be stored within the regulation.",

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

            Title = "Strip Row Names on Save",
            Description = "If enabled, row names are stripped upon save, meaning no row names will be stored within the regulation.",

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

            Title = "Restore Row Names on Load",
            Description = "If enabled, and stripped row names have been stored, row names will be re-apply upon loading.",

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

            Title = "Restore Row Names on Load",
            Description = "If enabled, and stripped row names have been stored, row names will be re-apply upon loading.",

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

            Title = "Restore Row Names on Load",
            Description = "If enabled, and stripped row names have been stored, row names will be re-apply upon loading.",

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

            Title = "Restore Row Names on Load",
            Description = "If enabled, and stripped row names have been stored, row names will be re-apply upon loading.",

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

            Title = "Restore Row Names on Load",
            Description = "If enabled, and stripped row names have been stored, row names will be re-apply upon loading.",

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

            Title = "Restore Row Names on Load",
            Description = "If enabled, and stripped row names have been stored, row names will be re-apply upon loading.",

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

            Title = "Restore Row Names on Load",
            Description = "If enabled, and stripped row names have been stored, row names will be re-apply upon loading.",

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

            Title = "Restore Row Names on Load",
            Description = "If enabled, and stripped row names have been stored, row names will be re-apply upon loading.",

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

            Title = "Restore Row Names on Load",
            Description = "If enabled, and stripped row names have been stored, row names will be re-apply upon loading.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Stripped_Row_Name_Load_NR);
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

            Title = "Automatically Use Latest",
            Description = "If enabled, the param reloader version will be set to the latest executable version whenever Smithbox is started.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.UseLatestGameOffset);
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

            Title = "Param Reloader Version",
            Description = "This should match the executable version you wish to target, otherwise the memory offsets will be incorrect.",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;
                var index = CFG.Current.SelectedGameOffsetData;

                string[] options = curProject.Handler.ParamData.ParamMemoryOffsets.list.Select(entry => entry.exeVersion).ToArray();

                if (ImGui.Combo("##GameOffsetVersion", ref index, options, options.Length))
                {
                    CFG.Current.SelectedGameOffsetData = index;
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

            Title = "Always Display Pinned Entries",
            Description = "If enabled, pinned param entries will always be visible at the top of the list, even when scrolling.",

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

            Title = "Sort List Alphabetically",
            Description = "If enabled, the param list will be sorted alphabetically.",

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

            Title = "Display Community Names",
            Description = "If enabled, community names will be used instead of the raw filename.",

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

            Title = "Display Categories",
            Description = "If enabled, the params will be grouped into collapsible categories.",

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

            Title = "Always Display Pinned Entries",
            Description = "If enabled, pinned row entries will always be visible at the top of the list, even when scrolling.",

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

            Title = "Enable Row Name Word-Wrap",
            Description = "If enabled, the row names will be word-wrapped.",

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

            Title = "Enable Row Grouping for Item Lots",
            Description = "If enabled, the rows within Item Lot params will be grouped.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_List_Enable_Row_Grouping);
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

            Title = "Always Display Pinned Entries",
            Description = "If enabled, pinned field entries will always be visible at the top of the list, even when scrolling.",

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

            Title = "Enable Field Rearrangement",
            Description = "If enabled, fields will be re-arranged according to the grouping arrangement specified in the Param Meta file.",

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

            Title = "Field Name Mode",
            Description = "Determines how the field naming is displayed.\nSource is the original param definition name.\nCommunity is a more readable and descriptive name.",

            Draw = () => {
                if (ImGui.BeginCombo("##inputValue", CFG.Current.ParamEditor_FieldNameMode.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(ParamFieldNameMode)))
                    {
                        var type = (ParamFieldNameMode)entry;

                        if (ImGui.Selectable(type.GetDisplayName()))
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

            Title = "Display Offsets",
            Description = "If enabled, the data offset for a field will be displayed.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_List_Display_Offsets);
            }
        };
    }
    #endregion

    #region Field Input

    #endregion

    #region Param Context Menu

    #endregion

    #region Table Context Menu

    #endregion

    #region Row Context Menu

    #endregion

    #region Field Context Menu

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

            Title = "Display Advanced Commands in Auto-fill",
            Description = "If enabled, more complex commands are visible in the auto-fill menu.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Show_Advanced_Mass_Edit_Commands);
            }
        };
    }
    #endregion
}
