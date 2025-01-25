using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(EntitySelectionGroupList))]
[JsonSerializable(typeof(EntitySelectionGroupResource))]

public partial class EntitySelectionGroupListSerializationContext
    : JsonSerializerContext
{ }

public class EntitySelectionGroupList
{
    public List<EntitySelectionGroupResource> Resources { get; set; }
}

public class EntitySelectionGroupResource
{
    public string Name { get; set; }
    public int SelectionGroupKeybind { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Selection { get; set; }
}
