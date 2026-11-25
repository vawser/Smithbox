using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
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

        var curLang = CFG.Current.ParamEditor.ParamEditor_Annotation_Language;

        Names.Add(new ParamCategoryTextEntry(curLang, key));

        Options = new List<ParamEnumOption>();
    }

    public string GetName()
    {
        var curLang = CFG.Current.ParamEditor.ParamEditor_Annotation_Language;

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

    /// <summary>
    /// What the option does, per language. Optional: left out of the JSON when the option has none.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ParamCategoryTextEntry> Descriptions { get; set; }

    public ParamEnumOption()
    {

    }

    public ParamEnumOption(string key, string name)
    {
        Key = key;

        var curLang = CFG.Current.ParamEditor.ParamEditor_Annotation_Language;

        Names.Add(new ParamCategoryTextEntry(curLang, name));
    }

    public string GetName()
    {
        var curLang = CFG.Current.ParamEditor.ParamEditor_Annotation_Language;

        if (Names.Any(e => e.Language == curLang))
        {
            var name = Names.FirstOrDefault(e => e.Language == curLang);

            return name.Text;
        }

        return "";
    }

    /// <summary>
    /// The description in the annotation language, or "" if the option has none in it
    /// </summary>
    public string GetDescription()
    {
        var curLang = CFG.Current.ParamEditor.ParamEditor_Annotation_Language;

        return Descriptions?.FirstOrDefault(e => e.Language == curLang)?.Text ?? "";
    }

    /// <summary>
    /// Sets the description in the annotation language, adding an entry for the language if there isn't one.
    /// An empty description removes the entry, so that the JSON doesn't keep blank descriptions.
    /// </summary>
    public void SetDescription(string text)
    {
        var curLang = CFG.Current.ParamEditor.ParamEditor_Annotation_Language;
        var entry = Descriptions?.FirstOrDefault(e => e.Language == curLang);

        if (string.IsNullOrEmpty(text))
        {
            if (entry != null)
                Descriptions.Remove(entry);

            if (Descriptions?.Count == 0)
                Descriptions = null;

            return;
        }

        if (entry != null)
        {
            entry.Text = text;
            return;
        }

        Descriptions ??= new();
        Descriptions.Add(new ParamCategoryTextEntry(curLang, text));
    }

    public ParamEnumOption Clone(ParamEnumOption entry)
    {
        var clone = (ParamEnumOption)entry.MemberwiseClone();

        // Copied, so that editing the clone's descriptions leaves the original's alone
        clone.Descriptions = entry.Descriptions?.Select(e => new ParamCategoryTextEntry(e.Language, e.Text)).ToList();

        return clone;
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