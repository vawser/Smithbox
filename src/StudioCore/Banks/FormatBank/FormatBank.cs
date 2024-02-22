using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text;
using StudioCore.UserProject;
using StudioCore.Banks.AliasBank;
using StudioCore.Editors.MapEditor;
using StudioCore.BanksMain;
using Veldrid;

namespace StudioCore.Banks.FormatBank;

/// <summary>
/// An info bank holds information for annotating formats, such as MSB.
/// An info bank has 1 source: Smithbox.
/// </summary>
public class FormatBank
{
    public FormatContainer _loadedInfoBank { get; set; }

    public bool IsLoadingInfoBank { get; set; }
    public bool mayReloadInfoBank { get; set; }

    private string FormatInfoName = "";

    private FormatBankType formatType;

    public FormatBank(FormatBankType _formatType)
    {
        mayReloadInfoBank = false;

        formatType = _formatType;

        if (formatType is FormatBankType.MSB)
        {
            FormatInfoName = "MSB";
        }

        if (formatType is FormatBankType.FLVER)
        {
            FormatInfoName = "FLVER";
        }

        if (formatType is FormatBankType.GPARAM)
        {
            FormatInfoName = "GPARAM";
        }
    }

    public FormatResource Entries
    {
        get
        {
            if (IsLoadingInfoBank)
            {
                return new FormatResource();
            }

            return _loadedInfoBank.Data;
        }
    }

    public void ReloadFormatBank()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Format Info - Load {FormatInfoName}", TaskManager.RequeueType.None, false,
        () =>
        {
            _loadedInfoBank = new FormatContainer();
            IsLoadingInfoBank = true;

            if (Project.Type != ProjectType.Undefined)
            {
                try
                {
                    _loadedInfoBank = new FormatContainer(formatType);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"FAILED LOAD: {e.Message}");
                }

                IsLoadingInfoBank = false;
            }
            else
                IsLoadingInfoBank = false;
        }));
    }

    public string GetReferenceName(string key, string name)
    {
        // Top
        foreach (FormatReference entry in _loadedInfoBank.Data.list)
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

        // Top
        foreach (FormatReference entry in _loadedInfoBank.Data.list)
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
    public bool IsBooleanProperty(string key)
    {
        // Top
        foreach (FormatReference entry in _loadedInfoBank.Data.list)
        {
            if (entry.id == key)
            {
                return IsBoolAttribute(entry.attributes);
            }

            // Members
            foreach (FormatMember member in entry.members)
            {
                if (member.id == key)
                {
                    return IsBoolAttribute(member.attributes);
                }
            }
        }

        return false;
    }

    public bool IsBoolAttribute(string rawAttributes)
    {
        string[] attributes = rawAttributes.Split(",");
        foreach (string attr in attributes)
        {
            if (attr == "IsBool")
            {
                return true;
            }
        }

        return false;
    }

    public string GetNameForProperty(string baseName, Entity entity)
    {
        var name = baseName;

        foreach (FormatReference entry in _loadedInfoBank.Data.list)
        {
            if (IsTypeMatch(entry, entity))
            {
                // Members
                foreach (FormatMember member in entry.members)
                {
                    if (member.id == baseName)
                    {
                        return member.name;
                    }
                }
            }
        }

        return name;
    }

    public string GetDescriptionForProperty(string baseName, Entity entity)
    {
        var desc = "";

        foreach (FormatReference entry in _loadedInfoBank.Data.list)
        {
            if (IsTypeMatch(entry, entity))
            {
                // Members
                foreach (FormatMember member in entry.members)
                {
                    if (member.id == baseName)
                    {
                        return member.description;
                    }
                }
            }
        }

        return desc;
    }

    public bool IsTypeMatch(FormatReference entry, Entity entity)
    {
        var entryType = entry.type.ToString();
        var entityType = entity.WrappedObject.GetType().ToString();

        if(entryType == entityType)
        {
            return true;
        }

        return false;
    }

}
