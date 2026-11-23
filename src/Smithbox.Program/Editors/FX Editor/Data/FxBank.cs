using Andre.IO.VFS;
using StudioCore.Editors.MetadataEditor;
using SoulsFormats;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Every effect in the project's sfx binders, by id. The index is built on a background task (binder file
/// tables only); an effect's FXR is read when it is first selected. Saving writes the effect to every binder
/// that holds it, through the project file system.
/// </summary>
public class FxBank : IDisposable
{
    public FxData Data;
    public ProjectEntry Project;

    /// <summary>Effects by id. Filled once indexing finishes; empty before.</summary>
    public SortedDictionary<int, FxEntry> Entries = new();

    // Indexing progress, read by the effect list
    public volatile bool IsIndexing;
    public volatile bool IsIndexed;
    public volatile int IndexedBinders;
    public volatile int TotalBinders;

    // Save progress, shown in the editor's menu bar. Recompressing a large binder (e.g. commoneffects) can take
    // tens of seconds, so the save shows where it is.
    public volatile bool IsSaving;
    public volatile int SaveDone;
    public volatile int SaveTotal;
    public volatile string SaveCurrent;

    /// <summary>Bumped whenever <see cref="Entries"/> is replaced, so lists can rebuild their caches.</summary>
    public int Version;

    private Task _indexTask;
    private CancellationTokenSource _cancel;
    private readonly ConcurrentQueue<SortedDictionary<int, FxEntry>> _finished = new();

    public FxBank(FxData data, ProjectEntry project)
    {
        Data = data;
        Project = project;
    }

    private VirtualFileSystem FS => Project.VFS.FS;

    // ---------------------------------------------------------------- index

    /// <summary>Starts indexing the sfx binders, once. Call every frame; results are picked up by <see cref="Update"/>.</summary>
    public void StartIndexing()
    {
        if (IsIndexing || IsIndexed)
            return;

        var binders = Project.Locator.SfxFiles?.Entries?
            .OrderBy(e => e.Path, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new();

        // DES and DS2 ship some binders twice, as x.ffxbnd and x.ffxbnd.dcx with the same files; the .dcx is the one
        // read (DS2S ships only the .dcx copies)
        if (IsDlse)
        {
            var paths = binders.Select(b => b.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
            binders = binders.Where(b => !(b.Path.EndsWith(".ffxbnd", StringComparison.OrdinalIgnoreCase) && paths.Contains(b.Path + ".dcx"))).ToList();
        }
        bool dlse = IsDlse;
        var effectExtension = EffectExtension;

        TotalBinders = binders.Count;
        IndexedBinders = 0;
        IsIndexing = true;
        _cancel = new CancellationTokenSource();
        var token = _cancel.Token;

        // The alias names are read on the main thread, before the task
        var aliases = new Dictionary<int, string>();
        if (Project.Handler.ProjectData?.Aliases != null
            && Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Particles, out var particles))
        {
            foreach (var a in particles)
                if (int.TryParse(a.ID, out var id))
                    aliases[id] = a.Name;
        }

        _indexTask = Task.Run(() =>
        {
            var entries = new SortedDictionary<int, FxEntry>();
            var textures = new Dictionary<string, List<FxBinderRef>>(StringComparer.OrdinalIgnoreCase);
            var models = new Dictionary<string, List<FxBinderRef>>(StringComparer.OrdinalIgnoreCase);
            // DLSE: every copy of each id, in binder order, grouped into versions once every binder is read
            var copies = new Dictionary<int, List<DlseCopy>>();
            foreach (var binder in binders)
            {
                if (token.IsCancellationRequested)
                    return;

                try
                {
                    if (FS.FileExists(binder.Path))
                    {
                        var reader = dlse ? OpenBinder(FS.ReadFile(binder.Path).Value) : new BND4Reader(FS.ReadFile(binder.Path).Value);

                        // Only a binder the project has saved can differ from the game's: its effects are compared
                        // with the game's copy of the binder (FX Effect List Actions Plan §1)
                        Dictionary<string, byte[]> vanilla = null;
                        if (Project.VFS.ProjectFS.FileExists(binder.Path))
                            vanilla = ReadVanillaEffects(binder.Path);

                        // Each effect's ffxreslist, for copying into binders it's added to
                        var lists = new Dictionary<string, BinderFileHeader>(StringComparer.OrdinalIgnoreCase);
                        foreach (var file in reader.Files)
                            if (file.Name.EndsWith(".ffxreslist", StringComparison.OrdinalIgnoreCase))
                                lists[Path.GetFileNameWithoutExtension(file.Name.Replace('\\', '/'))] = file;

                        foreach (var file in reader.Files)
                        {
                            // Effect textures (s<id>[_type].tpf): which binders hold them, for the viewport
                            if (TryParseTexture(file.Name, out var texture))
                            {
                                if (!textures.TryGetValue(texture, out var list))
                                    textures[texture] = list = new();
                                list.Add(new FxBinderRef(binder, file.Name));
                                continue;
                            }

                            // Effect models (s<id>.flver), the same way
                            if (TryParseModel(file.Name, out var model))
                            {
                                if (!models.TryGetValue(model, out var list))
                                    models[model] = list = new();
                                list.Add(new FxBinderRef(binder, file.Name));
                                continue;
                            }

                            if (!TryParseId(file.Name, effectExtension, out var id))
                                continue;

                            if (dlse)
                            {
                                if (!copies.TryGetValue(id, out var list))
                                    copies[id] = list = new();
                                byte[] original = null;
                                vanilla?.TryGetValue(file.Name, out original);
                                list.Add(new DlseCopy(new FxBinderRef(binder, file.Name), reader.ReadFile(file).ToArray(), vanilla != null, original));
                                continue;
                            }

                            if (!entries.TryGetValue(id, out var entry))
                            {
                                entry = new FxEntry { Id = id, Alias = aliases.GetValueOrDefault(id, "") };
                                entries[id] = entry;
                            }

                            var r = new FxBinderRef(binder, file.Name);
                            entry.Binders.Add(r);
                            entry.OnDisk[binder.Path] = r;

                            if (entry.ResourceList == null && lists.TryGetValue(entry.Stem, out var resourceList))
                                entry.ResourceList = reader.ReadFile(resourceList).ToArray();

                            // Keep the effect's bytes, packed, from the first binder that has it (copies in
                            // other binders are identical), so selecting it later never decompresses a binder
                            if (entry.Packed == null)
                            {
                                var bytes = reader.ReadFile(file);
                                entry.Packed = Pack(bytes.Span);
                                entry.PackedLength = bytes.Length;

                                // How it compares with the game's copy, judged from the binder it's read from
                                if (vanilla != null)
                                {
                                    if (!vanilla.TryGetValue(file.Name, out var original))
                                        entry.Vanilla = FxVanillaState.Added;
                                    else if (!bytes.Span.SequenceEqual(original))
                                    {
                                        entry.Vanilla = FxVanillaState.Modified;
                                        entry.VanillaPacked = Pack(original);
                                        entry.VanillaLength = original.Length;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Index_Binder", binder.Path), e);
                }

                IndexedBinders++;
            }

            if (dlse)
                foreach (var (id, list) in copies)
                    entries[id] = MakeVersions(id, list, aliases.GetValueOrDefault(id, ""));

            // Each binder was decompressed whole (textures included, up to ~350 MB in ER); release those
            // buffers now rather than whenever the GC gets round to them
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: false, compacting: true);

            _finishedTextures = textures;
            _finishedModels = models;
            _finished.Enqueue(entries);
        }, token);
    }

    /// <summary>
    /// Effect textures by name (s88610_a, as the TPF and the texture in it are named), with the binders holding
    /// them, in binder order. Filled by indexing.
    /// </summary>
    public Dictionary<string, List<FxBinderRef>> Textures { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    private volatile Dictionary<string, List<FxBinderRef>> _finishedTextures;

    /// <summary>Effect models by name (s88300), with the binders holding them. Filled by indexing.</summary>
    public Dictionary<string, List<FxBinderRef>> Models { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    private volatile Dictionary<string, List<FxBinderRef>> _finishedModels;

    /// <summary>The game's own copy of a binder: its effects' bytes by internal name. Empty when the game has no such binder.</summary>
    private Dictionary<string, byte[]> ReadVanillaEffects(string path)
    {
        var result = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        var vfs = Project.VFS.VanillaFS;
        if (!vfs.FileExists(path))
            return result;
        var reader = IsDlse ? OpenBinder(vfs.ReadFile(path).Value) : new BND4Reader(vfs.ReadFile(path).Value);
        foreach (var file in reader.Files)
            if (file.Name.EndsWith(EffectExtension, StringComparison.OrdinalIgnoreCase))
                result[file.Name] = reader.ReadFile(file).ToArray();
        return result;
    }

    /// <summary>One binder's copy of a DLSE effect, as read by indexing.</summary>
    private sealed record DlseCopy(FxBinderRef Where, byte[] Bytes, bool VanillaKnown, byte[] Vanilla);

    /// <summary>
    /// The entry of a DLSE id: its copies grouped by content, in binder order, one entry per version (DES keeps
    /// different effects under one id in ds_sfxbnd_main and the other binders). The first version is the bank's entry
    /// and lists them all in <see cref="FxEntry.Versions"/>; each only holds the binders with its content.
    /// </summary>
    private static FxEntry MakeVersions(int id, List<DlseCopy> list, string alias)
    {
        var groups = new List<List<DlseCopy>>();
        foreach (var c in list)
        {
            var group = groups.FirstOrDefault(g => g[0].Bytes.AsSpan().SequenceEqual(c.Bytes));
            if (group == null)
                groups.Add(group = new());
            group.Add(c);
        }

        var versions = new List<FxEntry>();
        foreach (var group in groups)
        {
            var first = group[0];
            var entry = new FxEntry
            {
                Id = id,
                Alias = alias,
                NameStem = Path.GetFileNameWithoutExtension(first.Where.InternalName.Replace('\\', '/')),
                CopyVersions = groups.Count,
                Packed = Pack(first.Bytes),
                PackedLength = first.Bytes.Length,
            };
            // A binder holding the file twice under one name (DS2S sfx9999_Append) counts once: saving writes both
            foreach (var c in group)
            {
                if (!entry.Binders.Contains(c.Where))
                    entry.Binders.Add(c.Where);
                entry.OnDisk[c.Where.Binder.Path] = c.Where;
            }

            // How it compares with the game's copy, judged from the binder it's read from (as for FXR3)
            if (first.VanillaKnown)
            {
                if (first.Vanilla == null)
                    entry.Vanilla = FxVanillaState.Added;
                else if (!first.Vanilla.AsSpan().SequenceEqual(first.Bytes))
                {
                    entry.Vanilla = FxVanillaState.Modified;
                    entry.VanillaPacked = Pack(first.Vanilla);
                    entry.VanillaLength = first.Vanilla.Length;
                }
            }
            versions.Add(entry);
        }

        if (versions.Count > 1)
        {
            versions[0].Versions = versions;
            foreach (var v in versions.Skip(1))
                v.Primary = versions[0];
        }
        return versions[0];
    }

    /// <summary>The effects are DLSE class streams (.ffx in BND3 or BND4 binders: DES, DS2, DS2S).</summary>
    public bool IsDlse => Data.Format == Model.FxFormat.Dlse;

    /// <summary>The effect files' extension in this game's binders.</summary>
    public string EffectExtension => IsDlse ? ".ffx" : ".fxr";

    /// <summary>A BND3 (DES) or BND4 (DS2) binder, compressed or not.</summary>
    private static BinderReader OpenBinder(Memory<byte> bytes)
    {
        if (DCX.Is(bytes))
            bytes = DCX.Decompress(bytes);
        return bytes.Length >= 4 && bytes.Span[..4].SequenceEqual("BND3"u8) ? new BND3Reader(bytes) : new BND4Reader(bytes);
    }

    /// <summary>The game's copy of the effect, read fresh; null when it has none (or it's the same as the project's, see <see cref="ReadCopy"/>).</summary>
    public FXR ReadVanilla(FxEntry entry)
    {
        if (IsDlse)
            return null;
        try
        {
            return entry.Vanilla switch
            {
                FxVanillaState.Modified when entry.VanillaPacked != null => FXR.Read(Unpack(entry.VanillaPacked, entry.VanillaLength)),
                FxVanillaState.Same => ReadCopy(entry),
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>s88300.flver(.dcx), or DS2's s000010.flv, with any folder, to s88300.</summary>
    public static bool TryParseModel(string internalName, out string name)
    {
        name = Path.GetFileName(internalName.Replace('\\', '/'));
        if (name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        if (name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase))
            name = name[..^6];
        else if (name.EndsWith(".flv", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        else
            return false;
        return name.Length > 1 && name[0] is 's' or 'S';
    }

    /// <summary>
    /// The name of the texture (or model) with this id as a binder holds it, whatever its digit count (DES s0003,
    /// DS2 s000002, FXR3 s88610); null when no binder holds one. A typed texture (s88610_a) is found from its id too.
    /// </summary>
    public string ResourceName(Dictionary<string, List<FxBinderRef>> names, int id)
    {
        foreach (var name in names.Keys)
        {
            var digits = name[1..];
            int underscore = digits.IndexOf('_');
            if (underscore >= 0)
                digits = digits[..underscore];
            if (int.TryParse(digits, out var n) && n == id)
                return name;
        }
        return null;
    }

    /// <summary>s88610_a.tpf(.dcx), with any folder, to s88610_a.</summary>
    public static bool TryParseTexture(string internalName, out string name)
    {
        name = Path.GetFileName(internalName.Replace('\\', '/'));
        if (name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        if (!name.EndsWith(".tpf", StringComparison.OrdinalIgnoreCase))
            return false;
        name = name[..^4];
        return name.Length > 1 && name[0] is 's' or 'S';
    }

    // FXRs are mostly zeros and repeated words, so they pack small (the index holds one per effect)
    private static byte[] Pack(ReadOnlySpan<byte> data)
    {
        using var output = new MemoryStream();
        using (var deflate = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionLevel.Fastest))
            deflate.Write(data);
        return output.ToArray();
    }

    private static byte[] Unpack(byte[] packed, int length)
    {
        var result = new byte[length];
        using var deflate = new System.IO.Compression.DeflateStream(new MemoryStream(packed), System.IO.Compression.CompressionMode.Decompress);
        deflate.ReadExactly(result);
        return result;
    }

    /// <summary>Picks up a finished index. Call on the main thread.</summary>
    public void Update()
    {
        if (_finished.TryDequeue(out var entries))
        {
            Entries = entries;
            Textures = _finishedTextures ?? Textures;
            Models = _finishedModels ?? Models;
            IsIndexing = false;
            IsIndexed = true;
            Version++;
            Smithbox.Log(this, LOC.Get("FX_Data_Indexed", entries.Count, TotalBinders));
        }
    }

    /// <summary>f000620400.fxr (with any folder) to 620400.</summary>
    public static bool TryParseId(string internalName, out int id) => TryParseId(internalName, ".fxr", out id);

    /// <summary>f000620400.fxr, or f0000030.ffx with <paramref name="extension"/> ".ffx" (with any folder), to the id.</summary>
    public static bool TryParseId(string internalName, string extension, out int id)
    {
        id = 0;
        var name = Path.GetFileName(internalName.Replace('\\', '/'));
        if (!name.EndsWith(extension, StringComparison.OrdinalIgnoreCase) || name.Length < 2 || name[0] is not ('f' or 'F'))
            return false;
        return int.TryParse(name[1..^extension.Length], out id);
    }

    // ---------------------------------------------------------------- load

    /// <summary>
    /// Reads the effect if it isn't loaded: from the packed copy made while indexing (no binder is read), or,
    /// without one, from its first binder. Returns false on failure (logged).
    /// </summary>
    public bool Load(FxEntry entry)
    {
        if (entry.IsLoaded)
            return true;

        var source = entry.Binders.FirstOrDefault();
        if (source == null)
            return false;

        if (entry.Packed != null)
        {
            try
            {
                SetFile(entry, Unpack(entry.Packed, entry.PackedLength));
                entry.IsDirty = false;
                return true;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Read_Effect", entry.Stem, source.Binder.Path), e);
                return false;
            }
        }

        try
        {
            var reader = IsDlse ? OpenBinder(FS.ReadFile(source.Binder.Path).Value) : new BND4Reader(FS.ReadFile(source.Binder.Path).Value);
            var file = reader.Files.FirstOrDefault(f => f.Name == source.InternalName);
            if (file == null)
            {
                Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Find_Effect", entry.Stem, source.Binder.Path));
                return false;
            }
            SetFile(entry, reader.ReadFile(file));
            entry.IsDirty = false;
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Read_Effect", entry.Stem, source.Binder.Path), e);
            return false;
        }
    }

    /// <summary>Reads the effect's bytes into the entry, as this game's format.</summary>
    private void SetFile(FxEntry entry, Memory<byte> bytes)
    {
        if (IsDlse)
            entry.Ffx = FFXDLSE.Read(bytes);
        else
            entry.Fxr = FXR.Read(bytes);
    }

    /// <summary>
    /// A separate copy of the effect, read from the packed bytes without loading the entry (for surveys of the whole
    /// bank, which shouldn't keep thousands of effects in memory). Null if it can't be read, and for DLSE games
    /// (the surveys read FXRs). Safe off the main thread.
    /// </summary>
    public FXR ReadCopy(FxEntry entry)
    {
        if (IsDlse)
            return null;
        try
        {
            var packed = entry.Packed;
            return packed == null ? null : FXR.Read(Unpack(packed, entry.PackedLength));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Adds an effect made in the editor. It's written into its binders on the next save.</summary>
    public void AddNew(FxEntry entry)
    {
        entry.IsNew = entry.OnDisk.Count == 0;
        if (entry.IsNew)
            entry.Vanilla = FxVanillaState.Added;
        Restore(entry);
    }

    /// <summary>Takes a new effect out again (its creation was undone). Once saved, the next save removes its files.</summary>
    public void RemoveNew(FxEntry entry) => Delete(entry);

    /// <summary>
    /// Effects deleted in the editor whose files are still in binders: the next save removes them
    /// (FX Effect List Actions Plan §2). Their ids stay taken until then.
    /// </summary>
    public List<FxEntry> Deleted = new();

    /// <summary>Whether an effect id is in use, or was deleted and not saved since.</summary>
    public bool IsIdTaken(int id) => Entries.ContainsKey(id) || Deleted.Any(e => e.Id == id);

    /// <summary>The first id from <paramref name="from"/> up that isn't taken.</summary>
    public int FreeId(int from)
    {
        int id = Math.Max(0, from);
        while (IsIdTaken(id))
            id++;
        return id;
    }

    /// <summary>Takes an effect out of the bank; its files go on the next save. Undone by <see cref="Restore"/>.</summary>
    public void Delete(FxEntry entry)
    {
        if (Entries.TryGetValue(entry.Id, out var e) && e == entry)
            Entries.Remove(entry.Id);
        if (entry.OnDisk.Count > 0 && !Deleted.Contains(entry))
            Deleted.Add(entry);
        Version++;
    }

    /// <summary>Puts an effect (back) in the bank; it's written to its binders on the next save.</summary>
    public void Restore(FxEntry entry)
    {
        Deleted.Remove(entry);
        Entries[entry.Id] = entry;
        entry.IsDirty = true;
        Version++;
    }

    /// <summary>Whether a save has anything to do: modified effects, or files to add or remove.</summary>
    public bool HasChanges => Deleted.Count > 0 || AllEntries.Any(e => e.IsLoaded && e.IsDirty);

    /// <summary>Every entry, with every version of the ids that have several (DES).</summary>
    public IEnumerable<FxEntry> AllEntries => Entries.Values.SelectMany(e => e.Versions ?? (IEnumerable<FxEntry>)new[] { e });

    /// <summary>Every file the next save removes: from binders an effect left, and every file of a deleted effect.</summary>
    public List<(FxEntry entry, FxBinderRef where)> PendingRemovals()
    {
        var result = new List<(FxEntry, FxBinderRef)>();
        foreach (var e in Entries.Values)
            if (e.OnDisk.Count > 0 && e.MembershipChanged)
                foreach (var r in e.OnDisk.Values)
                    if (!e.InBinder(r.Binder.Path))
                        result.Add((e, r));
        foreach (var e in Deleted)
            foreach (var r in e.OnDisk.Values)
                result.Add((e, r));
        return result;
    }

    /// <summary>Notes a version change, so lists rebuild (binder membership, aliases).</summary>
    public void Touch() => Version++;

    /// <summary>The internal name an effect would have in a binder, from another effect's name there.</summary>
    public static string SiblingName(string internalName, string stem, string extension) => FxBinderOps.SiblingName(internalName, stem, extension);

    /// <summary>Drops unsaved changes: reads the effect again from the project's files.</summary>
    public bool Reload(FxEntry entry)
    {
        // A new effect has nothing saved to go back to
        if (entry.IsNew)
            return true;
        entry.Fxr = null;
        entry.Ffx = null;
        entry.IsDirty = false;
        return Load(entry);
    }

    public void MarkDirty(FxEntry entry)
    {
        if (entry != null)
            entry.IsDirty = true;
    }

    public IEnumerable<FxEntry> DirtyEntries => AllEntries.Where(e => e.IsLoaded && e.IsDirty);

    // ---------------------------------------------------------------- save

    /// <summary>Saves the given effects. Each binder they touch is read and written once.</summary>
    public async Task<bool> Save(IEnumerable<FxEntry> entries)
    {
        // What to do is settled on the calling (main) thread, before the work moves off it
        if (IsDlse)
        {
            var dlse = entries.Where(e => e.Ffx != null).ToList();
            await Task.Yield();
            return SaveDlse(dlse);
        }
        var list = entries.Where(e => e.Fxr != null).ToList();
        var removals = PendingRemovals();
        var deleted = Deleted.ToList();

        await Task.Yield();

        if (list.Count == 0 && removals.Count == 0)
        {
            Smithbox.Log(this, LOC.Get("FX_Data_Save_Nothing"));
            return true;
        }

        Smithbox.Log(this, LOC.Get("FX_Data_Save_Start", list.Count, list.Sum(e => e.Binders.Count), Project.Descriptor.ProjectPath));
        if (removals.Count > 0)
            Smithbox.Log(this, LOC.Get("FX_Data_Save_Removals", removals.Count));

        // Serialize on the calling thread order, then write per binder
        var bytes = new Dictionary<FxEntry, byte[]>();
        foreach (var entry in list)
        {
            try
            {
                if (entry.Fxr.Version == FXR.FXRVersion.Sekiro)
                    entry.Fxr.RebuildReferences();
                bytes[entry] = entry.Fxr.Write();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Write_Effect", entry.Stem), e);
            }
        }

        // Every binder touched: by a write, or by a removal
        var work = bytes.Keys
            .SelectMany(e => e.Binders.Select(b => (binder: b.Binder, entry: e, r: b, remove: false)))
            .Concat(removals.Select(x => (binder: x.where.Binder, x.entry, r: x.where, remove: true)))
            .GroupBy(x => x.binder.Path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        SaveTotal = work.Count;
        SaveDone = 0;

        bool ok = true;
        var done = new List<(FxEntry entry, FxBinderRef r, bool remove)>();
        foreach (var group in work)
        {
            var binderEntry = group.First().binder;
            SaveCurrent = binderEntry.Filename;
            try
            {
                var bnd = BND4.Read(FS.ReadFile(binderEntry.Path).Value);
                bool groupOk = true;

                // Removals first, so an id deleted and made again in one save gets a fresh file
                foreach (var (_, entry, r, _) in group.Where(x => x.remove))
                    FxBinderOps.RemoveEffectFile(bnd, r.InternalName, entry.Stem);

                foreach (var (_, entry, r, _) in group.Where(x => !x.remove))
                {
                    var file = bnd.Files.FirstOrDefault(f => f.Name == r.InternalName);
                    // A binder the effect wasn't in yet (a new effect, or Add to Binder) gets the file
                    if (file == null && !entry.OnDisk.ContainsKey(binderEntry.Path))
                        file = AddEffectFile(bnd, entry, r.InternalName);
                    if (file == null)
                    {
                        Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Find_Effect", entry.Stem, binderEntry.Path));
                        ok = groupOk = false;
                        continue;
                    }
                    file.Bytes = bytes[entry];
                    UpdateResourceList(bnd, entry);
                }
                var written = bnd.Write();
                Project.VFS.ProjectFS.WriteFile(binderEntry.Path, written);
                Smithbox.Log(this, LOC.Get("FX_Data_Saved_Binder", binderEntry.Path, group.Count(), written.Length / 1024));

                if (groupOk)
                    done.AddRange(group.Select(x => (x.entry, x.r, x.remove)));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Write_Binder", binderEntry.Path), e);
                ok = false;
            }
            SaveDone++;
        }

        // The binders now hold what was written to them
        foreach (var (entry, r, remove) in done)
        {
            if (remove)
                entry.OnDisk.Remove(r.Binder.Path);
            else
                entry.OnDisk[r.Binder.Path] = r;
        }
        foreach (var e in deleted)
            if (e.OnDisk.Count == 0)
                Deleted.Remove(e);

        if (ok)
        {
            foreach (var (entry, data) in bytes)
            {
                entry.IsDirty = false;
                entry.IsNew = false;
                UpdateVanillaState(entry, data);
                // Reloading now shows what was saved
                entry.Packed = Pack(data);
                entry.PackedLength = data.Length;
            }
        }

        SaveCurrent = null;

        // The binders were decompressed and recompressed whole; release those buffers
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: false, compacting: true);

        return ok;
    }

    /// <summary>
    /// Saves DLSE effects (DES, DS2, DS2S): each into the binders holding its version, which are read and written once
    /// each, BND3 or BND4, with the binder's own compression (DES DCX_EDGE, DS2 DFLT). Where the game also ships the
    /// binder uncompressed next to it (x.ffxbnd beside x.ffxbnd.dcx), that copy is written too, so both stay the same.
    /// Effects are only changed, never added or removed (structure edits are FXR3 only).
    /// </summary>
    private bool SaveDlse(List<FxEntry> list)
    {
        if (list.Count == 0)
        {
            Smithbox.Log(this, LOC.Get("FX_Data_Save_Nothing"));
            return true;
        }
        Smithbox.Log(this, LOC.Get("FX_Data_Save_Start", list.Count, list.Sum(e => e.Binders.Count), Project.Descriptor.ProjectPath));

        var bytes = new Dictionary<FxEntry, byte[]>();
        foreach (var entry in list)
        {
            try
            {
                // Ids the effect uses that its resource lists lack are appended (§8.6). Every vanilla list holds its own
                // effect's ids, so an effect whose ids didn't change keeps its lists as they are
                int appended = Model.FfxResources.AppendMissing(entry.Ffx);
                if (appended > 0)
                    Smithbox.Log(this, LOC.Get("FX_Data_Resources_Appended", entry.Stem, appended));
                bytes[entry] = entry.Ffx.Write();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Write_Effect", entry.Stem), e);
            }
        }

        var work = bytes.Keys
            .SelectMany(e => e.Binders.Select(b => (binder: b.Binder, entry: e, r: b)))
            .GroupBy(x => x.binder.Path, StringComparer.OrdinalIgnoreCase)
            .ToList();
        SaveTotal = work.Count;
        SaveDone = 0;

        bool ok = true;
        var saved = new HashSet<FxEntry>(bytes.Keys);
        foreach (var group in work)
        {
            var binderEntry = group.First().binder;
            SaveCurrent = binderEntry.Filename;
            try
            {
                var written = Model.FfxBinders.Write(FS.ReadFile(binderEntry.Path).Value,
                    group.Select(x => (x.r.InternalName, bytes[x.entry])).ToList(), out var missing, out var plain);
                foreach (var name in missing)
                {
                    var entry = group.First(x => x.r.InternalName == name).entry;
                    Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Find_Effect", entry.Stem, binderEntry.Path));
                    saved.Remove(entry);
                    ok = false;
                }
                Project.VFS.ProjectFS.WriteFile(binderEntry.Path, written);
                Smithbox.Log(this, LOC.Get("FX_Data_Saved_Binder", binderEntry.Path, group.Count(), written.Length / 1024));

                // The uncompressed twin the game also ships (DES: every binder; DS2: sfx9999)
                var twin = Model.FfxBinders.TwinOf(binderEntry.Path);
                if (twin != null && FS.FileExists(twin))
                {
                    Project.VFS.ProjectFS.WriteFile(twin, plain);
                    Smithbox.Log(this, LOC.Get("FX_Data_Saved_Binder", twin, group.Count(), plain.Length / 1024));
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Write_Binder", binderEntry.Path), e);
                foreach (var x in group)
                    saved.Remove(x.entry);
                ok = false;
            }
            SaveDone++;
        }

        // An effect is saved once every binder of its version took it
        foreach (var (entry, data) in bytes)
        {
            if (!saved.Contains(entry))
                continue;
            entry.IsDirty = false;
            UpdateVanillaState(entry, data);
            entry.Packed = Pack(data);
            entry.PackedLength = data.Length;
        }

        SaveCurrent = null;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: false, compacting: true);
        return ok;
    }

    public Task<bool> SaveAll() => SaveGuarded(DirtyEntries.ToList());

    /// <summary>One save at a time: a save started while another runs is skipped (and logged).</summary>
    public async Task<bool> SaveGuarded(List<FxEntry> entries)
    {
        if (IsSaving)
        {
            Smithbox.Log(this, LOC.Get("FX_Data_Save_Busy"));
            return false;
        }
        IsSaving = true;
        try
        {
            return await Save(entries);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static BinderFile AddEffectFile(BND4 bnd, FxEntry entry, string name) =>
        FxBinderOps.AddEffectFile(bnd, name, entry.Stem, entry.ResourceListFrom >= 0 ? $"f{entry.ResourceListFrom:D9}" : null, entry.ResourceList);

    /// <summary>
    /// After a save, whether the saved bytes still match the game's. An effect that matched until now had the game's
    /// bytes as its packed copy, so that copy is kept as the vanilla one before it's replaced.
    /// </summary>
    private static void UpdateVanillaState(FxEntry entry, byte[] saved)
    {
        if (entry.Vanilla == FxVanillaState.Added)
            return;
        if (entry.Vanilla == FxVanillaState.Same && entry.VanillaPacked == null)
        {
            entry.VanillaPacked = entry.Packed;
            entry.VanillaLength = entry.PackedLength;
        }
        if (entry.VanillaPacked == null)
            return;
        var original = Unpack(entry.VanillaPacked, entry.VanillaLength);
        entry.Vanilla = original.AsSpan().SequenceEqual(saved) ? FxVanillaState.Same : FxVanillaState.Modified;
    }

    /// <summary>
    /// ER, AC6 and NR keep f&lt;id&gt;.ffxreslist next to each FXR: the build tool's list of the effect's source
    /// resources (textures as s&lt;id&gt;_&lt;type&gt;.tif, models as s&lt;id&gt;.sib), including ones it derives from models
    /// that can't be rebuilt from the FXR. So the stored list is kept as is, and only resources the effect
    /// references directly that are missing from it are appended (FX Editor Plan §2).
    /// </summary>
    private void UpdateResourceList(BND4 bnd, FxEntry entry)
    {
        var listFile = bnd.Files.FirstOrDefault(f =>
            Path.GetFileName(f.Name.Replace('\\', '/')).Equals($"{entry.Stem}.ffxreslist", StringComparison.OrdinalIgnoreCase));
        if (listFile == null)
            return;

        var text = Encoding.ASCII.GetString(listFile.Bytes.Span);
        var lines = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
        var present = new HashSet<string>(lines, StringComparer.OrdinalIgnoreCase);

        int added = 0;
        foreach (var r in FxrResources.Collect(entry.Fxr, Data.Schema, Data.Game))
        {
            if (r.Id <= 0)
                continue;
            string name = r.Kind switch
            {
                "texture" => $"s{r.Id:D5}_{r.TextureType ?? "a"}.tif",
                "model" => $"s{r.Id:D5}.sib",
                _ => null,
            };
            if (name != null && present.Add(name))
            {
                lines.Add(name);
                added++;
            }
        }

        if (added > 0)
            listFile.Bytes = Encoding.ASCII.GetBytes(string.Concat(lines.Select(l => l + "\r\n")));
    }

    public void Dispose()
    {
        _cancel?.Cancel();
        Entries = new();
    }
}
