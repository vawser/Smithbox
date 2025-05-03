using Microsoft.VisualBasic;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

public class FormatInformationUtils
{
    public static Dictionary<string, FormatReference> GetEntries(FormatResource source)
    {
        Dictionary<string, FormatReference> Entries = new Dictionary<string, FormatReference>();

        foreach (var entry in source.list)
        {
            Entries.Add(entry.id, entry);
        }

        return Entries;
    }

    public static Dictionary<string, FormatEnumEntry> GetEntries(FormatEnum source)
    {
        Dictionary<string, FormatEnumEntry> Entries = new Dictionary<string, FormatEnumEntry>();

        foreach (var entry in source.list)
        {
            Entries.Add(entry.id, entry);
        }

        return Entries;
    }

    public static Dictionary<string, FormatMaskEntry> GetEntries(FormatMask source)
    {
        Dictionary<string, FormatMaskEntry> Entries = new Dictionary<string, FormatMaskEntry>();

        foreach (var entry in source.list)
        {
            Entries.Add(entry.model, entry);
        }

        return Entries;
    }

    public static string GetClassReferenceName(FormatResource source, string classKey)
    {
        var name = "";

        if (source == null)
            return name;

        if (source.list == null)
            return name;

        // Top
        foreach (FormatReference entry in source.list)
        {
            if (entry.id == classKey)
            {
                name = entry.name;
            }
        }

        return name;
    }

    public static string GetClassReferenceDescription(FormatResource source, string classKey)
    {
        var desc = "";

        if (source == null)
            return desc;

        if (source.list == null)
            return desc;

        // Top
        foreach (FormatReference entry in source.list)
        {
            if (entry.id == classKey)
            {
                desc = entry.description;
            }
        }

        return desc;
    }

    public static string GetReferenceName(FormatResource source, string classKey, string name, string sharedTypeName = "")
    {
        if (source == null)
            return name;

        if (source.list == null)
            return name;

        // Top
        foreach (FormatReference entry in source.list)
        {
            if (entry.id == classKey || entry.id == sharedTypeName)
            {
                // Members
                foreach (FormatMember member in entry.members)
                {
                    if (member.id == name)
                    {
                        name = member.name;
                    }
                }
            }
        }

        return name;
    }

    public static string GetReferenceDescription(FormatResource source, string classKey, string key, string sharedTypeName = "")
    {
        var desc = "";

        if (source == null)
            return desc;

        if (source.list == null)
            return desc;

        // Top
        foreach (FormatReference entry in source.list)
        {
            if (entry.id == classKey || entry.id == sharedTypeName)
            {
                // Members
                foreach (FormatMember member in entry.members)
                {
                    if (member.id == key)
                    {
                        desc = member.description;
                    }
                }
            }
        }

        return desc;
    }

    public static string GetTypeForProperty(FormatResource source, string fieldKey)
    {
        string typeName = "";

        if (source == null)
            return typeName;

        foreach (var entry in source.list)
        {
            foreach (var subentry in entry.members)
            {
                if (subentry.id == fieldKey)
                {
                    typeName = GetAttributeContents(subentry.attributes, "IsType");
                }
            }
        }

        return typeName;
    }

    public static FormatEnumEntry GetEnumForProperty(FormatResource source, FormatEnum enumSource, string fieldKey)
    {
        FormatEnumEntry formatEnum = null;
        string enumName = "";

        if (source == null)
            return formatEnum;

        foreach (var entry in source.list)
        {
            foreach (var subentry in entry.members)
            {
                if (subentry.id == fieldKey)
                {
                    enumName = GetAttributeContents(subentry.attributes, "Enum");
                }
            }
        }

        if (enumName != "")
        {
            formatEnum = enumSource.list.Find(x => x.id == enumName);
        }

        return formatEnum;
    }

    public static string GetAttributeContents(string attributeStr, string targetAttribute)
    {
        string[] attributeList = new string[] { attributeStr };

        if (attributeStr.Contains(","))
        {
            attributeList = attributeStr.Split(",");
        }

        foreach (string attr in attributeList)
        {
            Match contents = Regex.Match(attr, $@"{targetAttribute}\[(.*)\]");
            if (contents.Success)
            {
                return contents.Groups[1].Value;
            }
        }

        return "";
    }

    // Hides property in editor view
    public static bool IsHiddenProperty(FormatResource source, string key)
    {
        return IsSpecifiedProperty(source, key, "IsHidden");
    }

    // Overrides input with checkbox
    public static bool IsBooleanProperty(FormatResource source, string key)
    {
        return IsSpecifiedProperty(source, key, "IsBool");
    }

    public static bool IsSpecifiedProperty(FormatResource source, string key, string attribute)
    {
        if (source == null)
            return false;

        if (source.list == null)
            return false;

        // Top
        foreach (FormatReference entry in source.list)
        {
            if (entry.id == key)
            {
                return IsSpecifiedAttribute(entry.attributes, attribute);
            }

            // Members
            foreach (FormatMember member in entry.members)
            {
                if (member.id == key)
                {
                    return IsSpecifiedAttribute(member.attributes, attribute);
                }
            }
        }

        return false;
    }

    public static bool IsSpecifiedAttribute(string rawAttributes, string specificAttribute)
    {
        string[] attributes = rawAttributes.Split(",");
        foreach (string attr in attributes)
        {
            if (attr == specificAttribute)
            {
                return true;
            }
        }

        return false;
    }
}
