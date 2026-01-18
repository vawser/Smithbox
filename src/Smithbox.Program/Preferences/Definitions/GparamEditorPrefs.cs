using Hexa.NET.ImGui;
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

    public static PreferenceItem Project_XXX()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.GparamEditor,

            Section = "XXX",
            Title = "XXX",

            Description = "XXX",
            Spacer = true,

            Draw = () => {

            }
        };
    }
}
