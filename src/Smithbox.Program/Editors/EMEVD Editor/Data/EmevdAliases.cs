using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MetadataEditor;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The project aliases the EMEVD Editor shows: Map Names for the files (m10_00_00_00) and Events for the events.
/// An event alias is stored per file, as file:id (m10_00_00_00:10002000), since map files reuse local IDs such as 0
/// and 50. An event without an alias shows its .emeld name, if any.
/// </summary>
public class EmevdAliases
{
    private readonly ProjectEntry Project;

    /// <summary>Bumped on every alias change made here, so lists that cache what they show rebuild</summary>
    public int Version { get; private set; }

    public EmevdAliases(ProjectEntry project)
    {
        Project = project;
    }

    private AliasStore Store => Project.Handler.ProjectData?.Aliases;

    public static string EventKey(EmevdFileWrapper file, EMEVD.Event ev) => $"{file.Name}:{ev.ID}";

    /// <summary>The file's Map Names alias; empty if none</summary>
    public string FileAlias(EmevdFileWrapper file)
    {
        return Find(ProjectAliasType.MapNames, file.Name)?.Name ?? "";
    }

    /// <summary>The event's own alias; empty if none</summary>
    public string EventAlias(EmevdFileWrapper file, EMEVD.Event ev)
    {
        return Find(ProjectAliasType.Events, EventKey(file, ev))?.Name ?? "";
    }

    /// <summary>What the event list shows for an event: its alias, else its .emeld name, else empty</summary>
    public string EventDisplayName(EmevdFileWrapper file, EMEVD.Event ev)
    {
        var alias = EventAlias(file, ev);
        return !string.IsNullOrEmpty(alias) ? alias : ev.Name ?? "";
    }

    public void SetFileAlias(EmevdFileWrapper file, string name) => Set(ProjectAliasType.MapNames, file.Name, name);

    public void SetEventAlias(EmevdFileWrapper file, EMEVD.Event ev, string name) => Set(ProjectAliasType.Events, EventKey(file, ev), name);

    private AliasEntry Find(ProjectAliasType type, string id)
    {
        var store = Store;
        if (store == null || !store.TryGetValue(type, out var list) || list == null)
            return null;

        return list.FirstOrDefault(e => string.Equals(e.ID, id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Adds, renames or (with an empty name) removes an alias, then writes that alias type to the project's alias
    /// folder, as the other editors' Update Alias menus do.
    /// </summary>
    private void Set(ProjectAliasType type, string id, string name)
    {
        var store = Store;
        if (store == null)
            return;

        if (!store.TryGetValue(type, out var list) || list == null)
        {
            list = new List<AliasEntry>();
            store[type] = list;
        }

        name = name?.Trim() ?? "";
        var existing = list.FirstOrDefault(e => string.Equals(e.ID, id, StringComparison.OrdinalIgnoreCase));

        if (name.Length == 0)
        {
            if (existing == null)
                return;

            list.Remove(existing);
        }
        else if (existing != null)
        {
            existing.Name = name;
        }
        else
        {
            list.Add(new AliasEntry { ID = id, Name = name, Tags = new() });
        }

        Smithbox.Orchestrator.ProjectEditor.AliasMenu.SaveIndividualAlias(type);
        Version++;

        Smithbox.Log(this, LOC.Get("EMEVD_Alias_Log_Updated", id, name.Length == 0 ? LOC.Get("EMEVD_Alias_Removed") : name));
    }
}
