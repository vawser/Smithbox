using StudioCore.Core.ProjectNS;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Resources.JSON;
using Microsoft.VisualBasic;
using StudioCore.Banks.FormatBank;
using System.Text.RegularExpressions;
using StudioCore.Editors.EventScriptEditorNS;
using StudioCore.Utilities;
using System.IO;
using System.Text.Json;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamMeta
{
    public BaseEditor BaseEditor;
    public Project Project;

    public GparamFormatResource Information;
    public GparamFormatEnum Enums;

    public GparamMeta(BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;
    }

    public async Task<bool> Setup()
    {
        // Information
        Task<bool> infoTask = LoadInformation();
        bool infoTaskResult = await infoTask;

        if (!infoTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Failed to load graphics param meta information.");
        }

        // Enums
        Task<bool> enumTask = LoadEnums();
        bool enumTaskResult = await enumTask;

        if (!enumTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Failed to load graphics param meta enums.");
        }

        return true;
    }

    public async Task<bool> LoadInformation()
    {
        await Task.Delay(1000);

        var folder = @$"{AppContext.BaseDirectory}\Assets\GPARAM\{ProjectUtils.GetGameDirectory(Project)}";

        var file = "";
        file = Path.Combine(folder, "Core.json");

        if (File.Exists(file))
        {
            using (var stream = File.OpenRead(file))
            {
                Information = JsonSerializer.Deserialize(stream, SmithboxSerializerContext.Default.GparamFormatResource);
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    public async Task<bool> LoadEnums()
    {
        await Task.Delay(1000);

        var folder = @$"{AppContext.BaseDirectory}\Assets\GPARAM\{ProjectUtils.GetGameDirectory(Project)}";

        var file = "";
        file = Path.Combine(folder, "Enums.json");

        if (File.Exists(file))
        {
            using (var stream = File.OpenRead(file))
            {
                Enums = JsonSerializer.Deserialize(stream, SmithboxSerializerContext.Default.GparamFormatEnum);
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    public string GetClassReferenceName(string classKey)
    {
        var name = "";

        if (Information == null)
            return name;

        if (Information.list == null)
            return name;


        // Top
        foreach (GparamFormatReference entry in Information.list)
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
        foreach (GparamFormatReference entry in Information.list)
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
        foreach (GparamFormatReference entry in Information.list)
        {
            if (entry.id == classKey || entry.id == sharedTypeName)
            {
                // Members
                foreach (GparamFormatMember member in entry.members)
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
        foreach (GparamFormatReference entry in Information.list)
        {
            if (entry.id == classKey || entry.id == sharedTypeName)
            {
                // Members
                foreach (GparamFormatMember member in entry.members)
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

    public GparamFormatEnumEntry GetEnumForProperty(string fieldKey)
    {
        GparamFormatEnumEntry formatEnum = null;
        string enumName = "";

        if (Information == null)
            return formatEnum;

        foreach (var entry in Information.list)
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
        foreach (GparamFormatReference entry in Information.list)
        {
            if (entry.id == key)
            {
                return IsSpecifiedAttribute(entry.attributes, attribute);
            }

            // Members
            foreach (GparamFormatMember member in entry.members)
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
