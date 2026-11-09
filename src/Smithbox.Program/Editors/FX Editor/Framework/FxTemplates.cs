using SoulsFormats;
using System.IO;
using System.Threading.Tasks;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Node templates kept with the project, one FXR per template in
/// &lt;project&gt;/.smithbox/Project/FX/Templates/&lt;game&gt;/&lt;name&gt;.fxr: an effect whose root holds the saved node as
/// its only child, with the source effect's states, so a template opens in any FXR tool and is written by the same,
/// round-trip-tested writer as the effects.
/// </summary>
public class FxTemplates
{
    private readonly FxEditorScreen Editor;

    private List<string> Cached;
    private string CachedFolder;

    public FxTemplates(FxEditorScreen editor)
    {
        Editor = editor;
    }

    public string Folder => Path.Join(Editor.Project.Descriptor.ProjectPath, ".smithbox", "Project", "FX", "Templates",
        Editor.Data.Game.ToString());

    /// <summary>The template names for this project's game, read from the folder (cached until it changes here).</summary>
    public IReadOnlyList<string> Names
    {
        get
        {
            var folder = Folder;
            if (Cached == null || CachedFolder != folder)
            {
                CachedFolder = folder;
                Cached = Directory.Exists(folder)
                    ? Directory.GetFiles(folder, "*.fxr").Select(Path.GetFileNameWithoutExtension).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList()
                    : new List<string>();
            }
            return Cached;
        }
    }

    public void Refresh() => Cached = null;

    /// <summary>Makes a name safe for a file name.</summary>
    public static string Clean(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = new string(name.Trim().Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return clean.Length == 0 ? null : clean;
    }

    public bool Exists(string name) => Clean(name) is { } n && File.Exists(Path.Join(Folder, $"{n}.fxr"));

    public bool Save(string name, FxEntry entry, FxrNode node) => Save(name, entry.Fxr, node);

    /// <param name="source">The effect the node comes from, for its states.</param>
    public bool Save(string name, FXR source, FxrNode node, bool quiet = false)
    {
        var clean = Clean(name);
        if (clean == null)
            return false;

        try
        {
            var fxr = FXR.Create(Editor.Data.Game, 0);
            fxr.States = source.States.Select(FxClone.State).ToList();
            if (fxr.States.Count == 0)
                fxr.States.Add(new FxrState());
            fxr.Root.Children.Add(FxClone.Node(node));
            if (fxr.Version == FXR.FXRVersion.Sekiro)
                fxr.RebuildReferences();

            Directory.CreateDirectory(Folder);
            var path = Path.Join(Folder, $"{clean}.fxr");
            File.WriteAllBytes(path, fxr.Write());
            Refresh();
            if (!quiet)
                Smithbox.Log(this, LOC.Get("FX_Template_Saved", clean, path));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Template_Failed_Save", clean), e);
            return false;
        }
    }

    /// <summary>The template's node and its effect's state count; null if it can't be read.</summary>
    public (FxrNode Node, int StateCount)? Load(string name)
    {
        var path = Path.Join(Folder, $"{name}.fxr");
        try
        {
            var fxr = FXR.Read(File.ReadAllBytes(path));
            if (fxr.Root.Children.Count == 0)
                return null;
            return (fxr.Root.Children[0], fxr.States.Count);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Template_Failed_Load", name), e);
            return null;
        }
    }

    // ---------------------------------------------------------------- starter templates

    public const string StarterPrefix = "Starter - ";

    public volatile bool IsBuildingStarters;
    public volatile int StarterDone, StarterTotal;

    /// <summary>
    /// Makes a starter template of each kind from the project's own effects, so none of the game's data ships
    /// with Smithbox: for every appearance type, the smallest root-level Basic node (no children, one-state effect)
    /// whose first config uses it; and the smallest Node Emitter and Levels of Detail branches. Small means fewest
    /// stored words, so the template is the plainest example the game has. Runs in the background.
    /// </summary>
    public void BuildStarters()
    {
        var bank = Editor.Data.Bank;
        if (IsBuildingStarters || bank == null || !bank.IsIndexed)
            return;

        var schema = Editor.Data.Schema;
        var entries = bank.Entries.Values.ToList();
        IsBuildingStarters = true;
        StarterDone = 0;
        StarterTotal = entries.Count;

        Task.Run(() =>
        {
            try
            {
                var best = new Dictionary<string, (int Size, FXR Source, FxrNode Node, int Id)>();
                void Offer(string kind, int size, FXR source, FxrNode node, int id)
                {
                    if (!best.TryGetValue(kind, out var cur) || size < cur.Size)
                        best[kind] = (size, source, node, id);
                }

                foreach (var entry in entries)
                {
                    StarterDone++;
                    var fxr = bank.ReadCopy(entry);
                    if (fxr == null || fxr.States.Count != 1)
                        continue;

                    foreach (var node in fxr.Root.Children)
                    {
                        int size = Size(node);
                        switch (node.Type)
                        {
                            case FxNodeOps.BasicType when node.Children.Count == 0 && node.Configs.Count > 0:
                                foreach (var a in node.Configs[0].Actions)
                                    if (schema.Actions.TryGetValue(a.Type, out var def) && def.IsAppearance)
                                    {
                                        Offer(def.Name, size, fxr, node, entry.Id);
                                        break;
                                    }
                                break;
                            case FxNodeOps.EmitterType when node.Children.Count > 0:
                                Offer("Node Emitter", size, fxr, node, entry.Id);
                                break;
                            case FxNodeOps.LodType when node.Children.Count > 0:
                                Offer("Levels of Detail", size, fxr, node, entry.Id);
                                break;
                        }
                    }
                }

                foreach (var (kind, pick) in best)
                    Save($"{StarterPrefix}{kind}", pick.Source, pick.Node, quiet: true);

                Smithbox.Log(this, LOC.Get("FX_Template_Starters_Built", best.Count, Folder));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FX_Template_Starters_Failed"), e);
            }
            finally
            {
                Refresh();
                IsBuildingStarters = false;
            }
        });
    }

    /// <summary>Stored words in the branch: fields, property fields and modifiers, plus one per action.</summary>
    private static int Size(FxrNode node)
    {
        int size = 0;
        foreach (var n in FXR.Walk(node))
            foreach (var a in FxNodeOps.OwnActions(n))
            {
                size += 1 + a.Fields1.Count + a.Fields2.Count + a.Section10s.Sum(s => s.Count);
                foreach (var p in a.Properties1.Concat(a.Properties2))
                    size += 1 + p.Fields.Count + p.Modifiers.Sum(m => 1 + m.Fields.Count + m.Properties.Sum(mp => 1 + mp.Fields.Count));
            }
        return size;
    }

    /// <summary>Moves a template to the folder's .archive (templates aren't deleted outright).</summary>
    public void Archive(string name)
    {
        var path = Path.Join(Folder, $"{name}.fxr");
        if (!File.Exists(path))
            return;
        try
        {
            var archive = Path.Join(Folder, ".archive");
            Directory.CreateDirectory(archive);
            var target = Path.Join(archive, $"{name}.fxr");
            if (File.Exists(target))
                target = Path.Join(archive, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.fxr");
            File.Move(path, target);
            Refresh();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Template_Failed_Remove", name), e);
        }
    }
}
