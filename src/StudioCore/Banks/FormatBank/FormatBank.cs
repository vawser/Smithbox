using StudioCore.Editor;
using System;
using StudioCore.UserProject;
using Veldrid;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1.Cms;
using System.Linq;
using DotNext.Collections.Generic;
using System.Collections.Generic;
using StudioCore.BanksMain;

namespace StudioCore.Banks.FormatBank;

/// <summary>
/// An info bank holds information for annotating formats, such as MSB.
/// An info bank has 1 source: Smithbox.
/// </summary>
public class FormatBank
{
    public FormatContainer _FormatBank { get; set; }

    public bool IsFormatBankLoaded { get; set; }
    public bool CanReloadFormatBank { get; set; }

    private string FormatInfoName = "";

    private FormatBankType FormatBankType;

    private bool IsGameSpecific;

    public FormatBank(FormatBankType formatBankType, bool isGameSpecific)
    {
        IsGameSpecific = isGameSpecific;
        CanReloadFormatBank = false;
        FormatBankType = formatBankType;

        if (FormatBankType is FormatBankType.MSB)
        {
            FormatInfoName = "MSB";
        }

        if (FormatBankType is FormatBankType.FLVER)
        {
            FormatInfoName = "FLVER";
        }

        if (FormatBankType is FormatBankType.GPARAM)
        {
            FormatInfoName = "GPARAM";
        }
    }

    public FormatResource Entries
    {
        get
        {
            if (IsFormatBankLoaded)
            {
                return new FormatResource();
            }

            return _FormatBank.Data;
        }
    }

    public FormatEnum Enums
    {
        get
        {
            if (IsFormatBankLoaded)
            {
                return new FormatEnum();
            }

            return _FormatBank.Enums;
        }
    }

    public void ReloadFormatBank()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Format Info - Load {FormatInfoName}", TaskManager.RequeueType.None, false,
        () =>
        {
            _FormatBank = new FormatContainer();
            IsFormatBankLoaded = true;

            if (Project.Type != ProjectType.Undefined)
            {
                try
                {
                    _FormatBank = new FormatContainer(FormatBankType, IsGameSpecific);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"FAILED LOAD: {e.Message}");
                }

                IsFormatBankLoaded = false;
            }
            else
                IsFormatBankLoaded = false;
        }));
    }

    public string GetReferenceName(string key, string name)
    {
        if (_FormatBank.Data.list == null)
            return key;

        // Top
        foreach (FormatReference entry in _FormatBank.Data.list)
        {
            if (entry.id == key)
            {
                name = entry.name;
            }

            // Members
            foreach (FormatMember member in entry.members)
            {
                if (member.id == key)
                {
                    name = member.name;
                }
            }
        }

        return name;
    }

    public string GetReferenceDescription(string key)
    {
        var desc = "";

        if (_FormatBank.Data.list == null)
            return desc;

        // Top
        foreach (FormatReference entry in _FormatBank.Data.list)
        {
            if (entry.id == key)
            {
                desc = entry.description;
            }

            // Members
            foreach (FormatMember member in entry.members)
            {
                if (member.id == key)
                {
                    desc = member.description;
                }
            }
        }

        return desc;
    }

    public FormatEnumEntry GetEnumForProperty(string fieldKey)
    {
        FormatEnumEntry formatEnum = null;
        string enumName = "";

        foreach(var entry in GparamFormatBank.Bank.Entries.list)
        {
            foreach(var subentry in entry.members)
            {
                if (subentry.id == fieldKey)
                {
                    enumName = GetEnumName(subentry.attributes, "Enum");
                }
            }
        }

        if(enumName != "")
        {
            formatEnum = GparamFormatBank.Bank.Enums.list.Find(x => x.id == enumName);
        }

        return formatEnum;
    }

    public string GetEnumName(string attributeStr, string targetAttribute)
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
        if (_FormatBank.Data.list == null)
            return false;

        // Top
        foreach (FormatReference entry in _FormatBank.Data.list)
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
