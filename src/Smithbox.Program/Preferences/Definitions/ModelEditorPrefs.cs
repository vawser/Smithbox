using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class ModelEditorPrefs
{
    public static Type GetPrefType()
    {
        return typeof(ModelEditorPrefs);
    }

    #region Properties
    public static PreferenceItem ModelEditor_Properties_Enable_Commmunity_Names()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.ModelEditor,
            Spacer = true,

            Section = "Properties",

            Title = "Display Community Names",
            Description = "If enabled, community names are displayed for FLVER property names.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ModelEditor_Properties_Enable_Commmunity_Names);
            }
        };
    }
    public static PreferenceItem ModelEditor_Properties_Enable_Commmunity_Descriptions()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.ModelEditor,
            Spacer = true,

            Section = "Properties",

            Title = "Display Community Descriptions",
            Description = "If enabled, community descriptions are displayed in the tooltips for FLVER properties.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ModelEditor_Properties_Enable_Commmunity_Descriptions);
            }
        };
    }
    #endregion
}
