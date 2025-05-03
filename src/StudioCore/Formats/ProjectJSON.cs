using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Formats.JSON;

// -------------- Aliases --------------
public class AliasStore
{
    public List<AliasEntry> Assets { get; set; }
    public List<AliasEntry> Characters { get; set; }
    public List<AliasEntry> Cutscenes { get; set; }
    public List<AliasEntry> EventFlags { get; set; }
    public List<AliasEntry> Gparams { get; set; }
    public List<AliasEntry> MapPieces { get; set; }
    public List<AliasEntry> MapNames { get; set; }
    public List<AliasEntry> Movies { get; set; }
    public List<AliasEntry> Particles { get; set; }
    public List<AliasEntry> Parts { get; set; }
    public List<AliasEntry> Sounds { get; set; }
    public List<AliasEntry> TalkScripts { get; set; }
    public List<AliasEntry> TimeActs { get; set; }
}

public class AliasEntry
{
    public string ID { get; set; }
    public string Name { get; set; }
    public List<string> Tags { get; set; }
}

// -------------- Project Enums --------------
public class ProjectEnumResource
{
    public List<ProjectEnumEntry> List { get; set; }

    public ProjectEnumResource()
    {
        List = new List<ProjectEnumEntry>();
    }
}

public class ProjectEnumEntry
{
    public string DisplayName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public List<ProjectEnumOption> Options { get; set; }

    public ProjectEnumEntry()
    {

    }

    public ProjectEnumEntry(string name, string displayName, string description)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        Options = new List<ProjectEnumOption>();
    }

    public ProjectEnumEntry Clone(ProjectEnumEntry entry)
    {
        return (ProjectEnumEntry)entry.MemberwiseClone();
    }
}

public class ProjectEnumOption : IComparable
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public ProjectEnumOption()
    {

    }

    public ProjectEnumOption(string id, string name, string description)
    {
        ID = id;
        Name = name;
        Description = description;
    }

    public ProjectEnumOption Clone(ProjectEnumOption entry)
    {
        return (ProjectEnumOption)entry.MemberwiseClone();
    }

    public int CompareTo(object obj)
    {
        ProjectEnumOption option = obj as ProjectEnumOption;

        try
        {
            var thisID = int.Parse(ID);
            var compID = int.Parse(option.ID);
            return thisID.CompareTo(compID);
        }
        catch
        {
            // For non-numeric values, compare strings
            return ID.CompareTo(option.ID);
        }
    }
}

// -------------- Format Information --------------
public class FormatResource
{
    public List<FormatReference> list { get; set; }
}

public class FormatReference
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string attributes { get; set; }
    public List<FormatMember> members { get; set; }
}

public class FormatMember
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string attributes { get; set; }
}

public class FormatEnum
{
    public List<FormatEnumEntry> list { get; set; }
}

public class FormatEnumEntry
{
    public string id { get; set; }
    public string name { get; set; }
    public List<FormatEnumMember> members { get; set; }
}

public class FormatEnumMember
{
    public string id { get; set; }
    public string name { get; set; }
}
public class FormatMask
{
    public List<FormatMaskEntry> list { get; set; }
}

public class FormatMaskEntry
{
    public string model { get; set; }
    public List<MaskSection> section_one { get; set; }
    public List<MaskSection> section_two { get; set; }
    public List<MaskSection> section_three { get; set; }
}

public class MaskSection
{
    public string mask { get; set; }
    public string name { get; set; }
}

