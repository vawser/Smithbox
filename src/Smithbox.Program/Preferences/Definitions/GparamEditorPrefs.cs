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

            Title = "Display Aliases",
            Description = "If enabled, aliases are displayed in the file list.",

            Draw = () => {
                ImGui.Checkbox("##inputValue_file", ref CFG.Current.GparamEditor_File_List_Display_Aliases);
            }
        };
    }
    #endregion

    #region Group List
    public static PreferenceItem GparamEditor_Group_List_Display_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Group_List,

            Title = "Display Aliases",
            Description = "If enabled, aliases are displayed in the group list.",

            Draw = () => {
                ImGui.Checkbox("##inputValue_group", ref CFG.Current.GparamEditor_Group_List_Display_Aliases);
            }
        };
    }
    public static PreferenceItem GparamEditor_Group_List_Display_Group_Add()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Group_List,

            Title = "Display Add Missing Group Button",
            Description = "If enabled, a button for a missing group will be displayed that allows you to add it.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Group_List_Display_Group_Add);
            }
        };
    }
    public static PreferenceItem GparamEditor_Group_List_Display_Empty_Group()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Group_List,

            Title = "Display Empty Groups",
            Description = "If enabled, empty groups with no fields are displayed.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Group_List_Display_Empty_Group);
            }
        };
    }
    #endregion

    #region Field List
    public static PreferenceItem Gparam_Editor_Field_List_Enable_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Field_List,

            Title = "Display Aliases",
            Description = "If enabled, aliases are displayed in the field list.",

            Draw = () => {
                ImGui.Checkbox("##inputValue_field", ref CFG.Current.GparamEditor_Field_List_Enable_Aliases);
            }
        };
    }
    public static PreferenceItem GparamEditor_Field_List_Display_Field_Add()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.GparamEditor,
            Spacer = true,

            Section = SectionCategory.GparamEditor_Field_List,

            Title = "Display Add Missing Field Button",
            Description = "If enabled, a button for a missing field will be displayed that allows you to add it.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Field_List_Display_Field_Add);
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

            Title = "Display Color Edit for 4-digit Properties",
            Description = "If enabled, the color picker will be displayed on properties with a 4-digit type.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.GparamEditor_Value_List_Display_Color_Edit_V4);
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

            Title = "Color Edit Display Mode",
            Description = "Determines how the color edit displays the numeric data.",

            Draw = () => {
                var curMode = CFG.Current.GparamEditor_Color_Edit_Mode;

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##inputValue", curMode.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(ColorEditDisplayMode)))
                    {
                        var mode = (ColorEditDisplayMode)entry;

                        if (ImGui.Selectable($"{mode.GetDisplayName()}", curMode == mode))
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
