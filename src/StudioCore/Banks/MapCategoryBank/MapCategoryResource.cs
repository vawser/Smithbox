using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.MapCategoryBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(MapCategoryResource))]
[JsonSerializable(typeof(MapCategoryEntry))]
public partial class MapCategorySerializationContext
    : JsonSerializerContext
{ }

public class MapCategoryResource
{
    public List<MapCategoryEntry> Categories { get; set; }
}

public class MapCategoryEntry
{
    public bool ForceBottom { get; set; } = false;
    public bool ForceTop { get; set; } = false;

    public string DisplayName { get; set; }
    public List<string> Maps { get; set; }
}
