using Hexa.NET.ImGui;
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

    public static PreferenceItem Project_XXX()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.ModelEditor,

            Section = "XXX",
            Title = "XXX",

            Description = "XXX",
            Spacer = true,

            Draw = () => {

            }
        };
    }
}
