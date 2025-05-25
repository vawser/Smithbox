using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(StoredFmgContainer))]
[JsonSerializable(typeof(StoredFmgWrapper))]
[JsonSerializable(typeof(FMG))]
[JsonSerializable(typeof(FMG.Entry))]

public partial class StoredContainerWrapperSerializationContext
    : JsonSerializerContext
{ }

/// <summary>
/// Represents a container
/// </summary>
public class StoredFmgContainer
{
    public string Name { get; set; }

    public List<StoredFmgWrapper> FmgWrappers { get; set; }

    public StoredFmgContainer() { }
}

/// <summary>
/// Represents a FMG
/// </summary>
public class StoredFmgWrapper
{
    public string Name { get; set; }
    public int ID { get; set; }

    public FMG Fmg { get; set; }

    public StoredFmgWrapper() { }
}