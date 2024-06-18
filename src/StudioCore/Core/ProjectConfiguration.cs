using StudioCore.Banks.AliasBank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using StudioCore.Core;

namespace StudioCore.UserProject;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(ProjectConfiguration))]
public partial class ProjectConfigurationSerializationContext
    : JsonSerializerContext
{ }

public class ProjectConfiguration
{
    public string ProjectName { get; set; } = "";
    public string GameRoot { get; set; } = "";
    public ProjectType GameType { get; set; } = ProjectType.Undefined;

    public List<string> PinnedParams { get; set; } = new();
    public Dictionary<string, List<int>> PinnedRows { get; set; } = new();
    public Dictionary<string, List<string>> PinnedFields { get; set; } = new();

    public bool UseLooseParams { get; set; } = false;

    public bool PartialParams { get; set; } = false;

    public string LastFmgLanguageUsed { get; set; } = "";

    [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData { get; set; }
}
