using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.MapGroupBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(MapGroupResource))]
[JsonSerializable(typeof(MapGroupReference))]
[JsonSerializable(typeof(MapGroupMember))]
public partial class MapGroupResourceSerializationContext
    : JsonSerializerContext
{ }

public class MapGroupResource
{
    public List<MapGroupReference> list { get; set; }
}

public class MapGroupReference
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string category { get; set; }
    public List<MapGroupMember> members { get; set; }
}

public class MapGroupMember
{
    public string id { get; set; }
}
