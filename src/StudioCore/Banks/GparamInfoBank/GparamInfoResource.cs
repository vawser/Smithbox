using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.GparamBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(GparamInfoResource))]
[JsonSerializable(typeof(GparamInfoReference))]
[JsonSerializable(typeof(GparamInfoMember))]
public partial class GparamInfoResourceSerializationContext
    : JsonSerializerContext
{ }

public class GparamInfoResource
{
    public List<GparamInfoReference> list { get; set; }
}
