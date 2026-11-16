using SoulsFormats;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The user's comments on the Script window's blocks (the event header and its instructions). They aren't part of the
/// EMEVD: they are kept with the project in &lt;project&gt;/.smithbox/Project/EMEVD/Comments/&lt;file&gt;.json, as
/// { event id: { anchor: text } }, where the anchor is "event" for the event's header, or an instruction's index in
/// the event as saved.
/// In memory a comment belongs to the event or instruction object (as the ESD Editor's EsdComments), so moving a block
/// or undoing a delete keeps its comment. The file is written when the .emevd is saved, since only then do the
/// anchors match the saved file, or straight away when the file has no unsaved changes. Comments whose anchor no
/// longer resolves (the file was changed outside Smithbox) are kept and written back while no other block takes
/// their place.
/// </summary>
public class EmevdComments
{
    public const string EventAnchor = "event";

    private readonly ProjectEntry Project;

    /// <summary>Block (EMEVD.Event, EMEVD.Instruction) → comment</summary>
    private readonly ConditionalWeakTable<object, string> Comments = new();

    /// <summary>The loaded EMEVD objects whose comments have been read in</summary>
    private readonly ConditionalWeakTable<EMEVD, object> Attached = new();

    /// <summary>Per EMEVD: event id → anchor → text of comments whose anchor didn't resolve on load</summary>
    private readonly ConditionalWeakTable<EMEVD, Dictionary<string, Dictionary<string, string>>> Orphans = new();

    /// <summary>Files with a comment change waiting for the file to be saved</summary>
    private readonly HashSet<EmevdFileWrapper> Pending = new();

    private static readonly object Mark = new();

    /// <summary>Bumped on every change (and load), so views can cache what they work out from the comments</summary>
    public int Version { get; private set; }

    public EmevdComments(ProjectEntry project)
    {
        Project = project;
    }

    private string Folder => Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Project", "EMEVD", "Comments");

    private string FileFor(EmevdFileWrapper file) => Path.Join(Folder, $"{file.Name}.json");

    public string Get(object block) => block != null && Comments.TryGetValue(block, out var text) ? text : null;

    public bool Has(object block) => Get(block) != null;

    /// <summary>Sets (or, with empty text, removes) a block's comment, and writes the file if the .emevd is saved</summary>
    public void Set(EmevdFileWrapper file, object block, string text)
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
        Pending.Add(file);
        WriteClean();
    }

    /// <summary>After a save: writes every saved file that had a comment change waiting</summary>
    public void OnSaved() => WriteClean();

    private void WriteClean()
    {
        foreach (var file in Pending.ToList())
        {
            if (file.IsDirty || !file.IsLoaded)
                continue;
            if (Write(file))
                Pending.Remove(file);
        }
    }

    // --- reading ---

    /// <summary>Reads the file's comments onto its events and instructions, once per loaded copy of the file</summary>
    public void EnsureLoaded(EmevdFileWrapper file)
    {
        var emevd = file?.Emevd;
        if (emevd == null || Attached.TryGetValue(emevd, out _))
            return;

        Attached.Add(emevd, Mark);

        if (ReadFile(file) is { } map)
            Attach(emevd, map);

        Version++;
    }

    private Dictionary<string, Dictionary<string, string>> ReadFile(EmevdFileWrapper file)
    {
        var path = FileFor(file);
        if (!File.Exists(path))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EMEVD_Comments_Failed_Load", path), e);
            return null;
        }
    }

    private void Attach(EMEVD emevd, Dictionary<string, Dictionary<string, string>> map)
    {
        // The first event of an ID, as EmevdFileWrapper.FindEvent
        var events = new Dictionary<long, EMEVD.Event>();
        foreach (var ev in emevd.Events)
            events.TryAdd(ev.ID, ev);

        foreach (var (eventKey, anchors) in map)
        {
            if (anchors == null)
                continue;

            var ev = long.TryParse(eventKey, out var id) ? events.GetValueOrDefault(id) : null;
            foreach (var (anchor, text) in anchors)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                if (ev != null && Resolve(ev, anchor) is { } block && !Comments.TryGetValue(block, out _))
                {
                    Comments.AddOrUpdate(block, text);
                }
                else
                {
                    var orphans = Orphans.GetOrCreateValue(emevd);
                    if (!orphans.TryGetValue(eventKey, out var list))
                        orphans[eventKey] = list = new();
                    list[anchor] = text;
                }
            }
        }
    }

    /// <summary>The block an anchor names in an event, or null</summary>
    private static object Resolve(EMEVD.Event ev, string anchor)
    {
        if (anchor == EventAnchor)
            return ev;

        return int.TryParse(anchor, out var i) && i >= 0 && i < ev.Instructions.Count ? ev.Instructions[i] : null;
    }

    // --- writing ---

    /// <summary>Writes the file's comments, by each block's place in the file as it is now (as saved)</summary>
    private bool Write(EmevdFileWrapper file)
    {
        var emevd = file.Emevd;
        var map = new SortedDictionary<string, SortedDictionary<string, string>>(Comparer<string>.Create(CompareKeys));
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);

        SortedDictionary<string, string> For(string eventKey)
        {
            if (!map.TryGetValue(eventKey, out var anchors))
                map[eventKey] = anchors = new(Comparer<string>.Create(CompareKeys));
            return anchors;
        }

        foreach (var ev in emevd.Events)
        {
            var key = ev.ID.ToString();

            void Add(object block, string anchor)
            {
                // A block is written at its first place only; with two events of one ID, the first keeps the place
                if (seen.Add(block) && Get(block) is { } text)
                    For(key).TryAdd(anchor, text);
            }

            Add(ev, EventAnchor);
            for (var i = 0; i < ev.Instructions.Count; i++)
                Add(ev.Instructions[i], i.ToString());
        }

        // Comments that didn't resolve on load, while their place is still free (or their event is gone)
        if (Orphans.TryGetValue(emevd, out var orphans))
        {
            foreach (var (eventKey, lost) in orphans)
                foreach (var (anchor, text) in lost)
                    For(eventKey).TryAdd(anchor, text);
        }

        foreach (var key in map.Where(m => m.Value.Count == 0).Select(m => m.Key).ToList())
            map.Remove(key);

        var path = FileFor(file);
        try
        {
            if (map.Count == 0)
            {
                // Nothing left to keep: the old file goes to the archive rather than being deleted
                if (File.Exists(path))
                {
                    var archive = Path.Join(Folder, ".archive");
                    Directory.CreateDirectory(archive);
                    File.Move(path, Path.Join(archive, $"{file.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.json"));
                }
                return true;
            }

            Directory.CreateDirectory(Folder);
            File.WriteAllText(path, JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true }));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EMEVD_Comments_Failed_Save", path), e);
            return false;
        }
    }

    /// <summary>"event" first, then numbers in numeric order, then anything else by text</summary>
    private static int CompareKeys(string a, string b)
    {
        if (a == b)
            return 0;
        if (a == EventAnchor)
            return -1;
        if (b == EventAnchor)
            return 1;

        var na = long.TryParse(a, out var la);
        var nb = long.TryParse(b, out var lb);
        if (na && nb)
            return la.CompareTo(lb);
        if (na != nb)
            return na ? -1 : 1;
        return string.CompareOrdinal(a, b);
    }
}
