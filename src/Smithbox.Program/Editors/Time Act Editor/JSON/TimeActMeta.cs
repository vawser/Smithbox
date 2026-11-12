using StudioCore.Editors.MetadataEditor;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// TAE/{Game}/Meta/{Def}.json: what the parameters of each event in a def refer to, and how they are edited.
/// Only the events with such fields are listed.
/// </summary>
public class TimeActMeta
{
    public List<TimeActEventMeta> Events { get; set; } = new();
}

public class TimeActEventMeta
{
    /// <summary>
    /// The event type, as in the def
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// The internal name of the event, for reading the file. The ID is what is matched.
    /// </summary>
    public string Event { get; set; } = "";

    public List<TimeActField> Fields { get; set; } = new();
}

public class TimeActField
{
    /// <summary>
    /// The internal name of the parameter, as the def gives it
    /// </summary>
    public string Field { get; set; } = "";

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
    /// The value is a behavior judge ID, linked to the BehaviorParam rows it picks (BehaviorParam_PC for c0000).
    /// "Variation": the ID is added to the behavior variation of the attacker (the weapon, or the NpcParam row).
    /// "Common": the ID is the row ID as it is.
    /// </summary>
    public string BehaviorRef { get; set; } = "";

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

/// <summary>
/// TAE/{Game}/Annotations/{Language}/{Def}.json: the annotated names and descriptions of the parameters
/// of each event in a def
/// </summary>
public class TimeActAnnotations
{
    public List<TimeActEventAnnotation> Events { get; set; } = new();
}

public class TimeActEventAnnotation
{
    /// <summary>
    /// The event type, as in the def
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// The internal name of the event, for reading the file. The ID is what is matched.
    /// </summary>
    public string Event { get; set; } = "";

    public List<TimeActFieldAnnotation> Fields { get; set; } = new();
}

public class TimeActFieldAnnotation
{
    /// <summary>
    /// The internal name of the parameter, as the def gives it
    /// </summary>
    public string Field { get; set; } = "";

    /// <summary>
    /// The annotated name shown in place of the internal one, when annotations are on
    /// </summary>
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";
}

/// <summary>
/// TAE/{Game}/Groups.json: the event group each event type sits in, in the game's own files, made by
/// Tools/TaeTemplateSurvey groupmap. The editor makes new groups to match, as the group type is often not the
/// event's own type (ER type 129 events are in group type 128).
/// </summary>
public class TimeActGroupMap
{
    public string Game { get; set; } = "";

    public List<TimeActGroupMapEntry> Groups { get; set; } = new();
}

public class TimeActGroupMapEntry
{
    public int EventType { get; set; }

    public long GroupType { get; set; }

    public long DataType { get; set; }

    /// <summary>
    /// How many events of the type the game's files have in groups
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// How many of them are in this group rather than another, from 0 to 1
    /// </summary>
    public double Share { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(TimeActMeta))]
[JsonSerializable(typeof(TimeActEventMeta))]
[JsonSerializable(typeof(TimeActField))]
[JsonSerializable(typeof(TimeActAnnotations))]
[JsonSerializable(typeof(TimeActEventAnnotation))]
[JsonSerializable(typeof(TimeActFieldAnnotation))]
[JsonSerializable(typeof(TimeActGroupMap))]
[JsonSerializable(typeof(TimeActGroupMapEntry))]

internal partial class TimeActEditorJsonSerializerContext : JsonSerializerContext
{
}
