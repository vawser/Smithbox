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
[JsonSerializable(typeof(TexturePathCorrectionResource))]
[JsonSerializable(typeof(TexturePathCorrectionReference))]
public partial class TexturePathCorrectionSerializationContext
    : JsonSerializerContext
{ }

public class TexturePathCorrectionResource
{
    public List<TexturePathCorrectionReference> list { get; set; }
}

public class TexturePathCorrectionReference
{
    public string VirtualPath { get; set; }
    public string CorrectedPath { get; set; }
    public string AssetID { get; set; }
}
