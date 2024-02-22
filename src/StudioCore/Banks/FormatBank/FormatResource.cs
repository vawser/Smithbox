using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace StudioCore.Banks.FormatBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(FormatResource))]
[JsonSerializable(typeof(FormatReference))]
[JsonSerializable(typeof(FormatMember))]
public partial class FormatResourceSerializationContext
    : JsonSerializerContext
{ }

public class FormatResource
{
    public List<FormatReference> list { get; set; }
}
