using StudioCore.Application;
using StudioCore.Memory;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The FX Reloader's work in a running game (DS3, SDT, ER, AC6, NR), over the project's shared GameLink: lists the
/// effects the game has loaded with how many of each are playing, and replaces an effect's data with new FXR bytes.
/// See Claude/Documentation/FX.md (FX Reloader) (F1, F4).
/// The AOBs and offsets come from Assets/FX Reloader/{game}/*.json (FxReloaderOffsets).
///
/// How the game holds effects (offsets as in ER 2.7.2.0; per game in the data files):
///   container = [[[SFX singleton]+60]+28]+160 (CSSfxImp → GXFfxSceneCtrl → GXFfxGraphicsResourceManager → resources)
///   container +20 = sentinel of the definition list, +28 = its length; node +0 next, +10 effect id, +18 wrapper
///   wrapper +0 = the game's copy of the FXR, +8 = how many instances of the effect are playing now
///   LoadBinaryFxr(bytes, size) copies an FXR into the effect heap and returns a new wrapper
///
/// Spawns don't look the node up each time: whatever spawns an effect keeps the wrapper it got earlier. So a send
/// loads the new FXR with the game's own loader and moves only its data pointer into the effect's existing wrapper,
/// the same write EvenTorset's fxr-ws-reloader makes. Instances that are playing keep their pointers into the old
/// data, which is never freed; ones spawned after the send read the new data.
/// </summary>
public class FxReloader
{
    private readonly ProjectEntry Project;

    public FxReloader(ProjectEntry project)
    {
        Project = project;
        Restarter = new FxRestart(project);
    }

    /// <summary>Restarts weapon and SpEffect effects that are playing, so they show a reload</summary>
    public readonly FxRestart Restarter;

    /// <summary>The project's connection to the game, shared with the other live tools</summary>
    public GameLink Link => Project.Handler.GameLink;

    private FxReloaderOffsets OffsetsCache;
    private FxReloaderOffsets Offsets => OffsetsCache ??= FxReloaderOffsets.Load(Project);

    /// <summary>Whether this game has both a game link and FX Reloader offsets</summary>
    public bool IsSupported => Link.IsSupported && Offsets.Groups.Count > 0;

    /// <summary>Why an effect counts as recently detected</summary>
    public enum DetectReason { Loaded, Started }

    /// <summary>
    /// An effect the game has loaded, with how many of it are playing, and when it was last detected: when it loaded,
    /// or when it last started playing (its count went from 0 up). Initial: loaded at the first poll after connecting
    /// and not detected since, so not "recent".
    /// </summary>
    public record LoadedEffect(int Id, int Playing, DateTime DetectedAt, DetectReason Reason, bool Initial);

    public enum ReloadOutcome { Reloaded, NotLoaded, Failed }

    public record ReloadResult(int Id, ReloadOutcome Outcome, string Message, DateTime At);

    public enum AfterReload { Nothing, ReloadWorld, QuitOut }

    /// <summary>An effect to send: its id and FXR bytes</summary>
    public record SendItem(int Id, byte[] Bytes);

    /// <summary>Where a send came from, shown in the history</summary>
    public enum SendSource { Selected, Modified, History, Vanilla }

    /// <summary>
    /// The latest reloaded effect: its wrapper, the sent data now in it, and how many instances are playing, refreshed
    /// by the poll. PlayingAtSend were already playing and keep the old data until they end.
    /// </summary>
    public record SendWatch(int Id, nint Wrapper, nint Data, int PlayingAtSend, DateTime At, WatchState State = WatchState.InUse)
    {
        public int Playing { get; init; } = PlayingAtSend;
    }

    /// <summary>Whether the game still uses the sent version of the watched effect</summary>
    public enum WatchState
    {
        /// <summary>The effect's entry and wrapper hold the sent version</summary>
        InUse,
        /// <summary>The game holds another version: it loaded the effect again (an area or world reload)</summary>
        Replaced,
        /// <summary>The effect isn't loaded any more</summary>
        Unloaded
    }

    // Updated by the background tasks, read by the UI
    public volatile IReadOnlyList<LoadedEffect> Loaded = Array.Empty<LoadedEffect>();
    public volatile bool IsPolling;
    public volatile bool IsReloading;
    public volatile ReloadResult LastReload;
    public volatile SendWatch Watch;
    public DateTime? LastPoll { get; private set; }

    /// <summary>How long the last read of the game took, in milliseconds (shown beside the refresh interval)</summary>
    public volatile int LastPollMs;

    // Why the effects can't be read while the link is attached (not found in this version, not loaded yet), or null
    private volatile string Problem;
    private volatile bool ProblemIsFailure;

    /// <summary>The state to show: the link's, or the reloader's own when the link is attached but effects can't be read</summary>
    public GameLink.LinkState State =>
        Link.State is GameLink.LinkState.Attached && Problem != null && ProblemIsFailure ? GameLink.LinkState.Failed : Link.State;

    public string StateMessage =>
        Link.State is GameLink.LinkState.Attached && Problem != null ? Problem : Link.StateMessage;

    /// <summary>Whether the link is attached and the effect functions were found</summary>
    public bool IsReady => Link.State is GameLink.LinkState.Attached && ResolvedFor == Link.AttachId && Group != null;

    // Found for one attach of the link
    private int ResolvedFor = -1;
    private FxReloaderOffsetEntry Group;
    private Layout Map;
    private nint SfxGlobal, LoadFxrAddr;
    private int[] ManagerFields = Array.Empty<int>();

    // Detection times, kept by the poll while an effect stays loaded; one that unloads and loads again is new
    private record Detection(DateTime At, DetectReason Reason, bool Initial, int Playing);
    private readonly Dictionary<int, Detection> Detections = new();
    private bool HasBaseline;

    /// <summary>The definition list layout, parsed from the offset group</summary>
    private record Layout(int Head, int Count, int NodeId, int NodeWrapper, int WrapperData, int WrapperUses)
    {
        public int NodeSize => Math.Max(8, Math.Max(NodeId + 4, NodeWrapper + 8));

        public static Layout Parse(FxReloaderOffsetEntry g)
        {
            int? H(string s) => GameLinkOffsets.ParseHex(s);

            var values = new[]
            {
                H(g.definitionsHeadOffset), H(g.definitionsCountOffset), H(g.nodeIdOffset), H(g.nodeWrapperOffset),
                H(g.wrapperDataOffset), H(g.wrapperUseCountOffset)
            };

            if (values.Any(v => v == null))
                return null;

            var v = values.Select(x => x.Value).ToArray();
            return new Layout(v[0], v[1], v[2], v[3], v[4], v[5]);
        }
    }

    /// <summary>Disconnects the shared link (other live tools too) and clears the list</summary>
    public void Detach()
    {
        Link.Detach();

        lock (Link.Lock)
            Forget();
    }

    #region Attach

    /// <summary>Connects the link and finds the effect functions for this attach. Call with the link's lock held.</summary>
    private bool EnsureReady()
    {
        if (!Link.EnsureAttached())
        {
            Forget();
            return false;
        }

        if (ResolvedFor == Link.AttachId)
            return Group != null;

        Forget();
        ResolvedFor = Link.AttachId;

        if (!Resolve(out var error))
        {
            SetProblem(error, true);
            return false;
        }

        Problem = null;

        Smithbox.Log(typeof(FxReloader),
            $"[FX Editor] FX Reloader: {Link.ExeName} offsets {Group.exeVersion}, CSSfxImp global 0x{SfxGlobal:X}, " +
            $"manager path +{string.Join(" +", ManagerFields.Select(f => f.ToString("X")))}, LoadBinaryFxr 0x{LoadFxrAddr:X}");

        return true;
    }

    private void SetProblem(string message, bool failure)
    {
        ProblemIsFailure = failure;
        Problem = message;
    }

    /// <summary>Forgets what was found for the last attach, and the list</summary>
    private void Forget()
    {
        ResolvedFor = -1;
        Group = null;
        Map = null;
        SfxGlobal = LoadFxrAddr = 0;
        ManagerFields = Array.Empty<int>();
        Problem = null;
        Loaded = Array.Empty<LoadedEffect>();
        Watch = null;
        Detections.Clear();
        HasBaseline = false;
    }

    /// <summary>
    /// Finds the CSSfxImp global, the path to the manager and LoadBinaryFxr with the first offset group that resolves:
    /// the running version's own group first, then the others
    /// </summary>
    private bool Resolve(out string error)
    {
        error = "";

        var version = Link.ExeVersion;
        var problems = new List<string>();

        foreach (var group in Offsets.GroupsFor(version))
        {
            if (!ResolveGroup(group, out var problem))
            {
                problems.Add($"{group.exeVersion}: {problem}");
                continue;
            }

            Group = group;

            if (group.exeVersion != version)
                Smithbox.Log(typeof(FxReloader), $"[FX Editor] No FX Reloader offsets for {Link.ExeName} {version}; the {group.exeVersion} offsets resolved.");

            return true;
        }

        error = LOC.Get("FX_Reloader_State_Version", version, string.Join("; ", problems));
        return false;
    }

    /// <summary>Resolves one offset group. The problem is for the log, not localized.</summary>
    private bool ResolveGroup(FxReloaderOffsetEntry g, out string problem)
    {
        SfxGlobal = LoadFxrAddr = 0;
        problem = "";

        var map = Layout.Parse(g);
        var sfxRel = GameLinkOffsets.ParseRelative(g.sfxGlobalRelativeOffset);
        var loadOffset = GameLinkOffsets.ParseHex(g.loadFxrAobOffset);
        var fields = ParsePath(g.managerPath);

        if (map == null || sfxRel == null || loadOffset == null || fields == null ||
            string.IsNullOrWhiteSpace(g.sfxGlobalAob) || string.IsNullOrWhiteSpace(g.loadFxrAob) ||
            string.IsNullOrWhiteSpace(g.loadFxrPrologue))
        {
            problem = "incomplete offsets file";
            return false;
        }

        // The SFX singleton's global
        var sites = Link.Scan(g.sfxGlobalAob);
        if (sites.Count != 1)
        {
            problem = $"SFX global {sites.Count} match(es)";
            return false;
        }

        var site = sites[0];
        var nameRel = GameLinkOffsets.ParseRelative(g.sfxNameRelativeOffset);

        if (nameRel != null && !string.IsNullOrEmpty(g.sfxName))
        {
            var name = Link.ReadBytes(Link.Relative(site, nameRel.Value), g.sfxName.Length + 1);
            if (System.Text.Encoding.ASCII.GetString(name, 0, g.sfxName.Length) != g.sfxName || name[^1] != 0)
            {
                problem = $"the singleton access isn't of {g.sfxName}";
                return false;
            }
        }

        // LoadBinaryFxr
        var loads = Link.Scan(g.loadFxrAob);
        if (loads.Count != 1)
        {
            problem = $"LoadBinaryFxr {loads.Count} match(es)";
            return false;
        }

        var load = loads[0] - loadOffset.Value;
        if (!Link.Matches(load, g.loadFxrPrologue))
        {
            problem = "unexpected LoadBinaryFxr start";
            return false;
        }

        SfxGlobal = Link.Relative(site, sfxRel.Value);
        LoadFxrAddr = load;
        ManagerFields = fields;
        Map = map;

        return true;
    }

    /// <summary>"0x60,0x28,0x160" → its fields; null when empty or malformed</summary>
    private static int[] ParsePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var fields = path.Split(',').Select(p => GameLinkOffsets.ParseHex(p.Trim())).ToList();
        return fields.Any(f => f == null) ? null : fields.Select(f => f.Value).ToArray();
    }

    /// <summary>The effect resource manager, or 0 while the game hasn't made it (title screen, loading)</summary>
    private nint Manager()
    {
        var p = Link.ReadPtr(SfxGlobal);

        foreach (var field in ManagerFields)
        {
            if (p == 0)
                return 0;

            p = Link.ReadPtr(p + field);
        }

        return p;
    }

    #endregion

    #region Poll

    /// <summary>Starts a background refresh of the loaded list, unless one is running</summary>
    public void RequestPoll()
    {
        if (IsPolling)
            return;

        IsPolling = true;

        Task.Run(() =>
        {
            try
            {
                lock (Link.Lock)
                {
                    if (EnsureReady())
                    {
                        var timer = System.Diagnostics.Stopwatch.StartNew();
                        Poll();
                        LastPollMs = (int)timer.ElapsedMilliseconds;
                    }
                }
            }
            catch (Exception e)
            {
                lock (Link.Lock)
                {
                    Link.Fail(LOC.Get("FX_Reloader_State_Read_Failed", e.Message));
                    Forget();
                }
            }
            finally
            {
                LastPoll = DateTime.Now;
                IsPolling = false;
            }
        });
    }

    private record Node(nint Address, int Id, nint Wrapper);

    private void Poll()
    {
        var manager = Manager();
        if (manager == 0)
        {
            SetProblem(LOC.Get("FX_Reloader_State_Not_Loaded"), false);
            Loaded = Array.Empty<LoadedEffect>();
            return;
        }

        var nodes = WalkDefinitions(manager);
        if (nodes == null)
        {
            // The list changes as areas load; the next poll reads it again
            SetProblem(LOC.Get("FX_Reloader_State_List_Changed"), false);
            return;
        }

        var now = DateTime.Now;
        var baseline = !HasBaseline;
        var effects = new List<LoadedEffect>(nodes.Count);
        var seen = new HashSet<int>();

        foreach (var node in nodes)
        {
            if (!seen.Add(node.Id))
                continue;

            var playing = PlayingOf(node.Wrapper);

            if (!Detections.TryGetValue(node.Id, out var detection))
                detection = new Detection(now, DetectReason.Loaded, baseline, playing);
            else if (detection.Playing <= 0 && playing > 0)
                detection = new Detection(now, DetectReason.Started, false, playing);
            else
                detection = detection with { Playing = playing };

            Detections[node.Id] = detection;
            effects.Add(new(node.Id, playing, detection.At, detection.Reason, detection.Initial));
        }

        foreach (var id in Detections.Keys.Where(id => !seen.Contains(id)).ToList())
            Detections.Remove(id);

        HasBaseline = true;

        effects.Sort((a, b) => a.Id.CompareTo(b.Id));
        Loaded = effects;
        Problem = null;

        UpdateWatch(nodes);
    }

    /// <summary>
    /// Every node of the definition list, or null when it doesn't read as a whole list (it changed while being read,
    /// or the layout is wrong)
    /// </summary>
    private List<Node> WalkDefinitions(nint manager)
    {
        try
        {
            var head = Link.ReadPtr(manager + Map.Head);
            var count = BitConverter.ToInt64(Link.ReadBytes(manager + Map.Count, 8), 0);

            if (head == 0)
                return null;

            // The count only bounds the walk; a list whose count field is elsewhere still walks, to a cap
            if (count < 0 || count > 1_000_000)
                count = 200_000;

            var nodes = new List<Node>((int)count);
            var node = Link.ReadPtr(head);
            var size = Map.NodeSize;

            while (node != head)
            {
                if (node == 0 || nodes.Count > count + 64)
                    return null;

                var bytes = Link.ReadBytes(node, size);
                nodes.Add(new(node, BitConverter.ToInt32(bytes, Map.NodeId), (nint)BitConverter.ToInt64(bytes, Map.NodeWrapper)));
                node = (nint)BitConverter.ToInt64(bytes, 0);
            }

            return nodes;
        }
        catch (InvalidOperationException)
        {
            // A node freed while walking
            return null;
        }
    }

    /// <summary>How many instances use a wrapper now; -1 when it can't be read</summary>
    private int PlayingOf(nint wrapper)
    {
        if (wrapper == 0)
            return -1;

        try
        {
            return Link.ReadI32(wrapper + Map.WrapperUses);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
    }

    /// <param name="nodes">the definition list just walked, or null to walk it here</param>
    private void UpdateWatch(List<Node> nodes = null)
    {
        var watch = Watch;
        if (watch == null)
            return;

        var playing = PlayingOf(watch.Wrapper);

        // Whether the effect's entry still holds the wrapper, and the wrapper the sent data
        var state = watch.State;
        nodes ??= Manager() is var manager && manager != 0 ? WalkDefinitions(manager) : null;

        if (nodes != null)
        {
            var node = nodes.FirstOrDefault(n => n.Id == watch.Id);
            state = node == null ? WatchState.Unloaded
                : node.Wrapper == watch.Wrapper && DataOf(watch.Wrapper) == watch.Data ? WatchState.InUse
                : WatchState.Replaced;
        }

        if (playing == watch.Playing && state == watch.State)
            return;

        // The game loading the effect again over a send is worth a line in the log; count changes aren't
        if (state != watch.State && state is not WatchState.InUse)
            Smithbox.Log(typeof(FxReloader), $"[FX Editor] FX Reloader: f{watch.Id:D9} is {state} since the send.");

        Watch = watch with { Playing = playing, State = state };
    }

    /// <summary>A wrapper's FXR data pointer, or 0 when it can't be read</summary>
    private nint DataOf(nint wrapper)
    {
        try
        {
            return wrapper == 0 ? 0 : Link.ReadPtr(wrapper + Map.WrapperData);
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    #endregion

    #region Reload

    /// <summary>
    /// Replaces effects in the running game with these FXR bytes, then does the "after" action once, on a background
    /// task. Each effect goes into the history.
    /// </summary>
    public void RequestReloadMany(IReadOnlyList<SendItem> items, AfterReload after, SendSource source)
    {
        if (items.Count == 0)
            return;

        if (IsReloading)
        {
            Smithbox.LogError(typeof(FxReloader), LOC.Get("FX_Reloader_Reload_Busy", items.Count));
            return;
        }

        IsReloading = true;
        var operation = Link.BeginOperation();
        var label = items.Count == 1 ? $"f{items[0].Id:D9}" : LOC.Get("FX_Reloader_Reload_Many_Name", items.Count);

        Task.Run(() =>
        {
            try
            {
                var results = new List<ReloadResult>();

                lock (Link.Lock)
                {
                    if (!EnsureReady())
                    {
                        LastReload = new(items[0].Id, ReloadOutcome.Failed, StateMessage, DateTime.Now);
                        return;
                    }

                    foreach (var item in items)
                    {
                        var result = Reload(item.Id, item.Bytes);
                        results.Add(result);
                        AddHistory(item.Id, result.Outcome, source);
                    }

                    UpdateWatch();
                }

                var summary = results.Count == 1 ? results[0] : Summarise(label, results);

                // Weapon and SpEffect effects that are playing keep the old data; restarting them shows the reload.
                // Pointless before a world reload, which restarts everything.
                var reloadedIds = results.Where(r => r.Outcome is ReloadOutcome.Reloaded).Select(r => r.Id).ToList();

                if (reloadedIds.Count > 0 && CFG.Current.FxEditor.FxEditor_InGame_RestartAfterReload && Restarter.IsAvailable &&
                    !(after is AfterReload.ReloadWorld && Link.CanReloadWorld))
                {
                    FxRestart.Result restart;
                    lock (Link.Lock)
                        restart = Restarter.Restart(reloadedIds);

                    summary = summary with { Message = summary.Message + " " + DescribeRestart(restart) };
                }

                LastReload = summary;

                var reloaded = results.Any(r => r.Outcome is ReloadOutcome.Reloaded);

                if (reloaded && after is AfterReload.ReloadWorld && Link.CanReloadWorld)
                    Link.ReloadWorld();
                else if (reloaded && after is AfterReload.QuitOut && Link.CanQuitOut)
                    Link.QuitOut();
            }
            catch (Exception e)
            {
                LastReload = new(items[0].Id, ReloadOutcome.Failed, e.Message, DateTime.Now);
                Smithbox.LogError(typeof(FxReloader), $"[FX Editor] In-game reload of {label} failed.", e);
            }
            finally
            {
                operation.Dispose();
                IsReloading = false;
            }
        });
    }

    /// <summary>A restart's result, for the reload message</summary>
    private static string DescribeRestart(FxRestart.Result restart) =>
        restart.Problem != null ? LOC.Get("FX_Reloader_Restart_Failed", restart.Problem)
        : restart.Total == 0 ? LOC.Get("FX_Reloader_Restart_None")
        : LOC.Get("FX_Reloader_Restart_Done", restart.WeaponRows, restart.SpEffectRows);

    /// <summary>
    /// Restarts the effect where weapons and SpEffects play it, without sending it, on a background task
    /// </summary>
    public void RequestRestart(int id)
    {
        if (IsReloading)
            return;

        IsReloading = true;
        var operation = Link.BeginOperation();

        Task.Run(() =>
        {
            try
            {
                lock (Link.Lock)
                {
                    if (!EnsureReady())
                    {
                        LastReload = new(id, ReloadOutcome.Failed, StateMessage, DateTime.Now);
                        return;
                    }

                    var restart = Restarter.Restart(new[] { id });
                    LastReload = new(id, restart.Problem == null ? ReloadOutcome.Reloaded : ReloadOutcome.Failed,
                        $"f{id:D9}: {DescribeRestart(restart)}", DateTime.Now);
                }
            }
            catch (Exception e)
            {
                LastReload = new(id, ReloadOutcome.Failed, e.Message, DateTime.Now);
                Smithbox.LogError(typeof(FxReloader), $"[FX Editor] Restart of f{id:D9} failed.", e);
            }
            finally
            {
                operation.Dispose();
                IsReloading = false;
            }
        });
    }

    private static ReloadResult Summarise(string label, List<ReloadResult> results)
    {
        var done = results.Count(r => r.Outcome is ReloadOutcome.Reloaded);
        var missing = results.Where(r => r.Outcome is ReloadOutcome.NotLoaded).Select(r => $"f{r.Id:D9}").ToList();
        var failed = results.Count(r => r.Outcome is ReloadOutcome.Failed);

        var message = LOC.Get("FX_Reloader_Reload_Many_Done", done, results.Count);
        if (missing.Count > 0)
            message += " " + LOC.Get("FX_Reloader_Reload_Many_Not_Loaded", string.Join(", ", missing.Take(5)) + (missing.Count > 5 ? ", ..." : ""));
        if (failed > 0)
            message += " " + LOC.Get("FX_Reloader_Reload_Many_Failed", failed);

        var outcome = done == results.Count ? ReloadOutcome.Reloaded : ReloadOutcome.Failed;
        return new ReloadResult(results[0].Id, outcome, message, DateTime.Now);
    }

    /// <summary>
    /// Whether bytes are an FXR the game's loader takes for this id: "FXR\0", u16 version, i32 1 at +8, the id at +C
    /// </summary>
    private bool IsLoadableFxr(int id, byte[] bytes) =>
        bytes != null && bytes.Length >= 0x10 &&
        bytes[0] == 'F' && bytes[1] == 'X' && bytes[2] == 'R' && bytes[3] == 0 &&
        BitConverter.ToUInt16(bytes, 6) == Group.fxrVersion &&
        bytes[8] == 1 &&
        BitConverter.ToInt32(bytes, 0xC) == id;

    private ReloadResult Reload(int id, byte[] fxr)
    {
        var name = $"f{id:D9}";

        if (!IsLoadableFxr(id, fxr))
            return new(id, ReloadOutcome.Failed, LOC.Get("FX_Reloader_Reload_Bad_File", name, Group.fxrVersion), DateTime.Now);

        var manager = Manager();
        if (manager == 0)
            return new(id, ReloadOutcome.Failed, LOC.Get("FX_Reloader_State_Not_Loaded"), DateTime.Now);

        var node = WalkDefinitions(manager)?.FirstOrDefault(n => n.Id == id);
        if (node == null)
            return new(id, ReloadOutcome.NotLoaded, LOC.Get("FX_Reloader_Reload_Not_Loaded", name), DateTime.Now);

        var oldData = DataOf(node.Wrapper);
        if (oldData == 0)
            return new(id, ReloadOutcome.Failed, LOC.Get("FX_Reloader_Reload_Changed", name), DateTime.Now);

        var playing = PlayingOf(node.Wrapper);

        var dataBuffer = Link.Alloc(fxr.Length);
        var resultBuffer = Link.Alloc(8);
        var codeBuffer = Link.Alloc(0x100, executable: true);

        // Freed only once the game's thread is done with them; a thread still running would crash on freed memory
        var finished = true;

        try
        {
            if (dataBuffer == 0 || resultBuffer == 0 || codeBuffer == 0)
                return new(id, ReloadOutcome.Failed, LOC.Get("FX_Reloader_Reload_Alloc_Failed"), DateTime.Now);

            Link.WriteBytes(dataBuffer, fxr);
            Link.WriteBytes(resultBuffer, new byte[8]);
            Link.WriteBytes(codeBuffer, BuildReloadCode(LoadFxrAddr, dataBuffer, fxr.Length, resultBuffer,
                node.Address, Map.NodeId, id, Map.NodeWrapper, node.Wrapper, Map.WrapperData, oldData));

            var outcome = Link.RunThread(codeBuffer, 10000, out var exitCode, out var error);

            if (outcome is GameLink.ThreadOutcome.NotStarted)
                return new(id, ReloadOutcome.Failed, LOC.Get("FX_Reloader_Reload_Thread_Failed", error), DateTime.Now);

            if (outcome is GameLink.ThreadOutcome.TimedOut)
            {
                finished = false;
                return new(id, ReloadOutcome.Failed, LOC.Get("FX_Reloader_Reload_Timeout"), DateTime.Now);
            }

            // The loader's own wrapper; only its data pointer is used
            var loaded = Link.ReadPtr(resultBuffer);
            var newData = DataOf(loaded);

            switch (exitCode)
            {
                case 1:
                    Watch = new SendWatch(id, node.Wrapper, newData, playing, DateTime.Now);
                    Smithbox.Log(typeof(FxReloader),
                        $"[FX Editor] Reloaded {name} in game ({fxr.Length} bytes): wrapper 0x{node.Wrapper:X} ({playing} playing) data 0x{oldData:X} -> 0x{newData:X}.");
                    return new(id, ReloadOutcome.Reloaded, playing > 0
                        ? LOC.Get("FX_Reloader_Reload_Done_Playing", name, playing)
                        : LOC.Get("FX_Reloader_Reload_Done", name), DateTime.Now);

                case 2:
                    // The node or wrapper changed between the walk and the swap; the loaded copy is left unused
                    return new(id, ReloadOutcome.Failed, LOC.Get("FX_Reloader_Reload_Changed", name), DateTime.Now);

                default:
                    return new(id, ReloadOutcome.Failed, LOC.Get("FX_Reloader_Reload_Load_Failed", name), DateTime.Now);
            }
        }
        finally
        {
            if (finished)
            {
                Link.Free(dataBuffer);
                Link.Free(resultBuffer);
                Link.Free(codeBuffer);
            }
        }
    }

    /// <summary>
    /// The thread body (keep in step with Claude/Tools/FxReloadProbe "code"):
    ///   loaded = LoadBinaryFxr(data, size); [result] = loaded; if !loaded return 0
    ///   newData = [loaded+dataOffset]
    ///   if [node+idOffset] != id or [node+wrapperOffset] != wrapper return 2
    ///   lock cmpxchg [wrapper+dataOffset]: oldData → newData, else return 2
    ///   return 1
    /// LoadBinaryFxr copies the bytes into the effect heap, so the buffers can be freed once the thread returns. The
    /// loader's own wrapper is left unused, and the old data isn't freed: instances that are playing keep pointers into it.
    /// </summary>
    public static byte[] BuildReloadCode(nint loadFxr, nint data, int size, nint result,
        nint node, int idOffset, int id, int wrapperOffset, nint wrapper, int dataOffset, nint oldData)
    {
        var code = new List<byte>();
        var labels = new Dictionary<string, int>();
        var jumps = new List<(int At, string Label)>();

        void Bytes(params byte[] b) => code.AddRange(b);
        void Imm64(long value) => code.AddRange(BitConverter.GetBytes(value));
        void Imm32(int value) => code.AddRange(BitConverter.GetBytes(value));
        void Jump(byte opcode, string label) { Bytes(opcode, 0); jumps.Add((code.Count - 1, label)); }
        void Label(string label) => labels[label] = code.Count;

        Bytes(0x48, 0x83, 0xEC, 0x28);                          // sub rsp,28h
        Bytes(0x48, 0xB9); Imm64(data);                         // mov rcx,data
        Bytes(0xBA); Imm32(size);                               // mov edx,size
        Bytes(0x48, 0xB8); Imm64(loadFxr);                      // mov rax,LoadBinaryFxr
        Bytes(0xFF, 0xD0);                                      // call rax
        Bytes(0x48, 0xB9); Imm64(result);                       // mov rcx,result
        Bytes(0x48, 0x89, 0x01);                                // mov [rcx],rax
        Bytes(0x48, 0x85, 0xC0);                                // test rax,rax
        Jump(0x74, "fail");                                     // je fail
        Bytes(0x48, 0x8B, 0x90); Imm32(dataOffset);             // mov rdx,[rax+dataOffset]  (new data)
        Bytes(0x48, 0xB9); Imm64(node);                         // mov rcx,node
        Bytes(0x81, 0xB9); Imm32(idOffset); Imm32(id);          // cmp dword [rcx+idOffset],id
        Jump(0x75, "changed");                                  // jne changed
        Bytes(0x48, 0xB8); Imm64(wrapper);                      // mov rax,wrapper
        Bytes(0x48, 0x39, 0x81); Imm32(wrapperOffset);          // cmp [rcx+wrapperOffset],rax
        Jump(0x75, "changed");                                  // jne changed
        Bytes(0x48, 0x8B, 0xC8);                                // mov rcx,rax
        Bytes(0x48, 0xB8); Imm64(oldData);                      // mov rax,oldData
        Bytes(0xF0, 0x48, 0x0F, 0xB1, 0x91); Imm32(dataOffset); // lock cmpxchg [rcx+dataOffset],rdx
        Jump(0x75, "changed");                                  // jne changed
        Bytes(0xB8, 0x01, 0x00, 0x00, 0x00);                    // mov eax,1
        Jump(0xEB, "done");                                     // jmp done
        Label("changed");
        Bytes(0xB8, 0x02, 0x00, 0x00, 0x00);                    // mov eax,2
        Jump(0xEB, "done");                                     // jmp done
        Label("fail");
        Bytes(0x31, 0xC0);                                      // xor eax,eax
        Label("done");
        Bytes(0x48, 0x83, 0xC4, 0x28);                          // add rsp,28h
        Bytes(0xC3);                                            // ret

        foreach (var (at, label) in jumps)
        {
            var rel = labels[label] - (at + 1);
            if (rel is < sbyte.MinValue or > sbyte.MaxValue)
                throw new InvalidOperationException($"jump to {label} out of range");
            code[at] = (byte)(sbyte)rel;
        }

        return code.ToArray();
    }

    #endregion

    #region History

    /// <summary>One effect sent this session: the latest send of each id is kept, newest first</summary>
    public record HistoryEntry(int Id, DateTime At, ReloadOutcome Outcome, SendSource Source);

    private const int HistoryLimit = 50;
    private readonly object HistoryLock = new();
    private List<HistoryEntry> HistoryList = new();

    public volatile IReadOnlyList<HistoryEntry> History = Array.Empty<HistoryEntry>();

    private void AddHistory(int id, ReloadOutcome outcome, SendSource source)
    {
        lock (HistoryLock)
        {
            var list = HistoryList.Where(h => h.Id != id).ToList();
            list.Insert(0, new HistoryEntry(id, DateTime.Now, outcome, source));

            if (list.Count > HistoryLimit)
                list.RemoveRange(HistoryLimit, list.Count - HistoryLimit);

            HistoryList = list;
            History = list;
        }
    }

    public void ClearHistory()
    {
        lock (HistoryLock)
        {
            HistoryList = new();
            History = HistoryList;
        }
    }

    #endregion
}
