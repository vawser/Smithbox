using StudioCore.Editor;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using StudioCore.Banks.GameOffsetBank;

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
        TaskManager.Run(new TaskManager.LiveTask($"Format Bank - Load {FormatTitle}", TaskManager.RequeueType.WaitThenRequeue, false, () =>
        {
            try
            {
                Information = BankUtils.LoadFormatResourceJSON(FormatTitle, IsGameSpecific);
                Enums = BankUtils.LoadFormatEnumJSON(FormatTitle, IsGameSpecific);
                Masks = BankUtils.LoadFormatMaskJSON(FormatTitle, IsGameSpecific);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load Format Bank {FormatTitle}: {e.Message}");
            }
        }));
    }

    public Dictionary<string, FormatReference> GetInformationEntries()
    {
        if (Information == null)
            return new Dictionary<string, FormatReference>();

        if (Information.list == null)
            return new Dictionary<string, FormatReference>();

        Dictionary<string, FormatReference> Entries = new Dictionary<string, FormatReference>();

        foreach (var entry in Information.list)
        {
            Entries.Add(entry.id, entry);
        }

        return Entries;
    }

    public Dictionary<string, FormatEnumEntry> GetEnumEntries()
    {
        if (Enums == null)
            return new Dictionary<string, FormatEnumEntry>();

        if (Enums.list == null)
            return new Dictionary<string, FormatEnumEntry>();

        Dictionary<string, FormatEnumEntry> Entries = new Dictionary<string, FormatEnumEntry>();

        foreach (var entry in Enums.list)
        {
            Entries.Add(entry.id, entry);
        }

        return Entries;
    }

    public Dictionary<string, FormatMaskEntry> GetMaskEntries()
    {
        if (Masks == null)
            return new Dictionary<string, FormatMaskEntry>();

        if (Masks.list == null)
            return new Dictionary<string, FormatMaskEntry>();

        Dictionary<string, FormatMaskEntry> Entries = new Dictionary<string, FormatMaskEntry>();

        foreach (var entry in Masks.list)
        {
            Entries.Add(entry.model, entry);
        }

        return Entries;
    }

    public string GetClassReferenceName(string classKey)
    {
        if (Information == null)
            return "";

        if (Information.list == null)
            return "";

        var name = "";

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
        if (Information == null)
            return "";

        if (Information.list == null)
            return "";

        var desc = "";

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
        if (Information == null)
            return "";

        if (Information.list == null)
            return "";

        string desc = "";

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
        if (Information == null)
            return "";

        if (Information.list == null)
            return "";

        string typeName = "";

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
        if (Information == null)
            return new FormatEnumEntry();

        if (Information.list == null)
            return new FormatEnumEntry();

        FormatEnumEntry formatEnum = null;
        string enumName = "";

        foreach(var entry in Information.list)
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
