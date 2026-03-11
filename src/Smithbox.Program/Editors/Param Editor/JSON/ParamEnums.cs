using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Editors.ParamEditor;
public class ParamEnums
{
    public List<ParamEnumEntry> List { get; set; } = new();
}

public class ParamEnumEntry
{
    public string Key { get; set; }
    public List<ParamCategoryTextEntry> Names { get; set; } = new();

    public List<ParamEnumOption> Options { get; set; } = new();

    public ParamEnumEntry() { }

    public ParamEnumEntry(string key, string displayName)
    {
        Key = key;

        var curLang = CFG.Current.ParamEditor_Annotation_Language;

        Names.Add(new ParamCategoryTextEntry(curLang, key));

        Options = new List<ParamEnumOption>();
    }

    public string GetName()
    {
        var curLang = CFG.Current.ParamEditor_Annotation_Language;

        if (Names.Any(e => e.Language == curLang))
        {
            var name = Names.FirstOrDefault(e => e.Language == curLang);

            return name.Text;
        }

        return "";
    }

    public ParamEnumEntry Clone(ParamEnumEntry entry)
    {
        return (ParamEnumEntry)entry.MemberwiseClone();
    }
}

public class ParamEnumOption : IComparable
{
    public string Key { get; set; }

    public List<ParamCategoryTextEntry> Names { get; set; } = new();

    public ParamEnumOption()
    {

    }

    public ParamEnumOption(string key, string name)
    {
        Key = key;

        var curLang = CFG.Current.ParamEditor_Annotation_Language;

        Names.Add(new ParamCategoryTextEntry(curLang, name));
    }

    public string GetName()
    {
        var curLang = CFG.Current.ParamEditor_Annotation_Language;

        if (Names.Any(e => e.Language == curLang))
        {
            var name = Names.FirstOrDefault(e => e.Language == curLang);

            return name.Text;
        }

        return "";
    }

    public ParamEnumOption Clone(ParamEnumOption entry)
    {
        return (ParamEnumOption)entry.MemberwiseClone();
    }

    public int CompareTo(object obj)
    {
        ParamEnumOption option = obj as ParamEnumOption;

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

public class ParamCategoryTextEntry
{
    public string Language { get; set; }
    public string Text { get; set; }

    public ParamCategoryTextEntry() { }

    public ParamCategoryTextEntry(string lang, string text)
    {
        Language = lang; 
        Text = text;
    }
}