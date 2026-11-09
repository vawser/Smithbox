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
            foreach (var binder in binders)
            {
                if (token.IsCancellationRequested)
                    return;

                try
                {
                    if (FS.FileExists(binder.Path))
                    {
                        var reader = new BND4Reader(FS.ReadFile(binder.Path).Value);
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

                            if (!TryParseId(file.Name, out var id))
                                continue;
                            if (!entries.TryGetValue(id, out var entry))
                            {
                                entry = new FxEntry { Id = id, Alias = aliases.GetValueOrDefault(id, "") };
                                entries[id] = entry;
                            }
                            entry.Binders.Add(new FxBinderRef(binder, file.Name));

                            // Keep the effect's bytes, packed, from the first binder that has it (copies in
                            // other binders are identical), so selecting it later never decompresses a binder
                            if (entry.Packed == null)
                            {
                                var bytes = reader.ReadFile(file);
                                entry.Packed = Pack(bytes.Span);
                                entry.PackedLength = bytes.Length;
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

    /// <summary>s88300.flver(.dcx), with any folder, to s88300.</summary>
    public static bool TryParseModel(string internalName, out string name)
    {
        name = Path.GetFileName(internalName.Replace('\\', '/'));
        if (name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        if (!name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase))
            return false;
        name = name[..^6];
        return name.Length > 1 && name[0] is 's' or 'S';
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
    public static bool TryParseId(string internalName, out int id)
    {
        id = 0;
        var name = Path.GetFileName(internalName.Replace('\\', '/'));
        if (!name.EndsWith(".fxr", StringComparison.OrdinalIgnoreCase) || name.Length < 2 || name[0] is not ('f' or 'F'))
            return false;
        return int.TryParse(name[1..^4], out id);
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
                entry.Fxr = FXR.Read(Unpack(entry.Packed, entry.PackedLength));
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
            var reader = new BND4Reader(FS.ReadFile(source.Binder.Path).Value);
            var file = reader.Files.FirstOrDefault(f => f.Name == source.InternalName);
            if (file == null)
            {
                Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Find_Effect", entry.Stem, source.Binder.Path));
                return false;
            }
            entry.Fxr = FXR.Read(reader.ReadFile(file));
            entry.IsDirty = false;
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Read_Effect", entry.Stem, source.Binder.Path), e);
            return false;
        }
    }

    /// <summary>
    /// A separate copy of the effect, read from the packed bytes without loading the entry (for surveys of the whole
    /// bank, which shouldn't keep thousands of effects in memory). Null if it can't be read. Safe off the main thread.
    /// </summary>
    public FXR ReadCopy(FxEntry entry)
    {
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
        entry.IsNew = true;
        entry.IsDirty = true;
        Entries[entry.Id] = entry;
        Version++;
    }

    /// <summary>Takes an unsaved new effect out again (its creation was undone).</summary>
    public void RemoveNew(FxEntry entry)
    {
        if (entry.IsNew && Entries.TryGetValue(entry.Id, out var e) && e == entry)
        {
            Entries.Remove(entry.Id);
            Version++;
        }
    }

    /// <summary>The internal name an effect would have in a binder, from another effect's name there.</summary>
    public static string SiblingName(string internalName, string stem, string extension) => FxBinderOps.SiblingName(internalName, stem, extension);

    /// <summary>Drops unsaved changes: reads the effect again from the project's files.</summary>
    public bool Reload(FxEntry entry)
    {
        // A new effect has nothing saved to go back to
        if (entry.IsNew)
            return true;
        entry.Fxr = null;
        entry.IsDirty = false;
        return Load(entry);
    }

    public void MarkDirty(FxEntry entry)
    {
        if (entry != null)
            entry.IsDirty = true;
    }

    public IEnumerable<FxEntry> DirtyEntries => Entries.Values.Where(e => e.IsLoaded && e.IsDirty);

    // ---------------------------------------------------------------- save

    /// <summary>Saves the given effects. Each binder they touch is read and written once.</summary>
    public async Task<bool> Save(IEnumerable<FxEntry> entries)
    {
        await Task.Yield();

        var list = entries.Where(e => e.IsLoaded).ToList();
        if (list.Count == 0)
        {
            Smithbox.Log(this, LOC.Get("FX_Data_Save_Nothing"));
            return true;
        }

        Smithbox.Log(this, LOC.Get("FX_Data_Save_Start", list.Count, list.Sum(e => e.Binders.Count), Project.Descriptor.ProjectPath));

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

        var byBinder = bytes.Keys
            .SelectMany(e => e.Binders.Select(b => (binder: b.Binder, entry: e, name: b.InternalName)))
            .GroupBy(x => x.binder.Path, StringComparer.OrdinalIgnoreCase);

        var groups = byBinder.ToList();
        SaveTotal = groups.Count;
        SaveDone = 0;

        bool ok = true;
        foreach (var group in groups)
        {
            var binderEntry = group.First().binder;
            SaveCurrent = binderEntry.Filename;
            try
            {
                var bnd = BND4.Read(FS.ReadFile(binderEntry.Path).Value);
                foreach (var (_, entry, name) in group)
                {
                    var file = bnd.Files.FirstOrDefault(f => f.Name == name);
                    if (file == null && entry.IsNew)
                        file = AddEffectFile(bnd, entry, name);
                    if (file == null)
                    {
                        Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Find_Effect", entry.Stem, binderEntry.Path));
                        ok = false;
                        continue;
                    }
                    file.Bytes = bytes[entry];
                    UpdateResourceList(bnd, entry);
                }
                var written = bnd.Write();
                Project.VFS.ProjectFS.WriteFile(binderEntry.Path, written);
                Smithbox.Log(this, LOC.Get("FX_Data_Saved_Binder", binderEntry.Path, group.Count(), written.Length / 1024));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Write_Binder", binderEntry.Path), e);
                ok = false;
            }
            SaveDone++;
        }

        if (ok)
        {
            foreach (var (entry, data) in bytes)
            {
                entry.IsDirty = false;
                entry.IsNew = false;
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
        FxBinderOps.AddEffectFile(bnd, name, entry.Stem, entry.ResourceListFrom >= 0 ? $"f{entry.ResourceListFrom:D9}" : null);

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
