using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.TextureCorrectionBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(TextureCorrectionResource))]
[JsonSerializable(typeof(TextureCorrectionReference))]
public partial class TextureCorrectionSerializationContext
    : JsonSerializerContext
{ }

public class TextureCorrectionResource
{
    public List<TextureCorrectionReference> list { get; set; }
}

public class TextureCorrectionReference
{
    public string VirtualPath { get; set; }
    public string CorrectedPath { get; set; }
    public string AssetID { get; set; }
}
