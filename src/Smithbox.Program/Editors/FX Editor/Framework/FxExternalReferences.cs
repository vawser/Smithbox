using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TimeActEditor;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.FxEditor;

/// <summary>A use of an effect outside the sfx binders: a TAE event or an MSB field, with the command that opens it.</summary>
public record FxExternalUse(int Id, string Kind, string Source, string Label, string Command);

/// <summary>
/// References to effects from TAE events and MSB map objects (FX Editor Plan §22, phase E5). Reading every anibnd
/// and every map takes minutes in ER, so it runs only when asked, on a background task, and the result is cached in
/// &lt;project&gt;/.smithbox/Project/FX/References.json for later sessions. The cache is marked out of date when a
/// time act or map file in the project is newer than the scan.
///
/// A value counts when the editors' own meta tags it as an effect id: TAE parameters whose alias is Particles
/// (<see cref="TimeActData.GetFieldMeta(TAE.Event, string)"/>), MSB fields with ParticleAlias. Time acts and maps
/// are read as saved, so unsaved edits in the other editors aren't seen.
/// </summary>
public class FxExternalReferences
{
    private readonly FxEditorScreen Editor;

    public FxExternalReferences(FxEditorScreen editor)
    {
        Editor = editor;
    }

    private ProjectEntry Project => Editor.Project;

    private string CachePath => Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Project", "FX", "References.json");

    private sealed class Cache
    {
        public DateTime ScannedAt { get; set; }
        public int Sources { get; set; }
        public List<FxExternalUse> Uses { get; set; } = new();
    }

    private Dictionary<int, List<FxExternalUse>> ById;
    private bool Loaded;

    public DateTime? ScannedAt { get; private set; }
    public int SourceCount { get; private set; }
    public int UseCount { get; private set; }

    /// <summary>Whether a time act or map in the project changed after the scan (checked when the cache is read or a scan ends).</summary>
    public bool IsStale { get; private set; }

    // Scan progress
    public volatile bool IsScanning;
    public volatile int Done;
    public volatile int Total;
    public volatile string Status = "";
    private CancellationTokenSource Cancellation;
    private volatile Cache Finished;

    public bool TaeAvailable => Project.Handler.TimeActData?.PrimaryBank != null;
    public bool MsbAvailable => Project.Handler.MapData?.PrimaryBank?.Maps != null && MsbGameAdapter.For(Project.Descriptor.ProjectType) != null;

    /// <summary>The TAE and MSB uses of an effect, from the cache (empty until a scan has run).</summary>
    public List<FxExternalUse> UsesOf(int id)
    {
        EnsureLoaded();
        return ById != null && ById.TryGetValue(id, out var list) ? list : new();
    }

    public bool HasScan
    {
        get
        {
            EnsureLoaded();
            return ScannedAt != null;
        }
    }

    private void EnsureLoaded()
    {
        if (Loaded)
            return;
        Loaded = true;
        try
        {
            if (File.Exists(CachePath))
                Apply(JsonSerializer.Deserialize<Cache>(File.ReadAllText(CachePath)));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_References_Failed_Load", CachePath), e);
        }
    }

    private void Apply(Cache cache)
    {
        if (cache == null)
            return;
        ById = cache.Uses.GroupBy(u => u.Id).ToDictionary(g => g.Key, g => g.ToList());
        ScannedAt = cache.ScannedAt;
        SourceCount = cache.Sources;
        UseCount = cache.Uses.Count;
        IsStale = ProjectChangedSince(cache.ScannedAt);
    }

    /// <summary>Whether a time act or map file in the project folder was written after the time.</summary>
    private bool ProjectChangedSince(DateTime time)
    {
        try
        {
            var root = Project.Descriptor.ProjectPath;
            foreach (var dir in new[] { "chr", "asset", "obj", "map" })
            {
                var path = Path.Join(root, dir);
                if (!Directory.Exists(path))
                    continue;
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    var name = file.ToLowerInvariant();
                    if ((name.Contains(".anibnd") || name.Contains(".msb")) && File.GetLastWriteTimeUtc(file) > time)
                        return true;
                }
            }
        }
        catch
        {
            // An unreadable folder isn't a reason to distrust the cache
        }
        return false;
    }

    /// <summary>Picks up a finished scan. Call on the main thread.</summary>
    public void Update()
    {
        var finished = Finished;
        if (finished == null)
            return;
        Finished = null;
        Apply(finished);
        IsScanning = false;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CachePath));
            File.WriteAllText(CachePath, JsonSerializer.Serialize(finished));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_References_Failed_Save", CachePath), e);
        }
        Smithbox.Log(this, LOC.Get("FX_References_Scanned", finished.Uses.Count, finished.Sources));
    }

    public void Cancel() => Cancellation?.Cancel();

    /// <summary>Starts the scan of every time act and map. Sources are gathered here, on the main thread.</summary>
    public void StartScan()
    {
        if (IsScanning)
            return;

        var tae = TaeAvailable ? new TimeActQueryRunner(null, Project).GetGlobalSources() : new();
        var taeData = Project.Handler.TimeActData;
        var maps = MsbAvailable ? Project.Handler.MapData.PrimaryBank.Maps.Keys.OrderBy(e => e.Filename).ToList() : new();
        var adapter = MsbGameAdapter.For(Project.Descriptor.ProjectType);
        var mapFs = Project.Handler.MapData?.PrimaryBank?.TargetFS;
        var mapMeta = Project.Handler.MapData?.Meta;

        Total = tae.Count + maps.Count;
        Done = 0;
        IsScanning = true;
        Cancellation = new CancellationTokenSource();
        var token = Cancellation.Token;

        Task.Run(() =>
        {
            var uses = new List<FxExternalUse>();
            try
            {
                foreach (var item in tae)
                {
                    token.ThrowIfCancellationRequested();
                    Status = item.Source.Name;
                    try { ScanTimeActs(item, taeData, uses, token); }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception e) { Smithbox.LogError(this, LOC.Get("FX_References_Failed_Source", item.Source.Name), e); }
                    Done++;
                }

                var matcher = maps.Count > 0 ? new MsbQueryMatcher(Project) : null;
                foreach (var fileEntry in maps)
                {
                    token.ThrowIfCancellationRequested();
                    Status = fileEntry.Filename;
                    try { ScanMap(fileEntry, adapter, mapFs, mapMeta, matcher, uses); }
                    catch (Exception e) { Smithbox.LogError(this, LOC.Get("FX_References_Failed_Source", fileEntry.Filename), e); }
                    Done++;
                }

                Finished = new Cache { ScannedAt = DateTime.UtcNow, Sources = tae.Count + maps.Count, Uses = uses };
            }
            catch (OperationCanceledException)
            {
                IsScanning = false;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FX_References_Failed"), e);
                IsScanning = false;
            }
        }, token);
    }

    /// <summary>Every event parameter the meta tags as a Particles id, in every time act of the source.</summary>
    private static void ScanTimeActs(TimeActQuerySource item, TimeActData data, List<FxExternalUse> uses, CancellationToken token)
    {
        var source = item.Source;
        var entries = item.Entries ?? source.ReadTimeActs();
        int categorySize = source.AnimationCategorySize;
        bool character = item.ListType == ModelListType.Character;

        foreach (var entry in entries)
        {
            token.ThrowIfCancellationRequested();
            entry.Load();
            if (entry.TimeAct == null)
                continue;

            foreach (var animation in entry.TimeAct.Animations.ToArray())
            {
                long fullId = entry.GetFullAnimationID(animation, categorySize);
                foreach (var ev in animation.Events.ToArray())
                {
                    var readable = entry.GetReadableEvent(ev, item.Template);
                    if (readable?.Parameters == null)
                        continue;
                    foreach (var parameter in readable.Parameters.Template.Values)
                    {
                        if (data.GetFieldMeta(readable, parameter.Name) is not { Alias: "Particles" })
                            continue;
                        if (!FxReferenceHelper.TryGetId(readable.Parameters[parameter.Name], out var id))
                            continue;
                        var eventName = TimeActQueryMatcher.GetEventName(ev, item.Template);
                        uses.Add(new FxExternalUse(id, "TAE", source.Name,
                            $"{source.Name}  {source.FormatAnimationName(fullId)}  {eventName}.{parameter.Name}",
                            character ? $"tae/select/{source.Name}/{fullId}" : null));
                    }
                }
            }
        }
    }

    /// <summary>Every field of every part, region and event the map meta tags with ParticleAlias.</summary>
    private static void ScanMap(FileDictionaryEntry fileEntry, MsbGameAdapter adapter, Andre.IO.VFS.VirtualFileSystem fs,
        MsbMeta meta, MsbQueryMatcher matcher, List<FxExternalUse> uses)
    {
        var msb = MsbQueryRunner.ReadMsb(adapter, fs, fileEntry);
        if (msb == null || meta == null)
            return;
        var mapId = fileEntry.Filename;

        foreach (var entry in matcher.GetEntries(mapId, msb))
        {
            foreach (var field in matcher.GetFields(entry.Object))
            {
                if (field.Property == null || field.Owner == null)
                    continue;
                if (meta.GetFieldMeta(field.Property.Name, field.Owner.GetType()) is not { ShowParticleList: true })
                    continue;
                if (!FxReferenceHelper.TryGetId(field.Value, out var id))
                    continue;
                uses.Add(new FxExternalUse(id, "MSB", mapId,
                    $"{mapId}  {entry.Name}  [{entry.TypeName}]  {field.Path}",
                    $"map/select/{mapId}/{entry.Name}/{entry.Category}"));
            }
        }
    }
}
