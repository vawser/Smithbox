using Andre.Formats;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Threading.Tasks;

namespace StudioCore.Editors.FxEditor;

/// <summary>A param row field holding an effect id.</summary>
public record FxParamUse(string Param, int RowId, string RowName, string Field);

/// <summary>
/// What uses each effect and what each effect uses (FX Effect List Actions Plan, phase B): the effects that start it
/// through a Proxy node, the param fields that hold its id, and the textures and models it draws with.
///
/// The effects are scanned on a background task from their packed copies; loaded effects, which may have unsaved
/// edits, are read live whenever asked, so the answers include those edits. Params are read on the main thread when
/// asked for (only the fields the meta tags with the Particles alias). TAE events and MSB parts are not searched.
/// </summary>
public class FxUsage
{
    private readonly FxEditorScreen Editor;

    public FxUsage(FxEditorScreen editor)
    {
        Editor = editor;
    }

    private FxBank Bank => Editor.Data.Bank;

    // ---------------------------------------------------------------- effect scan

    public volatile bool IsScanning;
    public volatile int ScanDone;
    public volatile int ScanTotal;

    /// <summary>Whether a scan has finished (its results may be older than later saves; loaded effects are always live).</summary>
    public bool HasScan { get; private set; }

    private Dictionary<int, List<int>> ScanStarts = new();
    private Dictionary<int, HashSet<string>> ScanResources = new();
    // Written by the scan before HasFinished is set, read on the main thread after
    private (Dictionary<int, List<int>>, Dictionary<int, HashSet<string>>) Finished;
    private volatile bool HasFinished;

    /// <summary>Starts the scan of every effect, unless one is running.</summary>
    public void StartScan()
    {
        if (IsScanning || !Bank.IsIndexed)
            return;

        var entries = Bank.Entries.Values.Where(e => e.Packed != null).ToList();
        var schema = Editor.Data.Schema;
        var game = Editor.Data.Game;
        ScanTotal = entries.Count;
        ScanDone = 0;
        IsScanning = true;

        Task.Run(() =>
        {
            var starts = new Dictionary<int, List<int>>();
            var resources = new Dictionary<int, HashSet<string>>();
            foreach (var entry in entries)
            {
                var fxr = Bank.ReadCopy(entry);
                if (fxr != null)
                {
                    starts[entry.Id] = Targets(fxr);
                    resources[entry.Id] = ResourceNames(fxr, schema, game);
                }
                ScanDone++;
            }
            Finished = (starts, resources);
            HasFinished = true;
        });
    }

    /// <summary>Picks up a finished scan. Call on the main thread.</summary>
    public void Update()
    {
        if (!HasFinished)
            return;
        HasFinished = false;
        (ScanStarts, ScanResources) = Finished;

        // Who starts each effect, so Used By is a lookup rather than a pass over every effect
        ScanStartedBy = new();
        foreach (var (host, targets) in ScanStarts)
            foreach (var t in targets)
            {
                if (!ScanStartedBy.TryGetValue(t, out var hosts))
                    ScanStartedBy[t] = hosts = new();
                hosts.Add(host);
            }

        HasScan = true;
        IsScanning = false;
    }

    private Dictionary<int, List<int>> ScanStartedBy = new();

    /// <summary>The effect ids an effect starts through its Proxy nodes, in tree order, without repeats.</summary>
    public static List<int> Targets(FXR fxr) =>
        FXR.Walk(fxr.Root)
            .Where(n => n.Type == FxNodeOps.ProxyType)
            .Select(FxProxyOps.Target)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

    /// <summary>The effect's textures and models as their file names (s88610, s88300).</summary>
    public static HashSet<string> ResourceNames(FXR fxr, FxrSchema schema, FxrGame game)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var r in FxrResources.Collect(fxr, schema, game))
                if (r.Id > 0 && r.Kind is "texture" or "model")
                    set.Add($"s{r.Id:D5}");
        }
        catch
        {
            // A malformed action can't break the scan of the rest
        }
        return set;
    }

    /// <summary>The effects this one starts: live when it's loaded, else from the scan (null before one).</summary>
    public List<int> Starts(FxEntry entry)
    {
        if (entry.IsLoaded)
            return Targets(entry.Fxr);
        if (ScanStarts.TryGetValue(entry.Id, out var list))
            return list;
        // Not scanned (made after the scan, or the scan hasn't run): read it now, once
        var fxr = Bank.ReadCopy(entry);
        list = fxr == null ? new() : Targets(fxr);
        ScanStarts[entry.Id] = list;
        return list;
    }

    /// <summary>The effects that start <paramref name="id"/>. Needs a scan; loaded effects are read live.</summary>
    public List<FxEntry> StartedBy(int id)
    {
        var result = new HashSet<FxEntry>();
        // Unloaded effects as scanned
        if (ScanStartedBy.TryGetValue(id, out var hosts))
            foreach (var h in hosts)
                if (h != id && Bank.Entries.TryGetValue(h, out var e) && !e.IsLoaded)
                    result.Add(e);
        // Loaded ones as they are now, unsaved edits included (few are loaded at once)
        foreach (var e in Bank.Entries.Values)
            if (e.IsLoaded && e.Id != id && Targets(e.Fxr).Contains(id))
                result.Add(e);
        return result.OrderBy(e => e.Id).ToList();
    }

    /// <summary>The effect's texture and model names: live when it's loaded, else from the scan (empty before one).</summary>
    public HashSet<string> Resources(FxEntry entry)
    {
        if (entry.IsLoaded)
            return ResourceNames(entry.Fxr, Editor.Data.Schema, Editor.Data.Game);
        return ScanResources.TryGetValue(entry.Id, out var set) ? set : new HashSet<string>();
    }

    /// <summary>Whether the effect uses a texture or model whose name starts with s&lt;text&gt; (88610 or s88610).</summary>
    public bool UsesResource(FxEntry entry, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;
        // Compared as numbers written out, so 88610, s88610 and s088610 all match s88610
        var t = text.Trim().TrimStart('s', 'S').TrimStart('0');
        if (t.Length == 0)
            return true;
        return Resources(entry).Any(r => r.Length > 1 && r[1..].TrimStart('0').StartsWith(t, StringComparison.OrdinalIgnoreCase));
    }

    // ---------------------------------------------------------------- params

    private Dictionary<int, List<FxParamUse>> ParamUses;

    /// <summary>When the param scan last ran; it's rerun on request, as param edits don't notify this editor.</summary>
    public DateTime ParamScanTime { get; private set; }

    public bool ParamsAvailable => Editor.Project.Handler.ParamData?.PrimaryBank?.Params != null;

    /// <summary>Reads every param field tagged as an effect id. A few tens of milliseconds on ER's params.</summary>
    public void ScanParams()
    {
        var uses = new Dictionary<int, List<FxParamUse>>();
        var data = Editor.Project.Handler.ParamData;
        var bank = data?.PrimaryBank;
        if (bank?.Params != null)
        {
            foreach (var (name, param) in bank.Params)
            {
                if (param?.AppliedParamdef == null)
                    continue;
                var meta = data.GetParamMeta(param.AppliedParamdef);
                var columns = param.Columns
                    .Where(c => data.GetParamFieldMeta(meta, c.Def)?.ShowParticleEnumList == true)
                    .ToList();
                if (columns.Count == 0)
                    continue;

                foreach (var row in param.Rows)
                    foreach (var col in columns)
                    {
                        if (!FxReferenceHelper.TryGetId(col.GetValue(row), out var id))
                            continue;
                        if (!uses.TryGetValue(id, out var list))
                            uses[id] = list = new();
                        list.Add(new FxParamUse(name, row.ID, row.Name ?? "", col.Def.InternalName));
                    }
            }
        }
        ParamUses = uses;
        ParamScanTime = DateTime.Now;
    }

    /// <summary>The param fields holding the effect's id. Scans the params the first time.</summary>
    public List<FxParamUse> ParamsUsing(int id)
    {
        if (ParamUses == null)
            ScanParams();
        return ParamUses.TryGetValue(id, out var list) ? list : new();
    }

    /// <summary>Opens the param row in the Param Editor.</summary>
    public static void JumpToParam(FxParamUse use) =>
        EditorCommandQueue.AddCommand($@"param/select/-1/{use.Param}/{use.RowId}");
}
