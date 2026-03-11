using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class FieldLayouts
{
    public List<FieldLayout> Entries { get; set; } = new();
}

public class FieldLayout
{
    public string Name { get; set; }
    public bool UngroupedAtBottom { get; set; }

    public List<FieldLayoutEntry> Groups { get; set; } = new();
}

public class FieldLayoutEntry
{
    public string Key { get; set; }
    public List<FieldLayoutNameEntry> Names { get; set; } = new();
    public List<string> Fields { get; set; }

    public string GetName()
    {
        var curLang = CFG.Current.ParamEditor_Annotation_Language;

        if (Names.Any(e => e.Language == curLang))
        {
            var name = Names.FirstOrDefault(e => e.Language == curLang);

            return name.Name;
        }

        return "";
    }
}

public class FieldLayoutNameEntry
{
    public string Language { get; set; }
    public string Name { get; set; }
}