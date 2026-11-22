namespace StudioCore.Port;

/// <summary>
/// The param rows a port needs, built on the port's thread without touching any param. The Map Editor's Port tool
/// applies it through the Param Editor once the port has finished (<see cref="StudioCore.Editors.MapEditor.MapPortParams"/>),
/// so the rows are an undoable edit in the loaded params, written to regulation.bin when the params are saved.
/// </summary>
public sealed class PortParamPlan
{
    /// <summary>The Param Editor's name of the param the rows go into.</summary>
    public string ParamName { get; init; }

    public List<PortParamRow> Rows { get; } = new();
}

/// <summary>
/// A row to add, or to replace when the ID is taken (a re-run of the port): a copy of the template row with the ID,
/// name and field values given.
/// </summary>
public sealed class PortParamRow
{
    public int Id { get; init; }
    public string Name { get; init; }
    public int TemplateId { get; init; }

    /// <summary>Field (internal name) -> value from the source game; fields the target param lacks are skipped.</summary>
    public Dictionary<string, object> Values { get; } = new(StringComparer.Ordinal);
}
