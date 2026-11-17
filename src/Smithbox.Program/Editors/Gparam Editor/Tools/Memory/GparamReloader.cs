using StudioCore.Application;
using StudioCore.Memory;
using System.Text;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The Gparam Reloader's work in the running game (ER), over the project's shared GameLink: lists the GPARAMs the game
/// has loaded, and replaces one with new bytes. See Claude/Documentation/Gparam Reloader Analysis.md.
/// The AOBs and offsets come from Assets/Gparam Reloader/{game}/*.json (GparamReloaderOffsets).
///
/// How the game holds GPARAMs (ER offsets):
///   CSGparamRepositoryImp (singleton) +90 = FD4 resource map; its nodes are the CSGparamResCaps, keyed by file stem
///   ResCap +88 GX handle (the GX gparam manager's parsed copy), +90 active flag, +94 blend slot,
///          +D0/+100/+130 lists of active IDs, +160 kept copy of the bytes
///   Init(rc, bytes, size) parses into a new handle, but doesn't release a previous one, so a reload first replays the
///   release half of the ResCap destructor (deactivate IDs, release the handle, free the copy), then runs Init, then
///   turns the same IDs back on (GparamReloadCode).
/// </summary>
public class GparamReloader
{
    private readonly ProjectEntry Project;

    public GparamReloader(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>The project's connection to the game, shared with the other live tools</summary>
    public GameLink Link => Project.Handler.GameLink;

    private GparamReloaderOffsets OffsetsCache;
    private GparamReloaderOffsets Offsets => OffsetsCache ??= GparamReloaderOffsets.Load(Project);

    /// <summary>Whether this game has both a game link and Gparam Reloader offsets</summary>
    public bool IsSupported => Link.IsSupported && Offsets.Groups.Count > 0;

    /// <param name="Active">the ResCap's active flag</param>
    /// <param name="ActiveIds">IDs in the ResCap's active-ID lists</param>
    public record InGameGparam(string Name, bool Active, int ActiveIds);

    public enum ReloadOutcome { Reloaded, NotFound, Failed }

    public record ReloadResult(string Name, ReloadOutcome Outcome, string Message, DateTime At);

    // Updated by the background task, read by the UI
    public volatile IReadOnlyList<InGameGparam> Loaded = Array.Empty<InGameGparam>();
    public volatile bool IsPolling;
    public volatile bool IsReloading;
    public volatile ReloadResult LastReload;
    public DateTime? LastPoll { get; private set; }

    // Why the GPARAMs can't be read while the link is attached, or null
    private volatile string Problem;
    private volatile bool ProblemIsFailure;

    public GameLink.LinkState State =>
        Link.State is GameLink.LinkState.Attached && Problem != null && ProblemIsFailure ? GameLink.LinkState.Failed : Link.State;

    public string StateMessage =>
        Link.State is GameLink.LinkState.Attached && Problem != null ? Problem : Link.StateMessage;

    /// <summary>Whether the link is attached and the GPARAM functions were found</summary>
    public bool IsReady => Link.State is GameLink.LinkState.Attached && ResolvedFor == Link.AttachId && Group != null;

    // Found for one attach of the link
    private int ResolvedFor = -1;
    private GparamReloaderOffsetEntry Group;
    private Layout L;
    private GparamReloadCode.Addresses Addr;

    /// <summary>The map and ResCap layout, parsed from the offset group</summary>
    private record Layout(int Map, int BucketCount, int Buckets, int Name, int NameLength, int NameCapacity, int Next,
        int Id, int IdOverride, int Handle, int Active, int Slot, int[] Lists, int ListCount, int ListCapacity)
    {
        public int NodeSize => new[] { Name + 16, NameLength + 8, NameCapacity + 8, Next + 8, Id + 4, IdOverride + 4,
            Handle + 8, Active + 1, Slot + 4 }.Concat(Lists.Select(l => l + ListCount + 8)).Max();

        public static Layout Parse(GparamReloaderOffsetEntry g)
        {
            int? H(string s) => GameLinkOffsets.ParseHex(s);

            var values = new[]
            {
                H(g.mapOffset), H(g.mapBucketCountOffset), H(g.mapBucketsOffset), H(g.nodeNameOffset),
                H(g.nodeNameLengthOffset), H(g.nodeNameCapacityOffset), H(g.nodeNextOffset), H(g.resIdOffset),
                H(g.resIdOverrideOffset), H(g.resHandleOffset), H(g.resActiveOffset), H(g.resSlotOffset),
                H(g.idListCountOffset)
            };

            var lists = (g.resIdListOffsets ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(H).ToList();

            if (values.Any(v => v == null) || lists.Count == 0 || lists.Any(v => v == null) ||
                !int.TryParse(g.idListCapacity, out var capacity))
                return null;

            var v = values.Select(x => x.Value).ToArray();
            return new Layout(v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7], v[8], v[9], v[10], v[11],
                lists.Select(x => x.Value).ToArray(), v[12], capacity);
        }
    }

    /// <summary>The name the game files a GPARAM under: the file name up to its first '.'</summary>
    public static string GameNameOf(string key)
    {
        var cut = Math.Max(key.LastIndexOf('\\'), key.LastIndexOf('/'));
        var name = cut >= 0 ? key[(cut + 1)..] : key;
        var dot = name.IndexOf('.');

        return dot >= 0 ? name[..dot] : name;
    }

    /// <summary>Disconnects the shared link (other live tools too) and clears the list</summary>
    public void Detach()
    {
        Link.Detach();

        lock (Link.Lock)
            Forget();
    }

    #region Attach

    /// <summary>Connects the link and finds the GPARAM functions for this attach. Call with the link's lock held.</summary>
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

        Smithbox.Log(typeof(GparamReloader),
            $"[Gparam Editor] Gparam Reloader: {Link.ExeName} offsets {Group.exeVersion}, repository ptr 0x{Addr.RepositoryPointer:X}, " +
            $"Find 0x{Addr.Find:X}, Init 0x{Addr.Init:X}, Activate 0x{Addr.Activate:X}, Deactivate 0x{Addr.Deactivate:X}, " +
            $"Release 0x{Addr.Release:X}, GetAllocator 0x{Addr.GetAllocator:X}, gx 0x{Addr.GxGlobal:X}");

        return true;
    }

    private void SetProblem(string message, bool failure)
    {
        ProblemIsFailure = failure;
        Problem = message;
    }

    private void Forget()
    {
        ResolvedFor = -1;
        Group = null;
        L = null;
        Addr = null;
        Problem = null;
        Loaded = Array.Empty<InGameGparam>();
    }

    /// <summary>
    /// Finds every address with the first offset group that resolves: the running version's own group first
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
                Smithbox.Log(typeof(GparamReloader), $"[Gparam Editor] No Gparam Reloader offsets for {Link.ExeName} {version}; the {group.exeVersion} offsets resolved.");

            return true;
        }

        error = LOC.Get("GPARAM_Reloader_State_Version", version, string.Join("; ", problems));
        return false;
    }

    /// <summary>Resolves one offset group. The problem is for the log, not localized.</summary>
    private bool ResolveGroup(GparamReloaderOffsetEntry g, out string problem)
    {
        problem = "";

        var layout = Layout.Parse(g);
        (int Start, int End)? R(string s) => GameLinkOffsets.ParseRelative(s);
        int? H(string s) => GameLinkOffsets.ParseHex(s);

        var repoRel = R(g.repositoryAobRelativeOffset);
        var findRel = R(g.findAobRelativeOffset);
        var initRel = R(g.initAobRelativeOffset);
        var gxRel = R(g.gxGlobalAobRelativeOffset);
        var deactivateRel = R(g.deactivateAobRelativeOffset);
        var releaseRel = R(g.releaseAobRelativeOffset);
        var allocRel = R(g.allocatorAobRelativeOffset);
        var activateRel = R(g.activateAobRelativeOffset);
        var allocFree = H(g.allocatorFreeOffset);
        var gxGparam = H(g.gxGparamOffset);
        var gxManager = H(g.gxManagerOffset);
        var paramsOffset = H(g.resActivateParamsOffset);
        var bytesOffset = H(g.resBytesOffset);

        var aobs = new[] { g.repositoryAob, g.createAob, g.releaseAob, g.freeAob, g.activateAob, g.initPrologue };

        if (layout == null || repoRel == null || findRel == null || initRel == null || gxRel == null ||
            deactivateRel == null || releaseRel == null || allocRel == null || activateRel == null || allocFree == null ||
            gxGparam == null || gxManager == null || paramsOffset == null || bytesOffset == null ||
            aobs.Any(string.IsNullOrWhiteSpace))
        {
            problem = "incomplete offsets file";
            return false;
        }

        var repoSites = Link.Scan(g.repositoryAob);
        var createSites = Link.Scan(g.createAob);
        var releaseSites = Link.Scan(g.releaseAob);
        var freeSites = Link.Scan(g.freeAob);
        var activateSites = Link.Scan(g.activateAob);

        if (repoSites.Count != 1 || createSites.Count != 1 || releaseSites.Count != 1 || freeSites.Count != 1 || activateSites.Count != 1)
        {
            problem = $"repository {repoSites.Count}, Create {createSites.Count}, release {releaseSites.Count}, " +
                      $"free {freeSites.Count}, Activate {activateSites.Count} match(es)";
            return false;
        }

        var init = Link.Relative(createSites[0], initRel.Value);

        if (!Link.Matches(init, g.initPrologue))
        {
            problem = "unexpected Init start";
            return false;
        }

        L = layout;
        Addr = new GparamReloadCode.Addresses(
            RepositoryPointer: Link.Relative(repoSites[0], repoRel.Value),
            Find: Link.Relative(repoSites[0], findRel.Value),
            Init: init,
            GxGlobal: Link.Relative(releaseSites[0], gxRel.Value),
            Activate: Link.Relative(activateSites[0], activateRel.Value),
            Deactivate: Link.Relative(releaseSites[0], deactivateRel.Value),
            Release: Link.Relative(releaseSites[0], releaseRel.Value),
            GetAllocator: Link.Relative(freeSites[0], allocRel.Value),
            AllocatorFree: allocFree.Value,
            GxGparamOffset: gxGparam.Value,
            GxManagerOffset: gxManager.Value,
            HandleOffset: layout.Handle,
            ActiveOffset: layout.Active,
            SlotOffset: layout.Slot,
            ActivateParamsOffset: paramsOffset.Value,
            BytesOffset: bytesOffset.Value);

        return true;
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
                        Poll();
                }
            }
            catch (Exception e)
            {
                lock (Link.Lock)
                {
                    Link.Fail(LOC.Get("GPARAM_Reloader_State_Read_Failed", e.Message));
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

    /// <summary>One ResCap as read from the game</summary>
    private record ResCapState(string Name, nint ResCap, long Handle, bool Active, int Slot, int MainId, List<int> ListIds);

    private void Poll()
    {
        var nodes = ReadResCaps(out var problem);

        if (nodes == null)
        {
            SetProblem(problem, problem != LOC.Get("GPARAM_Reloader_State_Not_Loaded"));
            Loaded = Array.Empty<InGameGparam>();
            return;
        }

        Loaded = nodes
            .Select(n => new InGameGparam(n.Name, n.Active, n.ListIds.Count))
            .OrderBy(n => n.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Problem = null;
    }

    /// <summary>Every ResCap in the repository's map; null (with the problem) when it can't be read</summary>
    private List<ResCapState> ReadResCaps(out string problem)
    {
        problem = null;

        var repo = Link.ReadPtr((nint)Addr.RepositoryPointer);
        if (repo == 0)
        {
            problem = LOC.Get("GPARAM_Reloader_State_Not_Loaded");
            return null;
        }

        var map = repo + L.Map;
        var bucketCount = Link.ReadI32(map + L.BucketCount);
        var buckets = Link.ReadPtr(map + L.Buckets);

        if (bucketCount <= 0 || bucketCount > 1 << 20 || buckets == 0)
        {
            problem = LOC.Get("GPARAM_Reloader_State_Bad_Map", bucketCount);
            return null;
        }

        var bucketBytes = Link.ReadBytes(buckets, bucketCount * 8);
        var result = new List<ResCapState>();

        for (var b = 0; b < bucketCount; b++)
        {
            var node = (nint)BitConverter.ToInt64(bucketBytes, b * 8);

            for (var chain = 0; node != 0 && chain < 4096; chain++)
            {
                var bytes = Link.ReadBytes(node, L.NodeSize);
                var state = ReadResCap(node, bytes);

                if (state != null)
                    result.Add(state);

                node = (nint)BitConverter.ToInt64(bytes, L.Next);
            }
        }

        return result;
    }

    private ResCapState ReadResCap(nint node, byte[] bytes)
    {
        // MSVC wstring: inline buffer when capacity < 8, else a pointer
        var length = BitConverter.ToInt64(bytes, L.NameLength);
        var capacity = BitConverter.ToInt64(bytes, L.NameCapacity);

        if (length <= 0 || length > 260)
            return null;

        var name = capacity < 8
            ? Encoding.Unicode.GetString(bytes, L.Name, (int)length * 2)
            : Encoding.Unicode.GetString(Link.ReadBytes((nint)BitConverter.ToInt64(bytes, L.Name), (int)length * 2));

        var ids = new List<int>();

        foreach (var list in L.Lists)
        {
            var count = BitConverter.ToInt64(bytes, list + L.ListCount);
            if (count < 0 || count > L.ListCapacity)
                return null;

            // DLFixedVector<u32>: elements start at the list, rounded up to 4 bytes
            var start = list + (int)(-(node + list) & 3);
            for (var i = 0; i < count; i++)
                ids.Add(BitConverter.ToInt32(bytes, start + i * 4));
        }

        var id = BitConverter.ToInt32(bytes, L.Id);
        var idOverride = BitConverter.ToInt32(bytes, L.IdOverride);

        return new ResCapState(name, node, BitConverter.ToInt64(bytes, L.Handle), bytes[L.Active] != 0,
            BitConverter.ToInt32(bytes, L.Slot), idOverride != -1 ? idOverride : id, ids);
    }

    #endregion

    #region Reload

    /// <summary>Replaces a GPARAM in the running game with these bytes, on a background task</summary>
    public void RequestReload(string gameName, byte[] gparam)
    {
        if (IsReloading)
        {
            Smithbox.LogError(typeof(GparamReloader), LOC.Get("GPARAM_Reloader_Busy", gameName));
            return;
        }

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
                        LastReload = new(gameName, ReloadOutcome.Failed, StateMessage, DateTime.Now);
                        return;
                    }

                    LastReload = Reload(gameName, gparam);
                    Poll();
                }
            }
            catch (Exception e)
            {
                LastReload = new(gameName, ReloadOutcome.Failed, e.Message, DateTime.Now);
                Smithbox.LogError(typeof(GparamReloader), $"[Gparam Editor] In-game reload of {gameName} failed.", e);
            }
            finally
            {
                operation.Dispose();
                IsReloading = false;
            }
        });
    }

    private ReloadResult Reload(string gameName, byte[] gparam)
    {
        // Init parses a plain GPARAM; it would read compressed bytes as one and leave garbage in the graphics settings
        if (gparam.Length < 4 || Encoding.ASCII.GetString(gparam, 0, 4) == "DCX\0")
            return new(gameName, ReloadOutcome.Failed, LOC.Get("GPARAM_Reloader_Compressed", gameName), DateTime.Now);

        var all = ReadResCaps(out var problem);
        if (all == null)
            return new(gameName, ReloadOutcome.Failed, problem, DateTime.Now);

        var matches = all.Where(r => string.Equals(r.Name, gameName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count == 0)
            return new(gameName, ReloadOutcome.NotFound, LOC.Get("GPARAM_Reloader_Not_Found", gameName), DateTime.Now);

        var before = matches[0];

        if (before.Handle == -1)
            return new(gameName, ReloadOutcome.Failed, LOC.Get("GPARAM_Reloader_No_Handle", gameName), DateTime.Now);

        // TEMP diagnostics for the first in-game checks of the reload (2026-11-17)
        Smithbox.Log(typeof(GparamReloader),
            $"[Gparam Editor] Reload {gameName}: before rc 0x{before.ResCap:X} handle 0x{before.Handle:X} active {before.Active} " +
            $"slot {before.Slot} id {before.MainId} list IDs [{string.Join(",", before.ListIds)}], {gparam.Length} bytes");

        var nameBytes = Encoding.Unicode.GetBytes(before.Name + "\0");

        var dataBuffer = Link.Alloc(gparam.Length);
        var nameBuffer = Link.Alloc(nameBytes.Length);

        var snapshot = new GparamReloadCode.Snapshot(before.ResCap, before.Handle, before.Active, before.Slot, before.MainId, before.ListIds);
        var code = GparamReloadCode.Build(Addr, snapshot, nameBuffer, dataBuffer, gparam.Length);
        var codeBuffer = Link.Alloc(code.Length, executable: true);

        // Freed only once the game's thread is done with them; a thread still running would crash on freed memory.
        // Init copies the bytes (into the GX manager's entry and the ResCap's kept copy), so nothing points at them after.
        var finished = true;

        try
        {
            if (dataBuffer == 0 || nameBuffer == 0 || codeBuffer == 0)
                return new(gameName, ReloadOutcome.Failed, LOC.Get("GPARAM_Reloader_Alloc_Failed"), DateTime.Now);

            Link.WriteBytes(dataBuffer, gparam);
            Link.WriteBytes(nameBuffer, nameBytes);
            Link.WriteBytes(codeBuffer, code);

            var outcome = Link.RunThread(codeBuffer, 10000, out var exitCode, out var error);

            if (outcome is GameLink.ThreadOutcome.NotStarted)
                return new(gameName, ReloadOutcome.Failed, LOC.Get("GPARAM_Reloader_Thread_Failed", error), DateTime.Now);

            if (outcome is GameLink.ThreadOutcome.TimedOut)
            {
                finished = false;
                return new(gameName, ReloadOutcome.Failed, LOC.Get("GPARAM_Reloader_Timeout"), DateTime.Now);
            }

            // TEMP diagnostics (2026-11-17)
            var after = ReadResCaps(out _)?.FirstOrDefault(r => r.ResCap == before.ResCap);
            Smithbox.Log(typeof(GparamReloader),
                $"[Gparam Editor] Reload {gameName}: exit {exitCode}; after " + (after == null ? "rc gone" :
                $"handle 0x{after.Handle:X} active {after.Active} slot {after.Slot} id {after.MainId} list IDs [{string.Join(",", after.ListIds)}]"));

            return exitCode switch
            {
                1 => new(gameName, ReloadOutcome.Reloaded, LOC.Get("GPARAM_Reloader_Done", gameName), DateTime.Now),
                2 => new(gameName, ReloadOutcome.Failed, LOC.Get("GPARAM_Reloader_Parse_Failed", gameName), DateTime.Now),
                _ => new(gameName, ReloadOutcome.Failed, LOC.Get("GPARAM_Reloader_Changed", gameName), DateTime.Now),
            };
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

    #endregion
}
