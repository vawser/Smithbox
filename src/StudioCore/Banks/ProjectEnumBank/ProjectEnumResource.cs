using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.ProjectEnumBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(ProjectEnumResource))]
[JsonSerializable(typeof(ProjectEnumEntry))]
[JsonSerializable(typeof(ProjectEnumOption))]
public partial class ProjectEnumResourceSerializationContext
    : JsonSerializerContext
{ }

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

public class ProjectEnumOption
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
}