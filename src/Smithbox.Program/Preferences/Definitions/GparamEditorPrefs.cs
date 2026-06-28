using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.GparamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class GparamEditorPrefs
{
    public static Type GetPrefType()
    {
        return typeof(GparamEditorPrefs);
    }

    #region File List
    public static PreferenceItem GparamEditor_File_List_Display_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_File_List,

            Title = "PREF_GparamEditor_File_List_Display_Aliases",
            Description = "PREF_GparamEditor_File_List_Display_Aliases_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue_file", ref CFG.Current.GparamEditor_File_List_Display_Aliases);
            }
        };
    }
    #endregion

    #region Group List
    public static PreferenceItem GparamEditor_Group_List_Display_Descriptions()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Group_List,

            Title = "PREF_GparamEditor_Group_List_Display_Descriptions",
            Description = "PREF_GparamEditor_Group_List_Display_Descriptions_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Group_List_Display_Descriptions);
            }
        };
    }
    public static PreferenceItem GparamEditor_Group_List_Display_Empty_Group()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Group_List,

            Title = "PREF_GparamEditor_Group_List_Display_Empty_Group",
            Description = "PREF_GparamEditor_Group_List_Display_Empty_Group_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Group_List_Display_Empty_Group);
            }
        };
    }
    #endregion

    #region Field List
    public static PreferenceItem GparamEditor_Field_List_Display_Descriptions()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Field_List,

            Title = "PREF_GparamEditor_Field_List_Display_Descriptions",
            Description = "PREF_GparamEditor_Field_List_Display_Descriptions_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Field_List_Display_Descriptions);
            }
        };
    }
    #endregion

    #region Value List
    public static PreferenceItem GparamEditor_Value_List_Display_Color_Edit_V4()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Value_List,

            Title = "PREF_GparamEditor_Value_List_Display_Color_Edit_V4",
            Description = "PREF_GparamEditor_Value_List_Display_Color_Edit_V4_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Value_List_Display_Color_Edit_V4);
            }
        };
    }
    public static PreferenceItem GparamEditor_Value_List_Display_Time_Of_Day_Column()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Value_List,

            Title = "PREF_GparamEditor_Value_List_Display_Time_Of_Day_Column",
            Description = "PREF_GparamEditor_Value_List_Display_Time_Of_Day_Column_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column);
            }
        };
    }
    public static PreferenceItem GparamEditor_Value_List_Display_Information_Column()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Value_List,

            Title = "PREF_GparamEditor_Value_List_Display_Information_Column",
            Description = "PREF_GparamEditor_Value_List_Display_Information_Column_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Value_List_Display_Information_Column);
            }
        };
    }
    #endregion

    #region Color Edit
    public static PreferenceItem GparamEditor_Color_Edit_Mode()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.GparamEditor_Color_Edit,

            Title = "PREF_GparamEditor_Color_Edit_Mode",
            Description = "PREF_GparamEditor_Color_Edit_Mode_TT",

            Draw = () => {
                var curMode = CFG.Current.GparamEditor_Color_Edit_Mode;

                var previewName = LOC.Get(curMode.GetDisplayName());

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", previewName))
                {
                    foreach (var entry in Enum.GetValues(typeof(ColorEditDisplayMode)))
                    {
                        var mode = (ColorEditDisplayMode)entry;

                        var displayName = LOC.Get(mode.GetDisplayName());

                        if (ImGui.Selectable(displayName, curMode == mode))
                        {
                            CFG.Current.GparamEditor_Color_Edit_Mode = mode;
                        }
                    }

                    ImGui.EndCombo();
                }
            }
        };
    }
    #endregion
}
