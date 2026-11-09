using SoulsFormats;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The user's notes on nodes, kept with the project in &lt;project&gt;/.smithbox/Project/FX/Notes/&lt;stem&gt;.json, keyed
/// by each node's child-index path ("0/2/1") in the effect as saved. In memory a note belongs to the node object, so
/// moving a node or undoing a delete keeps its note. The file is written when the effect is saved, since only then do
/// the paths match the saved effect, or straight away when the effect has no unsaved changes.
/// </summary>
public class FxNotes
{
    private readonly FxEditorScreen Editor;

    private readonly ConditionalWeakTable<FxrNode, string> Notes = new();

    /// <summary>The loaded FXR objects whose notes have been read in.</summary>
    private readonly ConditionalWeakTable<FXR, object> Attached = new();

    /// <summary>Effects with a note change waiting for the effect to be saved.</summary>
    private readonly HashSet<int> Pending = new();

    private static readonly object Mark = new();

    public FxNotes(FxEditorScreen editor)
    {
        Editor = editor;
    }

    private string Folder => Path.Join(Editor.Project.Descriptor.ProjectPath, ".smithbox", "Project", "FX", "Notes");

    private string FileFor(FxEntry entry) => Path.Join(Folder, $"{entry.Stem}.json");

    /// <summary>Reads the effect's notes onto its nodes, once per loaded copy of the effect.</summary>
    public void EnsureLoaded(FxEntry entry)
    {
        var fxr = entry?.Fxr;
        if (fxr == null || Attached.TryGetValue(fxr, out _))
            return;
        Attached.Add(fxr, Mark);

        var path = FileFor(entry);
        if (!File.Exists(path))
            return;
        try
        {
            var map = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            if (map == null)
                return;
            foreach (var (nodePath, note) in map)
                if (FxNodeOps.FromPath(fxr.Root, nodePath) is { } node && !string.IsNullOrWhiteSpace(note))
                    Notes.AddOrUpdate(node, note);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Notes_Failed_Load", path), e);
        }
    }

    public string Get(FxrNode node) => node != null && Notes.TryGetValue(node, out var note) ? note : null;

    public void Set(FxEntry entry, FxrNode node, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            Notes.Remove(node);
        else
            Notes.AddOrUpdate(node, note.Trim());

        if (entry.IsDirty)
            Pending.Add(entry.Id);
        else
            Write(entry);
    }

    /// <summary>After a save: writes the notes of every saved effect that had a note change waiting.</summary>
    public void OnSaved(IEnumerable<FxEntry> saved)
    {
        foreach (var entry in saved)
            if (!entry.IsDirty && Pending.Remove(entry.Id))
                Write(entry);
    }

    /// <summary>A reloaded effect drops its unsaved note changes along with its other changes.</summary>
    public void OnReloaded(FxEntry entry) => Pending.Remove(entry.Id);

    private void Write(FxEntry entry)
    {
        var fxr = entry.Fxr;
        if (fxr == null)
            return;

        var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var node in FXR.Walk(fxr.Root))
            if (Get(node) is { } note)
                map[FxNodeOps.PathOf(fxr.Root, node)] = note;

        var path = FileFor(entry);
        try
        {
            if (map.Count == 0)
            {
                // Nothing left to keep: the old file goes to the archive rather than being deleted
                if (File.Exists(path))
                {
                    var archive = Path.Join(Folder, ".archive");
                    Directory.CreateDirectory(archive);
                    File.Move(path, Path.Join(archive, $"{entry.Stem}_{DateTime.Now:yyyyMMdd_HHmmss}.json"));
                }
                return;
            }

            Directory.CreateDirectory(Folder);
            File.WriteAllText(path, JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Notes_Failed_Save", path), e);
        }
    }
}
