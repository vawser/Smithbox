using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamCategories
{
    public List<ParamCategoryEntry> Categories { get; set; } = new();
}

public class ParamCategoryEntry
{
    public string Key { get; set; }
    public bool ForceBottom { get; set; } = false;
    public bool ForceTop { get; set; } = false;

    public List<ParamCategoryNameEntry> DisplayNames { get; set; } = new();
    public List<string> Params { get; set; }

    public string GetDisplayName()
    {
        var curLang = CFG.Current.ParamEditor_Annotation_Language;

        if(DisplayNames.Any(e => e.Language == curLang))
        {
            var name = DisplayNames.FirstOrDefault(e => e.Language == curLang);

            return name.Name;
        }

        return "";
    }
}

public class ParamCategoryNameEntry
{
    public string Language { get; set; }
    public string Name { get; set; }
}