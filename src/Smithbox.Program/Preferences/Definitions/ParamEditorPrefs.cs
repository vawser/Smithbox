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

            Title = "Param Reloader Version",
            Description = "This should match the executable version you wish to target, otherwise the memory offsets will be incorrect.",

            Draw = () => {
                var curProject = Smithbox.Orchestrator.SelectedProject;
                var index = CFG.Current.ParamReloader_Current_Offsets;

                string[] options = curProject.Handler.ParamData.ParamMemoryOffsets.list.Select(entry => entry.exeVersion).ToArray();

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
    public static PreferenceItem ParamEditor_Row_List_Display_Decorators()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_List,

            Title = "Display Decorators",
            Description = "If enabled, the row decorators are displayed.",

            Draw = () => {
                ImGui.Checkbox("##inputValue_row", ref CFG.Current.ParamEditor_Row_List_Display_Decorators);
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
                DPI.ApplyInputWidth();
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
    public static PreferenceItem ParamEditor_Field_List_Display_Color_Picker()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "Display Color Picker",
            Description = "If enabled, a color picker is displayed (when suitable for the field).",

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

            Title = "Display Graph",
            Description = "If enabled, a graph is displayed (when suitable for the field).",

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

            Title = "Display Map Link",
            Description = "If enabled, a map link button is displayed (when suitable for the field).",

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

            Title = "Display Model Link",
            Description = "If enabled, a model link button is displayed (when suitable for the field).",

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

            Title = "Display Padding",
            Description = "If enabled, padding fields are displayed.",

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

            Title = "Display Decorators",
            Description = "If enabled, field decorators are displayed.",

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

            Title = "Display References",
            Description = "If enabled, field references are displayed.",

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

            Title = "Display Enums",
            Description = "If enabled, field enums are displayed.",

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

            Title = "Display Attributes",
            Description = "If enabled, field attributes are displayed in the tooltip.",

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

            Title = "Display Icon Preview",
            Description = "If enabled, a referenced icon will be displayed visually by the field.",

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

            Title = "Icon Preview Scale",
            Description = "The image scale of the icon preview.",

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

            Title = "Field Tooltip Mode",
            Description = "Determines how the field tooltip is shown.",

            Draw = () => {
                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", CFG.Current.ParamEditor_Field_List_Tooltip_Mode.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(ParamTooltipMode)))
                    {
                        var type = (ParamTooltipMode)entry;

                        if (ImGui.Selectable(type.GetDisplayName()))
                        {
                            CFG.Current.ParamEditor_Field_List_Tooltip_Mode = (ParamTooltipMode)entry;
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

            Title = "Display Traditional Percentages",
            Description = "If enabled, fields that represent a percentage will be displayed in a normal fashion, with the value representing the percentage. Typically, the value in represented as 1 - <value>.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Input_Display_Traditional_Percentage);
            }
        };
    }
    #endregion

    #region Row Context Menu
    public static PreferenceItem ParamEditor_Row_Context_Display_Advanced_Options()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "Display Advanced Options",
            Description = "If enabled, the advanced options in the row context menu will be displayed.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Advanced_Options);
            }
        };
    }
    public static PreferenceItem ParamEditor_Row_Context_Display_Row_Name_Input()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Row_Context_Menu,

            Title = "Display Row Name Input",
            Description = "If enabled, display the row name input box within the context menu.",

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

            Title = "Display Shortcut Tools",
            Description = "If enabled, display the shortcut tools within the context menu.",

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

            Title = "Display Pin Options",
            Description = "If enabled, display the pin options within the context menu.",

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

            Title = "Display Comparison Options",
            Description = "If enabled, display the comparison options within the context menu.",

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

            Title = "Display Reverse Lookup Tool",
            Description = "If enabled, display the reverse lookup tool within the context menu.",

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

            Title = "Display Proliferate Name Tool",
            Description = "If enabled, display the proliferate name tool within the context menu.",

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

            Title = "Display Inherit Name Tool",
            Description = "If enabled, display the inherit name tool within the context menu.",

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

            Title = "Display Row Name Tools",
            Description = "If enabled, display the row name tools within the context menu.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Row_Context_Display_Row_Name_Tools);
            }
        };
    }

    #endregion

    #region Field Context Menu
    public static PreferenceItem ParamEditor_Field_Context_Split()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "Split Menu",
            Description = "If enabled, the context menu is adjusted if the right-click is done on name text or a value input.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Split);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_Context_Display_Field_Name()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,

            Section = SectionCategory.ParamEditor_Field_Context_Menu,

            Title = "Display Field Name",
            Description = "If enabled, the field name is displayed in the context menu.",

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

            Title = "Display Field Description",
            Description = "If enabled, the field description is displayed in the context menu.",

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

            Title = "Display Field Attributes",
            Description = "If enabled, the field attributes are displayed in the context menu.",

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

            Title = "Display Field Pin Options",
            Description = "If enabled, the field pin options are displayed in the context menu.",

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

            Title = "Display Field Comparison Options",
            Description = "If enabled, the field comparison options are displayed in the context menu.",

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

            Title = "Display Field Value Distribution",
            Description = "If enabled, the field value distribution tool is displayed in the context menu.",

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

            Title = "Display Field Searchbar Options",
            Description = "If enabled, the field searchbar options are displayed in the context menu.",

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

            Title = "Display Field References",
            Description = "If enabled, the field references are displayed in the context menu.",

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

            Title = "Display Field Reference Search",
            Description = "If enabled, the field reference search tool is displayed in the context menu.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Field_Context_Display_Reference_Search);
            }
        };
    }
    public static PreferenceItem ParamEditor_Field_List_Context_Mass_Edit_Display_Mode()
    {
        return new PreferenceItem
        {
            OrderID = 10,
            Category = PreferenceCategory.ParamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ParamEditor_Field_List,

            Title = "Mass Edit Display Mode",
            Description = "Determines how the mass edit input is displayed in the field context menu.",

            Draw = () => {
                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", CFG.Current.ParamEditor_Field_List_Context_Mass_Edit_Display_Mode.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(ParamFieldMassEditMode)))
                    {
                        var type = (ParamFieldMassEditMode)entry;

                        if (ImGui.Selectable(type.GetDisplayName()))
                        {
                            CFG.Current.ParamEditor_Field_List_Context_Mass_Edit_Display_Mode = (ParamFieldMassEditMode)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
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

            Title = "Display Advanced Commands in Auto-fill",
            Description = "If enabled, more complex commands are visible in the auto-fill menu.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ParamEditor_Show_Advanced_Mass_Edit_Commands);
            }
        };
    }
    #endregion
}
