using StudioCore.Application;
using StudioCore.Editors.MetadataEditor;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The project aliases the ESD Editor shows: ESD Files for the file list's binders, loose ESDs and binder entries, and
/// ESD Machines for the machines.
/// A file alias is keyed by the name the file list shows: a binder or loose ESD by its container name
/// (m10_00_00_00.talkesdbnd, c0000), an ESD in a binder by its own name (t101006000).
/// A machine alias is stored per ESD, as esd:id (t101006000:1, t101006000:2147483647 for x0), since every ESD has its
/// own machine 1. The built-in machine aliases come from the games' .edd files; a machine without an alias shows its
/// .edd name, if the project has one (DS2).
/// </summary>
public class EsdAliases
{
    private readonly ProjectEntry Project;

    /// <summary>Bumped on every alias change made here, so lists that cache what they show rebuild</summary>
    public int Version { get; private set; }

    public EsdAliases(ProjectEntry project)
    {
        Project = project;
    }

    private AliasStore Store => Project.Handler.ProjectData?.Aliases;

    public static string ContainerKey(EsdContainer container) => container.Name;

    public static string FileKey(EsdFileWrapper file) => file.Name;

    public static string MachineKey(EsdFileWrapper file, long id) => $"{file.Name}:{id}";

    /// <summary>The binder's or loose ESD's alias; empty if none</summary>
    public string ContainerAlias(EsdContainer container)
    {
        return Find(ProjectAliasType.EsdFiles, ContainerKey(container))?.Name ?? "";
    }

    /// <summary>The binder entry's alias; empty if none</summary>
    public string FileAlias(EsdFileWrapper file)
    {
        return Find(ProjectAliasType.EsdFiles, FileKey(file))?.Name ?? "";
    }

    /// <summary>The machine's alias entry (its Description holds the original .edd name of a translated one); null if none</summary>
    public AliasEntry MachineAliasEntry(EsdFileWrapper file, long id)
    {
        return Find(ProjectAliasType.EsdMachines, MachineKey(file, id));
    }

    /// <summary>The machine's own alias; empty if none</summary>
    public string MachineAlias(EsdFileWrapper file, long id)
    {
        return MachineAliasEntry(file, id)?.Name ?? "";
    }

    /// <summary>What the editor shows for a machine: its alias, else its .edd name, else empty</summary>
    public string MachineDisplayName(EsdFileWrapper file, long id)
    {
        var alias = MachineAlias(file, id);
        if (!string.IsNullOrEmpty(alias))
            return alias;
        return file.MachineNames.TryGetValue(id, out var name) ? name : "";
    }

    public void SetContainerAlias(EsdContainer container, string name) => Set(ProjectAliasType.EsdFiles, ContainerKey(container), name);

    public void SetFileAlias(EsdFileWrapper file, string name) => Set(ProjectAliasType.EsdFiles, FileKey(file), name);

    public void SetMachineAlias(EsdFileWrapper file, long id, string name) => Set(ProjectAliasType.EsdMachines, MachineKey(file, id), name);

    /// <summary>An alias type's entries by ID, with what it was built from</summary>
    private class Index
    {
        public List<AliasEntry> List;
        public int Count;
        public int Version;
        public Dictionary<string, AliasEntry> ById;
    }

    private readonly Dictionary<ProjectAliasType, Index> Indexes = new();

    /// <summary>
    /// The alias with this ID. The machine aliases run to tens of thousands and labels are looked up every frame, so
    /// this goes through an index, rebuilt when the list is replaced (an alias reload), changes size (the alias editor
    /// adding or removing one) or is edited here. An entry whose ID was changed in the alias editor rebuilds it too.
    /// </summary>
    private AliasEntry Find(ProjectAliasType type, string id)
    {
        var store = Store;
        if (store == null || !store.TryGetValue(type, out var list) || list == null)
            return null;

        if (!Indexes.TryGetValue(type, out var index) || index.List != list || index.Count != list.Count || index.Version != Version)
            index = Rebuild(type, list);

        if (!index.ById.TryGetValue(id, out var entry))
            return null;

        if (!string.Equals(entry.ID, id, StringComparison.OrdinalIgnoreCase))
        {
            index = Rebuild(type, list);
            return index.ById.GetValueOrDefault(id);
        }

        return entry;
    }

    private Index Rebuild(ProjectAliasType type, List<AliasEntry> list)
    {
        var byId = new Dictionary<string, AliasEntry>(list.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var e in list)
            if (e?.ID != null)
                byId.TryAdd(e.ID, e);

        var index = new Index { List = list, Count = list.Count, Version = Version, ById = byId };
        Indexes[type] = index;
        return index;
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
        var existing = Find(type, id);

        if (name.Length == 0)
        {
            if (existing == null)
                return;

            list.Remove(existing);
        }
        else if (existing != null)
        {
            if (existing.Name == name)
                return;

            existing.Name = name;
        }
        else
        {
            list.Add(new AliasEntry { ID = id, Name = name, Tags = new() });
        }

        Smithbox.Orchestrator.ProjectEditor.AliasMenu.SaveIndividualAlias(type);
        Version++;

        Smithbox.Log(this, LOC.Get("ESD_Alias_Log_Updated", id, name.Length == 0 ? LOC.Get("ESD_Alias_Removed") : name));
    }
}
