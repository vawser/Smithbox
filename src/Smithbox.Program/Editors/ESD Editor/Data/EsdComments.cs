using SoulsFormats;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The user's comments on the state view's blocks (states, commands, transitions). They aren't part of the ESD: they
/// are kept with the project in &lt;project&gt;/.smithbox/Project/ESD/Comments/&lt;container&gt;.json, as
/// { esd name: { machine id: { anchor: text } } }.
/// An anchor is the block's place in its machine, as saved:
///   "12"                a state
///   "12/entry/0"        a command of a state's entry / while / exit list
///   "12/c1/c0"          a transition (c1 of the state's conditions, then c0 of its subconditions)
///   "12/c1/p2"          a pass command of a transition
/// In memory a comment belongs to the block object, so moving a block or undoing a delete keeps its comment. The file
/// is written when the container is saved, since only then do the anchors match the saved ESD, or straight away when
/// the container has no unsaved changes. A shared transition is one object, so its comment shows at every place it
/// appears. Comments whose anchor no longer resolves (the ESD was changed outside Smithbox) are kept and written back
/// while no other block takes their place.
/// </summary>
public class EsdComments
{
    private readonly ProjectEntry Project;

    /// <summary>Block (ESD.State, ESD.Condition, ESD.CommandCall) → comment</summary>
    private readonly ConditionalWeakTable<object, string> Comments = new();

    /// <summary>The loaded ESD objects whose comments have been read in</summary>
    private readonly ConditionalWeakTable<ESD, object> Attached = new();

    /// <summary>Per ESD: machine id → anchor → text of comments whose anchor didn't resolve on load</summary>
    private readonly ConditionalWeakTable<ESD, Dictionary<string, Dictionary<string, string>>> Orphans = new();

    /// <summary>Containers with a comment change waiting for the container to be saved</summary>
    private readonly HashSet<EsdContainer> Pending = new();

    private static readonly object Mark = new();

    /// <summary>Bumped on every change (and load), so views can cache what they work out from the comments</summary>
    public int Version { get; private set; }

    public EsdComments(ProjectEntry project)
    {
        Project = project;
    }

    private string Folder => Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Project", "ESD", "Comments");

    private string FileFor(EsdContainer container) => Path.Join(Folder, $"{container.Name}.json");

    /// <summary>The block a row comments on: its state, command or transition; null for other rows</summary>
    public static object AnchorOf(EsdRow row) => row?.Kind switch
    {
        EsdRowKind.State => row.State,
        EsdRowKind.Command => row.Command,
        EsdRowKind.Condition => row.Condition,
        _ => null
    };

    public string Get(object block) => block != null && Comments.TryGetValue(block, out var text) ? text : null;

    public bool Has(object block) => Get(block) != null;

    /// <summary>Sets (or, with empty text, removes) a block's comment, and writes the file if the container is saved</summary>
    public void Set(EsdFileWrapper file, object block, string text)
    {
        if (file == null || block == null)
            return;

        EnsureLoaded(file);

        text = text?.TrimEnd();
        var old = Get(block);
        if (string.IsNullOrWhiteSpace(text))
        {
            if (old == null)
                return;
            Comments.Remove(block);
        }
        else
        {
            if (old == text)
                return;
            Comments.AddOrUpdate(block, text);
        }

        Version++;
        Pending.Add(file.Container);
        WriteClean();
    }

    /// <summary>After a save: writes every saved container that had a comment change waiting</summary>
    public void OnSaved() => WriteClean();

    private void WriteClean()
    {
        foreach (var container in Pending.ToList())
        {
            if (container.IsDirty || !container.IsLoaded)
                continue;
            if (Write(container))
                Pending.Remove(container);
        }
    }

    // --- reading ---

    /// <summary>Reads the comments of the file's container onto its blocks, once per loaded copy of each ESD</summary>
    public void EnsureLoaded(EsdFileWrapper file)
    {
        if (file?.Esd == null || Attached.TryGetValue(file.Esd, out _))
            return;

        var container = file.Container;
        var map = ReadFile(container);

        foreach (var f in container.Files)
        {
            if (f.Esd == null || Attached.TryGetValue(f.Esd, out _))
                continue;
            Attached.Add(f.Esd, Mark);

            if (map != null && map.TryGetValue(f.Name, out var machines) && machines != null)
                Attach(f.Esd, machines);
        }

        Version++;
    }

    private Dictionary<string, Dictionary<string, Dictionary<string, string>>> ReadFile(EsdContainer container)
    {
        var path = FileFor(container);
        if (!File.Exists(path))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("ESD_Comments_Failed_Load", path), e);
            return null;
        }
    }

    private void Attach(ESD esd, Dictionary<string, Dictionary<string, string>> machines)
    {
        foreach (var (machineKey, anchors) in machines)
        {
            if (anchors == null)
                continue;

            var machine = long.TryParse(machineKey, out var mid) ? esd.StateGroups.GetValueOrDefault(mid) : null;
            foreach (var (anchor, text) in anchors)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                if (machine != null && Resolve(machine, anchor) is { } block && !Comments.TryGetValue(block, out _))
                {
                    Comments.AddOrUpdate(block, text);
                }
                else
                {
                    var orphans = Orphans.GetOrCreateValue(esd);
                    if (!orphans.TryGetValue(machineKey, out var list))
                        orphans[machineKey] = list = new();
                    list[anchor] = text;
                }
            }
        }
    }

    /// <summary>The block an anchor names in a machine, or null</summary>
    private static object Resolve(Dictionary<long, ESD.State> machine, string anchor)
    {
        var parts = anchor.Split('/');
        if (parts.Length == 0 || !long.TryParse(parts[0], out var sid) || !machine.TryGetValue(sid, out var state))
            return null;
        if (parts.Length == 1)
            return state;

        static bool Index(string s, int skip, out int i) => int.TryParse(s.AsSpan(skip), out i) && i >= 0;

        var list = parts[1] switch
        {
            "entry" => state.EntryCommands,
            "while" => state.WhileCommands,
            "exit" => state.ExitCommands,
            _ => null
        };
        if (list != null)
            return parts.Length == 3 && Index(parts[2], 0, out var ci) && ci < list.Count ? list[ci] : null;

        // c{i}/c{j}/.../[p{k}]
        ESD.Condition cond = null;
        var conds = state.Conditions;
        for (var p = 1; p < parts.Length; p++)
        {
            var part = parts[p];
            if (part.StartsWith('c') && Index(part, 1, out var i) && i < conds.Count)
            {
                cond = conds[i];
                conds = cond.Subconditions;
            }
            else if (part.StartsWith('p') && p == parts.Length - 1 && cond != null && Index(part, 1, out var k) && k < cond.PassCommands.Count)
            {
                return cond.PassCommands[k];
            }
            else
            {
                return null;
            }
        }
        return cond;
    }

    // --- writing ---

    /// <summary>Writes the container's comments, by each block's place in the ESD as it is now (as saved)</summary>
    private bool Write(EsdContainer container)
    {
        // Files whose ESD didn't read keep what the file had for them
        var old = ReadFile(container);
        var map = new SortedDictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.Ordinal);

        foreach (var f in container.Files)
        {
            if (f.Esd == null)
            {
                if (old != null && old.TryGetValue(f.Name, out var kept) && kept != null && kept.Count > 0)
                    map[f.Name] = kept;
                continue;
            }

            EnsureLoaded(f);
            var machines = Collect(f.Esd);
            if (machines.Count > 0)
                map[f.Name] = machines;
        }

        var path = FileFor(container);
        try
        {
            if (map.Count == 0)
            {
                // Nothing left to keep: the old file goes to the archive rather than being deleted
                if (File.Exists(path))
                {
                    var archive = Path.Join(Folder, ".archive");
                    Directory.CreateDirectory(archive);
                    File.Move(path, Path.Join(archive, $"{container.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.json"));
                }
                return true;
            }

            Directory.CreateDirectory(Folder);
            File.WriteAllText(path, JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true }));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("ESD_Comments_Failed_Save", path), e);
            return false;
        }
    }

    private Dictionary<string, Dictionary<string, string>> Collect(ESD esd)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();

        foreach (var (mid, machine) in esd.StateGroups.OrderBy(m => m.Key))
        {
            var anchors = new SortedDictionary<string, string>(StringComparer.Ordinal);
            var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);

            void Add(object block, string anchor)
            {
                // A shared block is written at its first place only
                if (seen.Add(block) && Get(block) is { } text)
                    anchors[anchor] = text;
            }

            void Conditions(List<ESD.Condition> list, string prefix)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var c = list[i];
                    var at = $"{prefix}/c{i}";
                    if (seen.Contains(c))
                        continue;
                    Add(c, at);
                    for (var k = 0; k < c.PassCommands.Count; k++)
                        Add(c.PassCommands[k], $"{at}/p{k}");
                    Conditions(c.Subconditions, at);
                }
            }

            foreach (var (sid, state) in machine.OrderBy(s => s.Key))
            {
                var s = sid.ToString();
                Add(state, s);
                for (var i = 0; i < state.EntryCommands.Count; i++) Add(state.EntryCommands[i], $"{s}/entry/{i}");
                for (var i = 0; i < state.WhileCommands.Count; i++) Add(state.WhileCommands[i], $"{s}/while/{i}");
                for (var i = 0; i < state.ExitCommands.Count; i++) Add(state.ExitCommands[i], $"{s}/exit/{i}");
                Conditions(state.Conditions, s);
            }

            // Comments that didn't resolve on load, while their place is still free
            if (Orphans.TryGetValue(esd, out var orphans) && orphans.TryGetValue(mid.ToString(), out var lost))
                foreach (var (anchor, text) in lost)
                    anchors.TryAdd(anchor, text);

            if (anchors.Count > 0)
                result[mid.ToString()] = new Dictionary<string, string>(anchors);
        }

        // Orphans of machines the ESD no longer has
        if (Orphans.TryGetValue(esd, out var all))
            foreach (var (machineKey, lost) in all)
                if (!result.ContainsKey(machineKey) && !(long.TryParse(machineKey, out var m) && esd.StateGroups.ContainsKey(m)) && lost.Count > 0)
                    result[machineKey] = new Dictionary<string, string>(lost);

        return result;
    }
}
