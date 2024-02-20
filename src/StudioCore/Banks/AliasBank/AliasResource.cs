using StudioCore.Banks.AliasBank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.AliasBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(AliasResource))]
[JsonSerializable(typeof(AliasReference))]
public partial class AliasResourceSerializationContext
    : JsonSerializerContext
{ }

public class AliasResource
{
    public List<AliasReference> list { get; set; }
}

