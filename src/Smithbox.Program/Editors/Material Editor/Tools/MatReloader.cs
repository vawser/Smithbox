using StudioCore.Application;
using StudioCore.Memory;
using System.Text;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The Material Reloader's work in a running MATBIN game (ER, NR, AC6), over the project's shared GameLink: lists the
/// materials loaded models use, and reloads materials from bytes. See Claude/Documentation/Materials and Textures.md (Material Reloader).
/// The AOBs and offsets come from Assets/Material Reloader/{game}/*.json (MaterialReloaderOffsets); the connection,
/// quit out and reload world are GameLink's.
///
/// How the game holds materials (offsets as in ER, NR and AC6; found by AOB at runtime):
///   MtdRepository (singleton) +78 = hash map of MtdResCap, keyed by file name up to the first '.'
///     map +1C bucket count (u32), +20 bucket array; node +18 name (MSVC wstring, +28 length, +30 capacity),
///     +50 next in bucket, +78 GXMaterialData* (refcount u32 at +8; the ResCap holds one, every model material another)
///   Find(repo, wchar* name)      returns the ResCap
///   Init(rc, bytes, size)        parses a MATBIN into a new GXMaterialData and swaps it in, releasing the old one
/// </summary>
public class MatReloader
{
    private readonly ProjectEntry Project;

    public MatReloader(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>The project's connection to the game, shared with the other live tools</summary>
    public GameLink Link => Project.Handler.GameLink;

    // The Material Editor's copy; read here if the Material Editor's data isn't loaded
    private MaterialReloaderOffsets OwnOffsets;
    private MaterialReloaderOffsets Offsets =>
        Project.Handler.MaterialData?.ReloaderOffsets ?? (OwnOffsets ??= MaterialReloaderOffsets.Load(Project));

    /// <summary>Whether this game has both a game link and Material Reloader offsets</summary>
    public bool IsSupported => Link.IsSupported && Offsets.Groups.Count > 0;

    /// <param name="FirstSeen">when it was first seen in use, while it has stayed in use</param>
    /// <param name="Initial">in use at the first poll after connecting, so not "recently loaded"</param>
    public record InGameMaterial(string Name, int Users, DateTime FirstSeen, bool Initial);

    /// <param name="Name">the model's file name up to the first '.' (c2010, AEG099_001, m60_44_36_00_600000)</param>
    public record InGameModel(string Name, DateTime FirstSeen, bool Initial);

    public enum ReloadOutcome { Reloaded, NotFound, Failed }

    public record ReloadResult(string Name, ReloadOutcome Outcome, string Message, DateTime At);

    public enum AfterReload { Nothing, ReloadWorld, QuitOut }

    /// <summary>A material to send: the game's name for it and its MATBIN bytes</summary>
    public record SendItem(string Name, byte[] Bytes);

    /// <summary>Where a send came from, shown in the history</summary>
    public enum SendSource { Selected, Modified, Save, History, Vanilla }

    // Updated by the background task, read by the UI
    public volatile IReadOnlyList<InGameMaterial> InUse = Array.Empty<InGameMaterial>();
    public volatile IReadOnlyList<InGameModel> Models = Array.Empty<InGameModel>();
    public volatile int TotalMaterials;
    public volatile bool IsPolling;
    public volatile bool IsReloading;
    public volatile ReloadResult LastReload;
    public DateTime? LastPoll { get; private set; }

    // Why the materials can't be read while the link is attached (not found in this version, not loaded yet), or null
    private volatile string Problem;
    private volatile bool ProblemIsFailure;

    /// <summary>
    /// The state to show: the link's, or the reloader's own when the link is attached but the materials can't be read
    /// </summary>
    public GameLink.LinkState State =>
        Link.State is GameLink.LinkState.Attached && Problem != null && ProblemIsFailure ? GameLink.LinkState.Failed : Link.State;

    public string StateMessage =>
        Link.State is GameLink.LinkState.Attached && Problem != null ? Problem : Link.StateMessage;

    /// <summary>Whether the link is attached and the material functions were found</summary>
    public bool IsReady => Link.State is GameLink.LinkState.Attached && ResolvedFor == Link.AttachId && Group != null;

    /// <summary>Whether the loaded model list was found; without it, only materials are listed</summary>
    public bool CanListModels => FlverRepoPtrAddr != 0;

    // Found for one attach of the link
    private int ResolvedFor = -1;
    private MaterialReloaderOffsetEntry Group;
    private MapLayout Map;
    private nint RepoPtrAddr, FindAddr, InitAddr;
    private readonly Dictionary<(nint Node, long Pointer, long Length), string> NameCache = new();

    // Loaded models: FlverRepositoryImp is the same FD4 resource map as MtdRepository, keyed by the model's file
    // name up to the first '.'. Found from a lookup of a model by name (ER: the player model, c0000).
    private nint FlverRepoPtrAddr;

    // First-seen times for "recently loaded", kept by the poll
    private readonly Dictionary<string, DateTime> MaterialFirstSeen = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTime> ModelFirstSeen = new(StringComparer.OrdinalIgnoreCase);
    private bool HasBaseline;
    private DateTime BaselineTime;

    /// <summary>
    /// The FD4 resource map layout, parsed from the offset group: repository + Map is the map; node fields after it
    /// </summary>
    private record MapLayout(int Map, int BucketCount, int Buckets, int Name, int NameLength, int NameCapacity,
        int Next, int Data, int RefCount)
    {
        /// <summary>How much of a node to read: enough for every field</summary>
        public int NodeSize => new[] { Name + 16, NameLength + 8, NameCapacity + 8, Next + 8, Data + 8 }.Max();

        public static MapLayout Parse(MaterialReloaderOffsetEntry g)
        {
            int? H(string s) => GameLinkOffsets.ParseHex(s);

            var values = new[]
            {
                H(g.mapOffset), H(g.mapBucketCountOffset), H(g.mapBucketsOffset), H(g.nodeNameOffset),
                H(g.nodeNameLengthOffset), H(g.nodeNameCapacityOffset), H(g.nodeNextOffset), H(g.nodeDataOffset),
                H(g.dataRefCountOffset)
            };

            if (values.Any(v => v == null))
                return null;

            var v = values.Select(x => x.Value).ToArray();
            return new MapLayout(v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7], v[8]);
        }
    }

    /// <summary>
    /// The name the game files a material under: the file name up to its first '.'
    /// </summary>
    public static string GameNameOf(string key)
    {
        var cut = Math.Max(key.LastIndexOf('\\'), key.LastIndexOf('/'));
        var name = cut >= 0 ? key[(cut + 1)..] : key;
        var dot = name.IndexOf('.');

        return dot >= 0 ? name[..dot] : name;
    }

    /// <summary>
    /// Disconnects the shared link (other live tools too) and clears the lists
    /// </summary>
    public void Detach()
    {
        Link.Detach();

        lock (Link.Lock)
            Forget();
    }

    #region Attach

    /// <summary>
    /// Connects the link and finds the material functions for this attach. Call with the link's lock held.
    /// </summary>
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

        Smithbox.Log(typeof(MatReloader),
            $"[Material Editor] Material Reloader: {Link.ExeName} offsets {Group.exeVersion}, repository ptr 0x{RepoPtrAddr:X}, Find 0x{FindAddr:X}, Init 0x{InitAddr:X}");

        return true;
    }

    private void SetProblem(string message, bool failure)
    {
        ProblemIsFailure = failure;
        Problem = message;
    }

    /// <summary>
    /// Forgets what was found for the last attach, and the lists
    /// </summary>
    private void Forget()
    {
        ResolvedFor = -1;
        Group = null;
        Map = null;
        RepoPtrAddr = FindAddr = InitAddr = 0;
        FlverRepoPtrAddr = 0;
        Problem = null;
        NameCache.Clear();
        MaterialFirstSeen.Clear();
        ModelFirstSeen.Clear();
        HasBaseline = false;
        InUse = Array.Empty<InGameMaterial>();
        Models = Array.Empty<InGameModel>();
    }

    /// <summary>
    /// Finds the repository pointer, Find and Init in the running exe's code, with the first offset group that
    /// resolves: the running version's own group first, then the others (AOBs often survive a patch)
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
                Smithbox.Log(typeof(MatReloader), $"[Material Editor] No material offsets for {Link.ExeName} {version}; the {group.exeVersion} offsets resolved.");

            ResolveModels();
            return true;
        }

        error = LOC.Get("MAT_InGameTool_State_Version", version, string.Join("; ", problems));
        return false;
    }

    /// <summary>
    /// Finds the repository, Find and Init with one offset group. The problem is for the log, not localized.
    /// </summary>
    private bool ResolveGroup(MaterialReloaderOffsetEntry g, out string problem)
    {
        RepoPtrAddr = FindAddr = InitAddr = 0;
        problem = "";

        var map = MapLayout.Parse(g);
        var repoRel = GameLinkOffsets.ParseRelative(g.repositoryAobRelativeOffset);
        var findRel = GameLinkOffsets.ParseRelative(g.findAobRelativeOffset);
        var nameRel = GameLinkOffsets.ParseRelative(g.fallbackNameAobRelativeOffset);
        var initOffset = GameLinkOffsets.ParseHex(g.initAobOffset);

        if (map == null || repoRel == null || findRel == null || initOffset == null ||
            string.IsNullOrWhiteSpace(g.repositoryAob) || string.IsNullOrWhiteSpace(g.initAob) || string.IsNullOrWhiteSpace(g.initPrologue))
        {
            problem = "incomplete offsets file";
            return false;
        }

        var lookups = Link.Scan(g.repositoryAob);
        var inits = Link.Scan(g.initAob);

        if (lookups.Count != 1 || inits.Count != 1)
        {
            problem = $"repository {lookups.Count}, Init {inits.Count} match(es)";
            return false;
        }

        var site = lookups[0];

        // The lea must point at the fallback material's name, or this is a different lookup
        if (nameRel != null && !string.IsNullOrEmpty(g.fallbackName) &&
            Link.ReadUnicode(Link.Relative(site, nameRel.Value), (g.fallbackName.Length + 1) * 2) != g.fallbackName)
        {
            problem = $"the lookup isn't of {g.fallbackName}";
            return false;
        }

        var init = inits[0] - initOffset.Value;

        if (!Link.Matches(init, g.initPrologue))
        {
            problem = "unexpected Init start";
            return false;
        }

        RepoPtrAddr = Link.Relative(site, repoRel.Value);
        FindAddr = Link.Relative(site, findRel.Value);
        InitAddr = init;
        Map = map;

        return true;
    }

    /// <summary>
    /// Finds the loaded model repository. Optional: without it, only materials are listed.
    /// </summary>
    private void ResolveModels()
    {
        FlverRepoPtrAddr = 0;

        if (string.IsNullOrWhiteSpace(Group.modelRepositoryAob))
            return;

        var repoRel = GameLinkOffsets.ParseRelative(Group.modelRepositoryAobRelativeOffset);
        var nameRel = GameLinkOffsets.ParseRelative(Group.modelLookupNameAobRelativeOffset);

        if (repoRel == null)
        {
            Smithbox.Log(typeof(MatReloader), "[Material Editor] Loaded models unavailable: incomplete offsets.");
            return;
        }

        var sites = Link.Scan(Group.modelRepositoryAob);
        if (sites.Count != 1)
        {
            Smithbox.Log(typeof(MatReloader), $"[Material Editor] Loaded models unavailable: {sites.Count} match(es).");
            return;
        }

        var site = sites[0];
        var expected = Group.modelLookupName;

        if (nameRel != null && !string.IsNullOrEmpty(expected) &&
            Link.ReadUnicode(Link.Relative(site, nameRel.Value), (expected.Length + 1) * 2) != expected)
        {
            Smithbox.Log(typeof(MatReloader), $"[Material Editor] Loaded models unavailable: the lookup isn't of {expected}.");
            return;
        }

        FlverRepoPtrAddr = Link.Relative(site, repoRel.Value);
        Smithbox.Log(typeof(MatReloader), $"[Material Editor] Loaded models: FlverRepository global 0x{FlverRepoPtrAddr:X}");
    }

    #endregion

    #region Poll

    /// <summary>
    /// Starts a background refresh of the in-use list, unless one is running
    /// </summary>
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
                        Poll();
                }
            }
            catch (Exception e)
            {
                lock (Link.Lock)
                {
                    Link.Fail(LOC.Get("MAT_InGameTool_State_Read_Failed", e.Message));
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

    private record MapNode(string Name, nint Data);

    /// <summary>
    /// Walks the materials (with their users) and the loaded models, and stamps when each first appeared
    /// </summary>
    private void Poll()
    {
        var repo = Link.ReadPtr(RepoPtrAddr);
        if (repo == 0)
        {
            SetProblem(LOC.Get("MAT_InGameTool_State_Not_Loaded"), false);
            InUse = Array.Empty<InGameMaterial>();
            Models = Array.Empty<InGameModel>();
            return;
        }

        var materialNodes = WalkMap(repo + Map.Map, out var bucketCount);
        if (materialNodes == null)
        {
            SetProblem(LOC.Get("MAT_InGameTool_State_Bad_Map", bucketCount), true);
            InUse = Array.Empty<InGameMaterial>();
            Models = Array.Empty<InGameModel>();
            return;
        }

        var now = DateTime.Now;
        var baseline = !HasBaseline;

        // Materials: one reference is the repository's own, each loaded model material holds another
        var materials = new List<InGameMaterial>();
        var seenMaterials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in materialNodes)
        {
            if (node.Data == 0)
                continue;

            var refs = Link.ReadI32(node.Data + Map.RefCount);
            if (refs <= 1)
                continue;

            seenMaterials.Add(node.Name);
            var firstSeen = Stamp(MaterialFirstSeen, node.Name, now);
            materials.Add(new(node.Name, refs - 1, firstSeen, baseline || firstSeen == BaselineTime));
        }

        Forget(MaterialFirstSeen, seenMaterials);

        // Models
        var models = new List<InGameModel>();

        if (FlverRepoPtrAddr != 0)
        {
            var flverRepo = Link.ReadPtr(FlverRepoPtrAddr);
            var modelNodes = flverRepo != 0 ? WalkMap(flverRepo + Map.Map, out _) : null;
            var seenModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var node in modelNodes ?? new List<MapNode>())
            {
                if (!seenModels.Add(node.Name))
                    continue;

                var firstSeen = Stamp(ModelFirstSeen, node.Name, now);
                models.Add(new(node.Name, firstSeen, baseline || firstSeen == BaselineTime));
            }

            Forget(ModelFirstSeen, seenModels);
        }

        if (baseline)
        {
            HasBaseline = true;
            BaselineTime = now;

            // TEMP diagnostics for the first in-app check of the model list: count and name format (2026-11-14)
            Smithbox.Log(typeof(MatReloader),
                $"[Material Editor] Loaded models: {models.Count}; sample: {string.Join(", ", models.Where((_, i) => i % Math.Max(1, models.Count / 12) == 0).Take(12).Select(m => m.Name))}");
        }

        materials.Sort((a, c) => string.Compare(a.Name, c.Name, StringComparison.OrdinalIgnoreCase));
        models.Sort((a, c) => string.Compare(a.Name, c.Name, StringComparison.OrdinalIgnoreCase));

        TotalMaterials = materialNodes.Count;
        InUse = materials;
        Models = models;
        Problem = null;
    }

    /// <summary>
    /// When a name first appeared: kept while it stays loaded, so a model that unloads and loads again counts as new.
    /// Names present at the first poll share the baseline time and aren't shown as new.
    /// </summary>
    private static DateTime Stamp(Dictionary<string, DateTime> firstSeen, string name, DateTime now)
    {
        if (firstSeen.TryGetValue(name, out var time))
            return time;

        firstSeen[name] = now;
        return now;
    }

    private static void Forget(Dictionary<string, DateTime> firstSeen, HashSet<string> seen)
    {
        foreach (var name in firstSeen.Keys.Where(n => !seen.Contains(n)).ToList())
            firstSeen.Remove(name);
    }

    /// <summary>
    /// Every node of an FD4 resource map (layout from the offsets: ER +1C bucket count, +20 bucket array;
    /// node +18 name, +50 next, +78 data). Null when the map doesn't look valid.
    /// </summary>
    private List<MapNode> WalkMap(nint map, out int bucketCount)
    {
        bucketCount = Link.ReadI32(map + Map.BucketCount);
        var buckets = Link.ReadPtr(map + Map.Buckets);
        var nodeSize = Map.NodeSize;

        if (bucketCount <= 0 || bucketCount > 1 << 20 || buckets == 0)
            return null;

        var bucketBytes = Link.ReadBytes(buckets, bucketCount * 8);
        var nodes = new List<MapNode>();

        for (var b = 0; b < bucketCount; b++)
        {
            var node = (nint)BitConverter.ToInt64(bucketBytes, b * 8);

            for (var chain = 0; node != 0 && chain < 4096; chain++)
            {
                var nodeBytes = Link.ReadBytes(node, nodeSize);
                var name = NameOf(node, nodeBytes);

                if (name != null)
                    nodes.Add(new(name, (nint)BitConverter.ToInt64(nodeBytes, Map.Data)));

                node = (nint)BitConverter.ToInt64(nodeBytes, Map.Next);
            }
        }

        return nodes;
    }

    private string NameOf(nint node, byte[] nodeBytes)
    {
        // MSVC wstring: inline buffer when capacity < 8, else a pointer
        var length = BitConverter.ToInt64(nodeBytes, Map.NameLength);
        var capacity = BitConverter.ToInt64(nodeBytes, Map.NameCapacity);
        var pointer = BitConverter.ToInt64(nodeBytes, Map.Name);

        if (length <= 0 || length > 260)
            return null;

        // Model nodes are freed and reused as models load and unload, so the key includes the name's own fields
        var key = (node, pointer, length);
        if (NameCache.TryGetValue(key, out var cached))
            return cached;

        var name = capacity < 8
            ? Encoding.Unicode.GetString(nodeBytes, Map.Name, (int)length * 2)
            : Encoding.Unicode.GetString(Link.ReadBytes((nint)pointer, (int)length * 2));

        if (NameCache.Count > 200000)
            NameCache.Clear();

        NameCache[key] = name;
        return name;
    }

    #endregion

    #region Reload

    /// <summary>
    /// Replaces a material in the running game with these MATBIN bytes, on a background task
    /// </summary>
    public void RequestReload(string gameName, byte[] matbin, AfterReload after, SendSource source = SendSource.Selected) =>
        RequestReloadMany(new[] { new SendItem(gameName, matbin) }, after, source);

    /// <summary>
    /// Replaces several materials in the running game, then does the "after" action once, on a background task.
    /// Each material goes into the history.
    /// </summary>
    public void RequestReloadMany(IReadOnlyList<SendItem> items, AfterReload after, SendSource source)
    {
        if (items.Count == 0)
            return;

        if (IsReloading)
        {
            Smithbox.LogError(typeof(MatReloader), LOC.Get("MAT_InGameTool_Reload_Busy", items.Count));
            return;
        }

        IsReloading = true;
        var operation = Link.BeginOperation();
        var label = items.Count == 1 ? items[0].Name : LOC.Get("MAT_InGameTool_Reload_Many_Name", items.Count);

        Task.Run(() =>
        {
            try
            {
                var results = new List<ReloadResult>();

                lock (Link.Lock)
                {
                    if (!EnsureReady())
                    {
                        LastReload = new(label, ReloadOutcome.Failed, StateMessage, DateTime.Now);
                        return;
                    }

                    foreach (var item in items)
                    {
                        var result = Reload(item.Name, item.Bytes);
                        results.Add(result);
                        AddHistory(item.Name, result.Outcome, source);
                    }
                }

                LastReload = results.Count == 1 ? results[0] : Summarise(label, results);

                var reloaded = results.Any(r => r.Outcome is ReloadOutcome.Reloaded);

                // The "after" setting is shared by every game; one this game doesn't have is skipped quietly
                if (reloaded && after is AfterReload.ReloadWorld && Link.CanReloadWorld)
                    Link.ReloadWorld();
                else if (reloaded && after is AfterReload.QuitOut && Link.CanQuitOut)
                    Link.QuitOut();
            }
            catch (Exception e)
            {
                LastReload = new(label, ReloadOutcome.Failed, e.Message, DateTime.Now);
                Smithbox.LogError(typeof(MatReloader), $"[Material Editor] In-game reload of {label} failed.", e);
            }
            finally
            {
                operation.Dispose();
                IsReloading = false;
            }
        });

        // The list's counts change once models reload
        RequestPoll();
    }

    private static ReloadResult Summarise(string label, List<ReloadResult> results)
    {
        var done = results.Count(r => r.Outcome is ReloadOutcome.Reloaded);
        var missing = results.Where(r => r.Outcome is ReloadOutcome.NotFound).Select(r => r.Name).ToList();
        var failed = results.Count(r => r.Outcome is ReloadOutcome.Failed);

        var message = LOC.Get("MAT_InGameTool_Reload_Many_Done", done, results.Count);
        if (missing.Count > 0)
            message += " " + LOC.Get("MAT_InGameTool_Reload_Many_Not_Found", string.Join(", ", missing.Take(5)) + (missing.Count > 5 ? ", ..." : ""));
        if (failed > 0)
            message += " " + LOC.Get("MAT_InGameTool_Reload_Many_Failed", failed);

        // Shown as a success when everything went; a partial send is shown as a problem so it gets read
        var outcome = done == results.Count ? ReloadOutcome.Reloaded : ReloadOutcome.Failed;
        return new ReloadResult(label, outcome, message, DateTime.Now);
    }

    private ReloadResult Reload(string gameName, byte[] matbin)
    {
        var nameBytes = Encoding.Unicode.GetBytes(gameName + "\0");

        var dataBuffer = Link.Alloc(matbin.Length);
        var nameBuffer = Link.Alloc(nameBytes.Length);
        var codeBuffer = Link.Alloc(0x100, executable: true);

        // Freed only once the game's thread is done with them; a thread still running would crash on freed memory
        var finished = true;

        try
        {
            if (dataBuffer == 0 || nameBuffer == 0 || codeBuffer == 0)
                return new(gameName, ReloadOutcome.Failed, LOC.Get("MAT_InGameTool_Reload_Alloc_Failed"), DateTime.Now);

            Link.WriteBytes(dataBuffer, matbin);
            Link.WriteBytes(nameBuffer, nameBytes);
            Link.WriteBytes(codeBuffer, BuildReloadCode(nameBuffer, dataBuffer, matbin.Length));

            var outcome = Link.RunThread(codeBuffer, 10000, out var exitCode, out var error);

            if (outcome is GameLink.ThreadOutcome.NotStarted)
                return new(gameName, ReloadOutcome.Failed, LOC.Get("MAT_InGameTool_Reload_Thread_Failed", error), DateTime.Now);

            if (outcome is GameLink.ThreadOutcome.TimedOut)
            {
                finished = false;
                return new(gameName, ReloadOutcome.Failed, LOC.Get("MAT_InGameTool_Reload_Timeout"), DateTime.Now);
            }

            if (exitCode == 0)
                return new(gameName, ReloadOutcome.NotFound, LOC.Get("MAT_InGameTool_Reload_Not_Found", gameName), DateTime.Now);

            Smithbox.Log(typeof(MatReloader), $"[Material Editor] Reloaded {gameName} in game ({matbin.Length} bytes).");
            return new(gameName, ReloadOutcome.Reloaded, LOC.Get("MAT_InGameTool_Reload_Done", gameName), DateTime.Now);
        }
        finally
        {
            if (finished)
            {
                Link.Free(dataBuffer);
                Link.Free(nameBuffer);
                Link.Free(codeBuffer);
            }
        }
    }

    /// <summary>
    /// The thread body: rc = Find([repo], name); if rc, Init(rc, data, size). Returns 1 when found, 0 when not.
    /// Init parses into a new GXMaterialData before releasing the old one, and copies everything it needs,
    /// so the buffers can be freed once the thread returns.
    /// </summary>
    private byte[] BuildReloadCode(nint name, nint data, int size)
    {
        var code = new List<byte>();

        void Bytes(params byte[] b) => code.AddRange(b);
        void Imm(long value) => code.AddRange(BitConverter.GetBytes(value));

        Bytes(0x48, 0x83, 0xEC, 0x28);                     // sub rsp,28h
        Bytes(0x48, 0xB8); Imm(RepoPtrAddr);               // mov rax,repoPtrAddr
        Bytes(0x48, 0x8B, 0x08);                           // mov rcx,[rax]
        Bytes(0x48, 0x85, 0xC9);                           // test rcx,rcx
        Bytes(0x74, 0x45);                                 // je fail (+0x45)
        Bytes(0x48, 0xBA); Imm(name);                      // mov rdx,name
        Bytes(0x48, 0xB8); Imm(FindAddr);                  // mov rax,Find
        Bytes(0xFF, 0xD0);                                 // call rax
        Bytes(0x48, 0x85, 0xC0);                           // test rax,rax
        Bytes(0x74, 0x2A);                                 // je fail (+0x2A)
        Bytes(0x48, 0x8B, 0xC8);                           // mov rcx,rax
        Bytes(0x48, 0xBA); Imm(data);                      // mov rdx,data
        Bytes(0x49, 0xB8); Imm(size);                      // mov r8,size
        Bytes(0x48, 0xB8); Imm(InitAddr);                  // mov rax,Init
        Bytes(0xFF, 0xD0);                                 // call rax
        Bytes(0xB8, 0x01, 0x00, 0x00, 0x00);               // mov eax,1
        Bytes(0xEB, 0x02);                                 // jmp done
        Bytes(0x31, 0xC0);                                 // fail: xor eax,eax
        Bytes(0x48, 0x83, 0xC4, 0x28);                     // done: add rsp,28h
        Bytes(0xC3);                                       // ret

        return code.ToArray();
    }

    #endregion

    #region History

    /// <summary>One material sent this session: the latest send of each name is kept, newest first</summary>
    public record HistoryEntry(string Name, DateTime At, ReloadOutcome Outcome, SendSource Source);

    private const int HistoryLimit = 50;
    private readonly object HistoryLock = new();
    private List<HistoryEntry> HistoryList = new();

    public volatile IReadOnlyList<HistoryEntry> History = Array.Empty<HistoryEntry>();

    private void AddHistory(string name, ReloadOutcome outcome, SendSource source)
    {
        lock (HistoryLock)
        {
            var list = HistoryList.Where(h => !string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
            list.Insert(0, new HistoryEntry(name, DateTime.Now, outcome, source));

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
