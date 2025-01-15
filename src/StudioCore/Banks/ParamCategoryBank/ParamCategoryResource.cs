using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.ParamCategoryBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(ParamCategoryResource))]
[JsonSerializable(typeof(ParamCategoryEntry))]
public partial class ParamCategorySerializationContext
    : JsonSerializerContext
{ }

public class ParamCategoryResource
{
    public List<ParamCategoryEntry> Categories { get; set; }
}

public class ParamCategoryEntry
{
    public string DisplayName { get; set; }
    public List<string> Params { get; set; }

    public bool IsProjectSpecific { get; set; } = false;
}
