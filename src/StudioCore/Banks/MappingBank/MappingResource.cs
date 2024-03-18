using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.MappingBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(MappingResource))]
[JsonSerializable(typeof(MappingReference))]
public partial class MappingResourceSerializationContext
    : JsonSerializerContext
{ }

public class MappingResource
{
    public List<MappingReference> list { get; set; }
}

public class MappingReference
{
    public string VirtualPath { get; set; }
    public string OverridePath { get; set; }
    public string AssetID { get; set; }
}
