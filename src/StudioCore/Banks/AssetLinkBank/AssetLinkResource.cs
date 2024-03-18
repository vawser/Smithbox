using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.ChrLinkBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(AssetLinkResource))]
[JsonSerializable(typeof(AssetLinkReference))]
public partial class AssetLinkSerializationContext
    : JsonSerializerContext
{ }

public class AssetLinkResource
{
    public List<AssetLinkReference> list { get; set; }
}

public class AssetLinkReference
{
    public string BaseID { get; set; }
    public List<string> AdditionalIDs { get; set; }
}
