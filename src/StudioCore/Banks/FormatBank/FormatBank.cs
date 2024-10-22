using StudioCore.Editor;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using HKLib.hk2018.hkReflect;

namespace StudioCore.Banks.FormatBank;

/// <summary>
/// An info bank holds information for annotating formats, such as MSB.
/// An info bank has 1 source: Smithbox.
/// </summary>
public class FormatBank
{
    public FormatResource Information { get; set; }
    public FormatEnum Enums { get; set; }
    public FormatMask Masks { get; set; }

    private bool IsGameSpecific;

    private string FormatTitle = "";

    public FormatBank(string title, bool isGameSpecific)
    {
        IsGameSpecific = isGameSpecific;

        FormatTitle = title;
    }

    public void LoadBank()
    {
        try
        {
            Information = BankUtils.LoadFormatResourceJSON(FormatTitle, IsGameSpecific);
            Enums = BankUtils.LoadFormatEnumJSON(FormatTitle, IsGameSpecific);
            Masks = BankUtils.LoadFormatMaskJSON(FormatTitle, IsGameSpecific);
        }
        catch (Exception e)
        {
#if DEBUG
            TaskLogs.AddLog($"Failed to load Format Bank {FormatTitle}: {e.Message}");
#endif
        }

        TaskLogs.AddLog($"Format Bank: Loaded {FormatTitle} Bank");
    }

    public Dictionary<string, FormatReference> GetInformationEntries()
    {
        Dictionary<string, FormatReference> Entries = new Dictionary<string, FormatReference>();

        foreach (var entry in Information.list)
        {
            Entries.Add(entry.id, entry);
        }

        return Entries;
    }

    public Dictionary<string, FormatEnumEntry> GetEnumEntries()
    {
        Dictionary<string, FormatEnumEntry> Entries = new Dictionary<string, FormatEnumEntry>();

        foreach (var entry in Enums.list)
        {
            Entries.Add(entry.id, entry);
        }

        return Entries;
    }

    public Dictionary<string, FormatMaskEntry> GetMaskEntries()
    {
        Dictionary<string, FormatMaskEntry> Entries = new Dictionary<string, FormatMaskEntry>();

        foreach (var entry in Masks.list)
        {
            Entries.Add(entry.model, entry);
        }

        return Entries;
    }

    public string GetClassReferenceName(string classKey)
    {
        var name = "";

        if (Information == null)
            return name;

        if (Information.list == null)
            return name;

        // Top
        foreach (FormatReference entry in Information.list)
        {
            if (entry.id == classKey)
            {
                name = entry.name;
            }
        }

        return name;
    }

    public string GetClassReferenceDescription(string classKey)
    {
        var desc = "";

        if (Information == null)
            return desc;

        if (Information.list == null)
            return desc;

        // Top
        foreach (FormatReference entry in Information.list)
        {
            if (entry.id == classKey)
            {
                desc = entry.description;
            }
        }

        return desc;
    }

    public string GetReferenceName(string classKey, string name, string sharedTypeName = "")
    {
        if (Information == null)
            return name;

        if (Information.list == null)
            return name;

        // Top
        foreach (FormatReference entry in Information.list)
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

    public string GetReferenceDescription(string classKey, string key, string sharedTypeName = "")
    {
        var desc = "";

        if (Information == null)
            return desc;

        if (Information.list == null)
            return desc;

        // Top
        foreach (FormatReference entry in Information.list)
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

    public string GetTypeForProperty(string fieldKey)
    {
        string typeName = "";

        if (Information == null)
            return typeName;

        foreach (var entry in Information.list)
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

    public FormatEnumEntry GetEnumForProperty(string fieldKey)
    {
        FormatEnumEntry formatEnum = null;
        string enumName = "";

        if (Information == null)
            return formatEnum;

        foreach (var entry in Information.list)
        {
            foreach(var subentry in entry.members)
            {
                if (subentry.id == fieldKey)
                {
                    enumName = GetAttributeContents(subentry.attributes, "Enum");
                }
            }
        }

        if(enumName != "")
        {
            formatEnum = Enums.list.Find(x => x.id == enumName);
        }

        return formatEnum;
    }

    public string GetAttributeContents(string attributeStr, string targetAttribute)
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
    public bool IsHiddenProperty(string key)
    {
        return IsSpecifiedProperty(key, "IsHidden");
    }

    // Overrides input with checkbox
    public bool IsBooleanProperty(string key)
    {
        return IsSpecifiedProperty(key, "IsBool");
    }

    public bool IsSpecifiedProperty(string key, string attribute)
    {
        if (Information == null)
            return false;

        if (Information.list == null)
            return false;

        // Top
        foreach (FormatReference entry in Information.list)
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

    public bool IsSpecifiedAttribute(string rawAttributes, string specificAttribute)
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
