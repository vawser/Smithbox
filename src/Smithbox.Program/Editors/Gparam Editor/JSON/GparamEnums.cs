using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamEnums
{
    public List<GparamEnumEntry> List { get; set; } = new();
}

public class GparamEnumEntry
{
    public string Key { get; set; }
    public List<GparamCategoryTextEntry> Names { get; set; } = new();

    public List<GparamEnumOption> Options { get; set; } = new();

    public GparamEnumEntry() { }

    public GparamEnumEntry(string key, string displayName)
    {
        Key = key;

        var curLang = CFG.Current.GparamEditor_Annotation_Language;

        Names.Add(new GparamCategoryTextEntry(curLang, key));

        Options = new List<GparamEnumOption>();
    }

    public string GetName()
    {
        var curLang = CFG.Current.GparamEditor_Annotation_Language;

        if (Names.Any(e => e.Language == curLang))
        {
            var name = Names.FirstOrDefault(e => e.Language == curLang);

            return name.Text;
        }

        return "";
    }

    public GparamEnumEntry Clone(GparamEnumEntry entry)
    {
        return (GparamEnumEntry)entry.MemberwiseClone();
    }
}

public class GparamEnumOption : IComparable
{
    public string Key { get; set; }

    public List<GparamCategoryTextEntry> Names { get; set; } = new();

    public GparamEnumOption()
    {

    }

    public GparamEnumOption(string key, string name)
    {
        Key = key;

        var curLang = CFG.Current.GparamEditor_Annotation_Language;

        Names.Add(new GparamCategoryTextEntry(curLang, name));
    }

    public string GetName()
    {
        var curLang = CFG.Current.GparamEditor_Annotation_Language;

        if (Names.Any(e => e.Language == curLang))
        {
            var name = Names.FirstOrDefault(e => e.Language == curLang);

            return name.Text;
        }

        return "";
    }

    public GparamEnumOption Clone(GparamEnumOption entry)
    {
        return (GparamEnumOption)entry.MemberwiseClone();
    }

    public int CompareTo(object obj)
    {
        GparamEnumOption option = obj as GparamEnumOption;

        try
        {
            var thisID = int.Parse(Key);
            var compID = int.Parse(option.Key);
            return thisID.CompareTo(compID);
        }
        catch
        {
            // For non-numeric values, compare strings
            return Key.CompareTo(option.Key);
        }
    }
}

public class GparamCategoryTextEntry
{
    public string Language { get; set; }
    public string Text { get; set; }

    public GparamCategoryTextEntry() { }

    public GparamCategoryTextEntry(string lang, string text)
    {
        Language = lang;
        Text = text;
    }
}