using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.TextureAdditionBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(TextureAdditionResource))]
[JsonSerializable(typeof(TextureAdditionReference))]
public partial class TextureAdditionSerializationContext
    : JsonSerializerContext
{ }

public class TextureAdditionResource
{
    public List<TextureAdditionReference> list { get; set; }
}

public class TextureAdditionReference
{
    public string BaseID { get; set; }
    public List<string> AdditionalIDs { get; set; }
}
