using StudioCore.Editors.MetadataEditor;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Describes what the parameters of events refer to. Parameters go by their names in the
/// event templates, as the same name means the same thing across the events and the games.
/// Names that differ in meaning between events, such as Unk04, can be limited to the events they apply to.
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

    /// <summary>
    /// The event types the entry applies to. Empty applies it to every event with a parameter of that name.
    /// An entry for the event's type goes before one for every event.
    /// </summary>
    public List<int> EventTypes { get; set; } = new();

    /// <summary>
    /// The annotated name shown in place of the internal one, when annotations are on
    /// </summary>
    public string AltName { get; set; } = "";

    public string Description { get; set; } = "";

    /// <summary>
    /// The params the value is a row of, separated by commas
    /// </summary>
    public string ParamRef { get; set; } = "";

    /// <summary>
    /// The aliases that name the value, as a <see cref="ProjectAliasType" />
    /// </summary>
    public string Alias { get; set; } = "";

    /// <summary>
    /// The project enum that names the values, by its key
    /// </summary>
    public string ProjectEnum { get; set; } = "";

    /// <summary>
    /// A number that is only ever 0 or 1, so it is edited with a checkbox
    /// </summary>
    public bool IsBool { get; set; } = false;

    /// <summary>
    /// The game doesn't read the value. It is shown dimmed, and can be hidden.
    /// </summary>
    public bool Obsolete { get; set; } = false;

    /// <summary>
    /// Draws a line after the field, to split the parameters into groups
    /// </summary>
    public bool Separator { get; set; } = false;

    /// <summary>
    /// The value the field usually has, as text, which it can be reset to
    /// </summary>
    public string DefaultValue { get; set; } = "";
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(TimeActMeta))]
[JsonSerializable(typeof(TimeActField))]
[JsonSerializable(typeof(List<int>))]

internal partial class TimeActEditorJsonSerializerContext : JsonSerializerContext
{
}
