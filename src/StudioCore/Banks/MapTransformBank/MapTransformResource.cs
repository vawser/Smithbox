using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.MapTransformBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(MapTransformList))]
[JsonSerializable(typeof(MapTransformEntry))]
public partial class MapTransformListSerializationContext
    : JsonSerializerContext
{ }

public class MapTransformList
{
    public List<MapTransformEntry> TransformList { get; set; }
}

public class MapTransformEntry
{
    public string MapID { get; set; }

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }
}

