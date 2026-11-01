using StudioCore.Editors.MetadataEditor;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Describes what the parameters of events refer to. Parameters go by their names in the
/// event templates, as the same name means the same thing across the events and the games.
/// </summary>
public class TimeActMeta
{
    public List<TimeActField> Fields { get; set; } = new();
}

public class TimeActField
{
    public string Field { get; set; } = "";

    /// <summary>
    /// Also applies to the parameters that start with the name, such as SpEffectID_LowFP
    /// </summary>
    public bool IsPrefix { get; set; } = false;

    public string Description { get; set; } = "";

    /// <summary>
    /// The params the value is a row of, separated by commas
    /// </summary>
    public string ParamRef { get; set; } = "";

    /// <summary>
    /// The aliases that name the value, as a <see cref="ProjectAliasType" />
    /// </summary>
    public string Alias { get; set; } = "";
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(TimeActMeta))]
[JsonSerializable(typeof(TimeActField))]

internal partial class TimeActEditorJsonSerializerContext : JsonSerializerContext
{
}
