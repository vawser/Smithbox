using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudioCore.Application;

// -------------- File Dictionary --------------
#region File Dictionary
public class FileDictionary
{
    public List<FileDictionaryEntry> Entries { get; set; }
}
public class FileDictionaryEntry
{
    /// <summary>
    /// The archive this entry belongs to.
    /// </summary>
    public string Archive { get; set; }

    /// <summary>
    /// The relative path for this entry
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The folder path for this entry (excludes the filename and extension)
    /// </summary>
    public string Folder { get; set; }

    /// <summary>
    /// The file name for this entry (excludes extension)
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    /// The extension for this entry (ignoring .dcx)
    /// </summary>
    public string Extension { get; set; }
}
#endregion

// -------------- Aliases --------------
#region Aliases



public enum AliasType
{
    [Display(Name = "None")]
    None,
    [Display(Name = "Assets")]
    Assets,
    [Display(Name = "Characters")]
    Characters,
    [Display(Name = "Cutscenes")]
    Cutscenes,
    [Display(Name = "Event Flags")]
    EventFlags,
    [Display(Name = "Gparams")]
    Gparams,
    [Display(Name = "Map Pieces")]
    MapPieces,
    [Display(Name = "Map Names")]
    MapNames,
    [Display(Name = "Movies")]
    Movies,
    [Display(Name = "Particles")]
    Particles,
    [Display(Name = "Parts")]
    Parts,
    [Display(Name = "Sounds")]
    Sounds,
    [Display(Name = "Talk Scripts")]
    TalkScripts,
    [Display(Name = "Time Acts")]
    TimeActs
}

public class AliasStore : Dictionary<AliasType, List<AliasEntry>>;

public class AliasEntry
{
    public string ID { get; set; }
    public string Name { get; set; }
    public List<string> Tags { get; set; }
}
#endregion

// -------------- Project Enums --------------
#region Project Enums
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
#endregion

// -------------- Format Information --------------
#region Format Information
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
#endregion

// -------------- Legacy Project JSON --------------
#region Legacy Project JSON
public class LegacyProjectJSON
{
    public string ProjectName { get; set; }
    public string GameRoot { get; set; }
    public ProjectType GameType { get; set; }

    public List<string> PinnedParams { get; set; }
    public Dictionary<string, List<int>> PinnedRows { get; set; }
    public Dictionary<string, List<string>> PinnedFields { get; set; }

    public bool UseLooseParams { get; set; }
    public bool PartialParams { get; set; }
    public string LastFmgLanguageUsed { get; set; }

    public LegacyProjectJSON() { }

    public LegacyProjectJSON(ProjectEntry curProject)
    {
        ProjectName = curProject.ProjectName;
        GameRoot = curProject.DataPath;
        GameType = curProject.ProjectType;

        PinnedParams = new();
        PinnedRows = new();
        PinnedFields = new();

        UseLooseParams = false;

        // Account for this for DS3 projects
        if (curProject.ProjectType is ProjectType.DS3)
        {
            UseLooseParams = CFG.Current.UseLooseParams;
        }

        PartialParams = false;

        LastFmgLanguageUsed = "engus";
    }
}


#endregion