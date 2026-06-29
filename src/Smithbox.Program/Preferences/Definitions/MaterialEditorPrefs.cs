using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class MaterialEditorPrefs
{
    public static Type GetPrefType()
    {
        return typeof(MaterialEditorPrefs);
    }

    #region Properties
    public static PreferenceItem MaterialEditor_Properties_Display_Community_Names()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MaterialEditor,
            Spacer = true,

            Section = SectionCategory.MaterialEditor_Properties,

            Title = "PREF_MaterialEditor_Properties_Display_Community_Names",
            Description = "PREF_MaterialEditor_Properties_Display_Community_Names_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MaterialEditor_Properties_Display_Community_Names);
            }
        };
    }
    #endregion
}
