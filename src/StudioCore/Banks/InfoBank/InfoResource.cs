using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace StudioCore.Banks.InfoBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(InfoResource))]
[JsonSerializable(typeof(InfoReference))]
public partial class InfoResourceSerializationContext
    : JsonSerializerContext
{ }

public class InfoResource
{
    public List<InfoReference> list { get; set; }
}
