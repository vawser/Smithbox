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

            Title = "Display Community Names",
            Description = "If enabled, display community names in the properties list.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MaterialEditor_Properties_Display_Community_Names);
            }
        };
    }
    #endregion
}
