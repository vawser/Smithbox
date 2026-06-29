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
            OrderID = 0,
            Category = PreferenceCategory.ModelEditor,
            Spacer = true,

            Section = SectionCategory.ModelEditor_Properties,

            Title = "PREF_ModelEditor_Properties_Enable_Commmunity_Names",
            Description = "PREF_ModelEditor_Properties_Enable_Commmunity_Names_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ModelEditor_Properties_Enable_Commmunity_Names);
            }
        };
    }
    public static PreferenceItem ModelEditor_Properties_Enable_Commmunity_Descriptions()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.ModelEditor,
            Spacer = true,

            Section = SectionCategory.ModelEditor_Properties,

            Title = "PREF_ModelEditor_Properties_Enable_Commmunity_Descriptions",
            Description = "PREF_ModelEditor_Properties_Enable_Commmunity_Descriptions_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.ModelEditor_Properties_Enable_Commmunity_Descriptions);
            }
        };
    }
    #endregion
}
