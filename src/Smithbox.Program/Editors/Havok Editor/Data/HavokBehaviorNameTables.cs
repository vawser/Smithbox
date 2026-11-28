using StudioCore.Application;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The game's global behavior name tables: action/eventnameid.txt, variablenameid.txt and statenameid.txt.
/// The game reads them at startup and gives each event, variable and state name one global ID; the event and
/// variable IDs seed the Havok world's symbol linkers, and the state IDs name a character's active states (a name
/// missing from statenameid.txt resolves to no ID). After a behavior save, the names the saved graphs use that a
/// table lacks are appended to it with the next IDs, and the table is written to the project.
/// Only games that ship the tables are touched: a table the game doesn't have is skipped.
/// The text editing is in <see cref="HavokBehaviorNameTableText"/>: existing lines and IDs are kept byte for byte,
/// so vanilla IDs never move.
/// </summary>
public static class HavokBehaviorNameTables
{
    public enum Table { Event, Variable, State }

    private static readonly (Table Table, string Path)[] Paths =
    {
        (Table.Event, "/action/eventnameid.txt"),
        (Table.Variable, "/action/variablenameid.txt"),
        (Table.State, "/action/statenameid.txt"),
    };

    /// <summary>
    /// The event, variable and state names a behavior graph uses
    /// </summary>
    public static void CollectNames(HavokBehaviorSymbols symbols, Dictionary<Table, List<string>> names)
    {
        if (symbols == null)
            return;

        Add(names, Table.Event, symbols.EventNames);
        Add(names, Table.Variable, symbols.VariableNames);

        var states = new List<string>();

        foreach (var obj in symbols.Objects)
        {
            if (obj.GetType().Name is "StateInfo" or "hkbStateMachineStateInfo"
                && HavokBehaviorSymbols.GetValue(obj, "m_name") is string name)
            {
                states.Add(name);
            }
        }

        Add(names, Table.State, states);
    }

    private static void Add(Dictionary<Table, List<string>> names, Table table, System.Collections.IList list)
    {
        if (list == null)
            return;

        if (!names.TryGetValue(table, out var target))
            names[table] = target = new();

        foreach (var entry in list)
        {
            if (entry is string s && s.Length > 0)
                target.Add(s);
        }
    }

    /// <summary>
    /// Append the names each table lacks and write the changed tables to the project
    /// </summary>
    public static void Update(ProjectEntry project, Dictionary<Table, List<string>> names)
    {
        foreach (var (table, path) in Paths)
        {
            if (!names.TryGetValue(table, out var wanted) || wanted.Count == 0)
                continue;

            try
            {
                UpdateTable(project, path, wanted);
            }
            catch (Exception ex)
            {
                Smithbox.LogError(typeof(HavokBehaviorNameTables), LOC.Get("HAVOK_Data_Name_Table_Failed", path), ex);
            }
        }
    }

    private static void UpdateTable(ProjectEntry project, string path, List<string> wanted)
    {
        var raw = project.VFS.ProjectFS.FileExists(path)
            ? project.VFS.ProjectFS.ReadFile(path)
            : project.VFS.FS.ReadFile(path);

        // The game has no such table
        if (raw == null)
            return;

        var result = HavokBehaviorNameTableText.Append(raw.Value.Span, wanted, out var missing);

        if (result == null)
            return;

        project.VFS.ProjectFS.WriteFile(path, result);

        Smithbox.Log(typeof(HavokBehaviorNameTables), LOC.Get("HAVOK_Data_Name_Table_Added", path, missing.Count, string.Join(", ", missing.Take(10))));
    }
}
