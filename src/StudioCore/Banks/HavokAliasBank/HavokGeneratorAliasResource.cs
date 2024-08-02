using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.HavokAliasBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(HavokGeneratorAliasResource))]
[JsonSerializable(typeof(HavokAliasReference))]
public partial class HavokAliasResourceSerializationContext
    : JsonSerializerContext
{ }

public class HavokGeneratorAliasResource
{
    public List<HavokAliasReference> List { get; set; }
}

public class HavokAliasReference
{
    public string ID { get; set; }
    public List<string> Generators { get; set; }
}