using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.SpawnStateBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(SpawnStateResource))]
[JsonSerializable(typeof(SpawnStateEntry))]
[JsonSerializable(typeof(SpawnStatePair))]
public partial class SpawnStateSerializationContext
    : JsonSerializerContext
{ }

public class SpawnStateResource
{
    public List<SpawnStateEntry> list { get; set; }
}

public class SpawnStateEntry
{
    public string id { get; set; }
    public string name { get; set; }
    public List<SpawnStatePair> states { get; set; }
}

public class SpawnStatePair
{
    public string value { get; set; }
    public string name { get; set; }
}
