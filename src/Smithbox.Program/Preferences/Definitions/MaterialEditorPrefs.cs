using Hexa.NET.ImGui;
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

    public static PreferenceItem Project_XXX()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MaterialEditor,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "XXX",
            Description = "XXX",

            Draw = () => {

            }
        };
    }
}
