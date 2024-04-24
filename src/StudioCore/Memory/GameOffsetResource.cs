using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Memory;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(GameOffsetResource))]
[JsonSerializable(typeof(GameOffsetReference))]
public partial class GameOffsetResourceSerializationContext
    : JsonSerializerContext
{ }

public class GameOffsetResource
{
    public string exeName { get; set; }
    public List<GameOffsetReference> list { get; set; }
}

public class GameOffsetReference
{
    public string exeVersion { get; set; }
    public string paramBaseAob { get; set; }
    public string paramBaseAobRelativeOffset { get; set; }
    public string paramBase { get; set; }
    public string paramInnerPath { get; set; }
    public string paramCountOffset { get; set; }
    public string paramDataOffset { get; set; }
    public string rowPointerOffset { get; set; }
    public string rowHeaderSize { get; set; }

    public List<string> paramOffsets { get; set; }
    public List<string> itemGibOffsets { get; set; }
}

