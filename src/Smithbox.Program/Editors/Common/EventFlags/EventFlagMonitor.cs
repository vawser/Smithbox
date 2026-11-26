using StudioCore.Memory;
using System.Runtime.InteropServices;

namespace StudioCore.Editors.Common;

/// <summary>
/// Watches the running game's event flags (ER, NR, AC6, DS3, SDT). On attach it builds an <see cref="EventFlagLayout"/>
/// from the game's own tables, then on each poll reads every chunk, compares it with the last read and records which
/// flags changed and when. The first read after attaching (or after a save is loaded) is the baseline: nothing counts
/// as changed. Two storage families (Game Link offsets, eventFlagLayout):
/// <list type="bullet">
/// <item>VirtualMemory (ER, NR, AC6): CS::CSFD4VirtualMemoryFlag at the manager's start: a slot buffer and a std::map
/// of block (flag / 1000) → entry; type 1 = slot in the buffer, type 2 = a block stored elsewhere.</item>
/// <item>Sprj (DS3, SDT): group tables (flag / 10^7 % 10) of world-block categories, each with 10 records (flag / 1000
/// % 10) pointing at 1000 bits; category 0 = global, category c = the world block whose info has category c − 1.</item>
/// </list>
/// Writes only on request (<see cref="RequestWrite"/>). See Claude/Documentation/Event Flag Viewer.md.
/// </summary>
public sealed class EventFlagMonitor
{
    public enum MonitorState { Idle, NotRunning, Failed, NotFound, NoCharacter, SpecialLayout, Watching }

    public enum ChangeKind { Flag, Baseline, Load }

    /// <param name="Sequence">order of the change, for a stable newest-first sort</param>
    /// <param name="Count">how many times the flag has changed since the baseline (Flag entries only)</param>
    /// <param name="Written">the change is one Smithbox wrote (not the game)</param>
    public readonly record struct FlagChange(ChangeKind Kind, long Flag, bool On, DateTime At, long Sequence, int Count, bool Written = false);

    /// <summary>More changed bytes than this in one poll is a save being loaded, not gameplay: re-baseline</summary>
    private const int BulkThreshold = 512;

    private const int LogCapacity = 20000;

    // std::map node (MSVC _Tree_node) of the VirtualMemory tree: left, parent, right, isnil, key, entry type, value
    private const int NodeLeft = 0x0, NodeParent = 0x8, NodeRight = 0x10, NodeIsNil = 0x19, NodeKey = 0x20,
        NodeType = 0x28, NodeValue = 0x30, NodeSize = 0x38;

    // Sprj tables (same in DS3 and SDT): groups of { categories*, count }, categories of 10 records { data*, size }
    private const int SprjGroups = 10, SprjGroupStride = 0x18, SprjGroupCount = 0x8, SprjCategoryStride = 0xA8,
        SprjRecordStride = 0x10, SprjRecordSize = 0x8;

    // Sprj world info (FieldArea + eventFlagWorldInfoOffset): count, areas (0x38 each: blocks count, blocks*), blocks
    // (eventFlagWorldBlockStride each: map ID, category)
    private const int WorldCount = 0x8, WorldAreas = 0x10, AreaBlockCount = 0x20, AreaBlocks = 0x28,
        BlockMapId = 0x8, BlockCategory = 0x20;

    private readonly ProjectEntry Project;

    public EventFlagMonitor(ProjectEntry project)
    {
        Project = project;
    }

    private GameLink Link => Project.Handler.GameLink;

    /// <summary>Whether the game's offsets describe its event flags</summary>
    public bool IsSupported => Link.IsSupported &&
        Link.OffsetGroups.Any(g => !string.IsNullOrWhiteSpace(g.eventFlagManAob) && !string.IsNullOrWhiteSpace(g.eventFlagLayout));

    public volatile MonitorState State = MonitorState.Idle;
    public volatile string StateMessage = "";
    public volatile bool IsPolling;
    public volatile bool Paused;
    public DateTime? LastPoll { get; private set; }

    /// <summary>When the current baseline was taken (null while not watching)</summary>
    public DateTime? WatchingSince { get; private set; }

    /// <summary>Goes up whenever the snapshot or the history changes, so views rebuild their rows</summary>
    public int Version => Volatile.Read(ref version);
    private int version;

    #region Configuration

    // Hashed (DS2S): blocks of 10,000 flags in a hash table of bucket lists; node: data*, size u32, key u32 (flag / 10,000), next
    private const int NodeData = 0x0, HashNodeSize = 0x8, HashNodeKey = 0xC, HashNodeNext = 0x10, HashNodeBytes = 0x18;

    private enum Family { VirtualMemory, Sprj, Hashed }

    /// <summary>The resolved offsets of one attach</summary>
    private sealed class Config
    {
        public Family Family;
        public nint ManGlobal, FieldAreaGlobal;
        public int Buffer, SlotCount, BlockBytes, BlockFlags, Tree, LayoutMode = -1;
        public int Groups, Ready, WorldInfo, WorldBlockStride, WorldAreaStride = 0x38;
        public int Buckets, BucketCount;

        /// <summary>Offsets to follow from the global to the manager (DS2S: GameManager + 0x70, + 0x20)</summary>
        public int[] ManPath = Array.Empty<int>();
        public long AlwaysOn = -1, AlwaysOff = -1;
        public string Version;
    }

    private int ResolvedFor = -1;
    private Config Cfg;

    #endregion

    // Current is shared with the UI (under DataLock); Scratch belongs to the poll. Layout is swapped under DataLock.
    private readonly object DataLock = new();
    private EventFlagLayout Layout;
    private byte[] Current = Array.Empty<byte>();
    private byte[] Scratch = Array.Empty<byte>();
    private bool HasBaseline;
    private long Sequence;
    private readonly Dictionary<long, FlagChange> Latest = new();
    private readonly LinkedList<FlagChange> Log = new();

    // Flags Smithbox wrote and hasn't seen change yet: the value written (under DataLock)
    private readonly Dictionary<long, bool> PendingWrites = new();

    /// <summary>Changed bytes that belong to no chunk (padding, or storage Smithbox doesn't know)</summary>
    public int UnknownSlotChanges { get; private set; }

    /// <summary>The game's flag ranges (chunks of 1000) in the current layout; 0 when not built</summary>
    public int ChunkCount => Layout?.ChunkCount ?? 0;

    /// <summary>How many flags the game stores in the current layout; 0 when not built</summary>
    public long StoredFlagCount => Layout is { } l ? (long)l.ChunkCount * l.FlagsPerChunk : 0;

    /// <summary>Chunks the game has whose flag IDs couldn't be worked out</summary>
    public int SkippedChunks => Layout?.SkippedChunks ?? 0;

    #region Polling

    /// <summary>Reads the game once, on a background task (does nothing while a poll runs or when paused)</summary>
    public void RequestPoll()
    {
        if (IsPolling || Paused || !IsSupported)
            return;

        IsPolling = true;

        Task.Run(() =>
        {
            try
            {
                lock (Link.Lock)
                    Poll();
            }
            catch (Exception e)
            {
                SetState(MonitorState.Failed, LOC.Get("EVENTFLAG_Monitor_State_Read_Failed", e.Message));
                DropLayout();
            }
            finally
            {
                LastPoll = DateTime.Now;
                IsPolling = false;
            }
        });
    }

    /// <summary>Closes the game connection (shared with the other Game Link tools); the next poll connects again</summary>
    public void Detach()
    {
        Paused = true;
        Link.Detach();
        ResolvedFor = -1;
        DropLayout();
        SetState(MonitorState.Idle, "");
    }

    /// <summary>Takes a new baseline on the next poll; the history is kept</summary>
    public void ResetBaseline() => DropBaseline();

    public void ClearHistory()
    {
        lock (DataLock)
        {
            Latest.Clear();
            Log.Clear();
            UnknownSlotChanges = 0;
        }
        Interlocked.Increment(ref version);
    }

    private void SetState(MonitorState state, string message)
    {
        State = state;
        StateMessage = message;
    }

    private void DropBaseline()
    {
        lock (DataLock)
        {
            HasBaseline = false;
            WatchingSince = null;
            PendingWrites.Clear();
        }
        Interlocked.Increment(ref version);
    }

    private void DropLayout()
    {
        lock (DataLock)
            Layout = null;
        DropBaseline();
    }

    // Under Link.Lock
    private void Poll()
    {
        if (!Link.EnsureAttached())
        {
            SetState(Link.State == GameLink.LinkState.Failed ? MonitorState.Failed : MonitorState.NotRunning, Link.StateMessage);
            ResolvedFor = -1;
            DropLayout();
            return;
        }

        if (ResolvedFor != Link.AttachId)
        {
            DropLayout();
            Cfg = Resolve();
            ResolvedFor = Link.AttachId;   // a failed resolve isn't retried until the next attach
        }

        if (Cfg == null)
            return;

        // The tables the layout came from: unchanged, or the layout is built again (and a new baseline taken)
        var identity = ReadIdentity(Cfg);
        if (identity == null)
        {
            if (Layout != null)
                DropLayout();
            return;
        }

        var layout = Layout;
        if (layout == null || !layout.Identity.AsSpan().SequenceEqual(identity))
        {
            layout = Cfg.Family switch
            {
                Family.VirtualMemory => BuildVirtualMemory(Cfg, identity),
                Family.Sprj => BuildSprj(Cfg, identity),
                _ => BuildHashed(Cfg, identity)
            };
            lock (DataLock)
                Layout = layout;
            DropBaseline();

            if (layout == null || layout.ChunkCount == 0)
            {
                SetState(MonitorState.NoCharacter, LOC.Get("EVENTFLAG_Monitor_State_No_Character"));
                return;
            }

            Smithbox.Log(typeof(EventFlagMonitor),
                $"[Event Flags] Layout: {layout.ChunkCount} chunks, {layout.Regions.Count} region(s), {layout.Size / 1024} KB" +
                (layout.SkippedChunks > 0 ? $", {layout.SkippedChunks} chunk(s) without known flag IDs" : "") + ".");
        }

        if (Scratch.Length != layout.Size)
            Scratch = new byte[layout.Size];

        foreach (var region in layout.Regions)
        {
            if (!Link.ReadInto(region.Address, Scratch, region.Offset, region.Length))
            {
                SetState(MonitorState.Failed, LOC.Get("EVENTFLAG_Monitor_State_Buffer_Read_Failed", $"0x{region.Address:X}"));
                DropLayout();
                return;
            }
        }

        if (!IsCharacterLoaded(Cfg, layout, Scratch))
        {
            if (HasBaseline)
                DropBaseline();
            SetState(MonitorState.NoCharacter, LOC.Get("EVENTFLAG_Monitor_State_No_Character"));
            return;
        }

        if (!HasBaseline || Current.Length != layout.Size)
        {
            TakeBaseline(ChangeKind.Baseline);
            return;
        }

        var changed = ChangedBytes(Current, Scratch, layout.Size);
        if (changed.Count > BulkThreshold)
        {
            TakeBaseline(ChangeKind.Load);
            return;
        }

        if (changed.Count > 0)
            Record(layout, changed);

        SetState(MonitorState.Watching, LOC.Get("EVENTFLAG_Monitor_State_Watching"));
    }

    private static bool IsCharacterLoaded(Config cfg, EventFlagLayout layout, byte[] data)
    {
        // Known constants: the surest check
        if (cfg.AlwaysOn >= 0 || cfg.AlwaysOff >= 0)
        {
            return (cfg.AlwaysOn < 0 || layout.Get(data, cfg.AlwaysOn)) &&
                   (cfg.AlwaysOff < 0 || !layout.Get(data, cfg.AlwaysOff));
        }

        // Otherwise: an empty flag store is no character (the Sprj ready byte and world info were checked already)
        return cfg.Family != Family.VirtualMemory || data.AsSpan().IndexOfAnyExcept((byte)0) >= 0;
    }

    #endregion

    #region Resolve and layouts

    private Config Resolve()
    {
        foreach (var g in Link.OffsetGroups.Where(g => !string.IsNullOrWhiteSpace(g.eventFlagManAob)))
        {
            var cfg = new Config { Version = g.exeVersion };

            if (!Enum.TryParse(g.eventFlagLayout, true, out cfg.Family))
                continue;

            var manGlobal = FindGlobal(g.eventFlagManAob, g.eventFlagManRelativeOffset, g.exeVersion, "flag manager");
            if (manGlobal == 0)
                continue;
            cfg.ManGlobal = manGlobal;

            // Optional pointer path from the global to the manager ("0x70/0x20")
            if (!string.IsNullOrWhiteSpace(g.eventFlagManPath))
            {
                var path = g.eventFlagManPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(GameLinkOffsets.ParseHex).ToList();
                if (path.Any(p => p == null))
                    continue;
                cfg.ManPath = path.Select(p => p.Value).ToArray();
            }

            if (cfg.Family == Family.Hashed)
            {
                int? buckets = GameLinkOffsets.ParseHex(g.eventFlagBucketsOffset), count = GameLinkOffsets.ParseHex(g.eventFlagBucketCount);
                if (buckets == null || count == null || count <= 0 || count > 4096)
                    continue;

                cfg.Buckets = buckets.Value;
                cfg.BucketCount = count.Value;
            }
            else if (cfg.Family == Family.VirtualMemory)
            {
                int? buffer = GameLinkOffsets.ParseHex(g.eventFlagBufferOffset), slots = GameLinkOffsets.ParseHex(g.eventFlagSlotCountOffset),
                    bytes = GameLinkOffsets.ParseHex(g.eventFlagBlockBytesOffset), flags = GameLinkOffsets.ParseHex(g.eventFlagBlockFlagsOffset),
                    tree = GameLinkOffsets.ParseHex(g.eventFlagTreeOffset);
                if (buffer == null || slots == null || bytes == null || flags == null || tree == null)
                    continue;

                cfg.Buffer = buffer.Value;
                cfg.SlotCount = slots.Value;
                cfg.BlockBytes = bytes.Value;
                cfg.BlockFlags = flags.Value;
                cfg.Tree = tree.Value;
                cfg.LayoutMode = GameLinkOffsets.ParseHex(g.eventFlagLayoutModeOffset) ?? -1;
            }
            else
            {
                int? groups = GameLinkOffsets.ParseHex(g.eventFlagGroupsOffset), ready = GameLinkOffsets.ParseHex(g.eventFlagReadyOffset),
                    world = GameLinkOffsets.ParseHex(g.eventFlagWorldInfoOffset), stride = GameLinkOffsets.ParseHex(g.eventFlagWorldBlockStride);
                if (groups == null || ready == null || world == null || stride == null)
                    continue;

                var fieldArea = FindGlobal(g.eventFlagFieldAreaAob, g.eventFlagFieldAreaRelativeOffset, g.exeVersion, "field area");
                if (fieldArea == 0)
                    continue;

                cfg.FieldAreaGlobal = fieldArea;
                cfg.Groups = groups.Value;
                cfg.Ready = ready.Value;
                cfg.WorldInfo = world.Value;
                cfg.WorldBlockStride = stride.Value;
                cfg.WorldAreaStride = GameLinkOffsets.ParseHex(g.eventFlagWorldAreaStride) ?? 0x38;
            }

            if (long.TryParse(g.eventFlagAlwaysOn, out var alwaysOn))
                cfg.AlwaysOn = alwaysOn;
            if (long.TryParse(g.eventFlagAlwaysOff, out var alwaysOff))
                cfg.AlwaysOff = alwaysOff;

            Smithbox.Log(typeof(EventFlagMonitor),
                $"[Event Flags] {cfg.Family} flags: manager global at module + 0x{cfg.ManGlobal - Link.ModuleBase:X}" +
                (cfg.FieldAreaGlobal != 0 ? $", field area at module + 0x{cfg.FieldAreaGlobal - Link.ModuleBase:X}" : "") +
                $" (offsets group {g.exeVersion}, exe {Link.ExeVersion}).");
            return cfg;
        }

        SetState(MonitorState.NotFound, LOC.Get("EVENTFLAG_Monitor_State_Not_Found", Link.ExeVersion));
        return null;
    }

    /// <summary>The flag manager: the global's value, then the configured pointer path. 0 while any step is null.</summary>
    private nint ReadManager(Config cfg)
    {
        var p = Link.ReadPtr(cfg.ManGlobal);
        foreach (var offset in cfg.ManPath)
        {
            if (p == 0)
                return 0;
            p = Link.ReadPtr(p + offset);
        }
        return p;
    }

    /// <summary>The global an AOB loads; several matches are fine when they all load the same one. 0 if none.</summary>
    private nint FindGlobal(string aob, string relative, string version, string what)
    {
        var rel = GameLinkOffsets.ParseRelative(relative);
        if (string.IsNullOrWhiteSpace(aob) || rel == null)
            return 0;

        var targets = Link.Scan(aob).Select(hit => Link.Relative(hit, rel.Value)).Distinct().ToList();
        if (targets.Count == 1)
            return targets[0];

        Smithbox.Log(typeof(EventFlagMonitor), $"[Event Flags] Group {version}: the {what} AOB gave {targets.Count} different globals.");
        return 0;
    }

    /// <summary>
    /// The values the layout depends on, read fresh; null (with the state set) while the game has no flags to show
    /// </summary>
    private byte[] ReadIdentity(Config cfg)
    {
        var manager = ReadManager(cfg);
        if (manager == 0)
        {
            // DS2S reaches its manager through the game manager: null outside a loaded game
            if (cfg.Family == Family.Hashed)
                SetState(MonitorState.NoCharacter, LOC.Get("EVENTFLAG_Monitor_State_No_Character"));
            else
                SetState(MonitorState.NotFound, LOC.Get("EVENTFLAG_Monitor_State_No_Manager"));
            return null;
        }

        var id = new List<byte>();
        void Add(long value) => id.AddRange(BitConverter.GetBytes(value));

        if (cfg.Family == Family.Hashed)
        {
            var nodes = WalkHashed(cfg, manager);
            if (nodes.Count == 0)
            {
                SetState(MonitorState.NoCharacter, LOC.Get("EVENTFLAG_Monitor_State_No_Character"));
                return null;
            }

            Add(manager);
            foreach (var n in nodes)
            {
                Add(n.Node);
                Add(n.Data);
                Add(((long)n.Key << 32) | (uint)n.Size);
            }
        }
        else if (cfg.Family == Family.VirtualMemory)
        {
            var size = new[] { cfg.Buffer + 8, cfg.SlotCount + 4, cfg.BlockBytes + 4, cfg.BlockFlags + 4, cfg.Tree + 16,
                cfg.LayoutMode >= 0 ? cfg.LayoutMode + 4 : 0 }.Max();
            var header = Link.ReadBytes(manager, size);

            var buffer = BitConverter.ToInt64(header, cfg.Buffer);
            var slots = BitConverter.ToInt32(header, cfg.SlotCount);
            var blockBytes = BitConverter.ToInt32(header, cfg.BlockBytes);
            var blockFlags = BitConverter.ToInt32(header, cfg.BlockFlags);

            if (blockFlags != 1000 || blockBytes != 125 || slots < 0 || slots > 1_000_000)
            {
                SetState(MonitorState.Failed, LOC.Get("EVENTFLAG_Monitor_State_Layout_Mismatch", slots, blockBytes, blockFlags));
                return null;
            }

            if (buffer == 0)
            {
                SetState(MonitorState.NotFound, LOC.Get("EVENTFLAG_Monitor_State_No_Manager"));
                return null;
            }

            Add(manager);
            Add(buffer);
            Add(slots);
            Add(BitConverter.ToInt64(header, cfg.Tree));        // head node
            Add(BitConverter.ToInt64(header, cfg.Tree + 8));    // node count
            if (cfg.LayoutMode >= 0)
                Add(BitConverter.ToInt32(header, cfg.LayoutMode));
        }
        else
        {
            var header = Link.ReadBytes(manager + cfg.Groups, Math.Max(8, cfg.Ready - cfg.Groups + 1));
            var groups = (nint)BitConverter.ToInt64(header, 0);
            var ready = cfg.Ready >= cfg.Groups ? header[cfg.Ready - cfg.Groups] : Link.ReadBytes(manager + cfg.Ready, 1)[0];

            var worldCount = 0;
            var fieldArea = Link.ReadPtr(cfg.FieldAreaGlobal);
            if (fieldArea != 0)
            {
                var owner = Link.ReadPtr(fieldArea + cfg.WorldInfo);
                if (owner != 0)
                    worldCount = Link.ReadI32(owner + WorldCount);
            }

            if (ready == 0 || groups == 0 || worldCount <= 0)
            {
                SetState(MonitorState.NoCharacter, LOC.Get("EVENTFLAG_Monitor_State_No_Character"));
                return null;
            }

            Add(manager);
            Add(groups);
            Add(worldCount);
            id.AddRange(Link.ReadBytes(groups, SprjGroups * SprjGroupStride));
        }

        return id.ToArray();
    }

    private readonly record struct HashNode(nint Node, nint Data, int Size, uint Key);

    /// <summary>Every node of the DS2S hash table: BucketCount lists from the manager's bucket array</summary>
    private List<HashNode> WalkHashed(Config cfg, nint manager)
    {
        var nodes = new List<HashNode>();
        var heads = Link.ReadBytes(manager + cfg.Buckets, cfg.BucketCount * 8);

        for (var b = 0; b < cfg.BucketCount; b++)
        {
            var node = (nint)BitConverter.ToInt64(heads, b * 8);
            var guard = 0;
            while (node != 0 && guard++ < 10000)
            {
                // A value that can't be a heap node ends the list (wrong offsets, or a list being rebuilt)
                if (!IsHeapPointer(node))
                {
                    InvalidNodes++;
                    break;
                }

                var bytes = Link.ReadBytes(node, HashNodeBytes);
                nodes.Add(new HashNode(node, (nint)BitConverter.ToInt64(bytes, NodeData),
                    BitConverter.ToInt32(bytes, HashNodeSize), BitConverter.ToUInt32(bytes, HashNodeKey)));
                node = (nint)BitConverter.ToInt64(bytes, HashNodeNext);
            }
        }

        return nodes;
    }

    /// <summary>Hash table nodes skipped because their address couldn't be a pointer (logged once per layout)</summary>
    private int InvalidNodes;

    /// <summary>An 8-byte aligned user-mode address above the null page</summary>
    private static bool IsHeapPointer(nint p) => p > 0x10000 && p < 0x7FFF_FFFF_0000 && (p & 7) == 0;

    // TEMP diagnostics for the first DS2S check (2026-11-26): the pointer chain, bucket heads and nodes, once per layout
    private const string TempDiagnosticsPath = @"F:\Programming\C#\Smithbox\Claude\Logs\EventFlags-DS2S.log";

    private void TempDiagnostics(Config cfg, nint manager)
    {
        try
        {
            var lines = new List<string> { $"--- {DateTime.Now:yyyy-MM-dd HH:mm:ss} exe {Link.ExeVersion}" };
            var p = Link.ReadPtr(cfg.ManGlobal);
            lines.Add($"global 0x{cfg.ManGlobal:X} (module + 0x{cfg.ManGlobal - Link.ModuleBase:X}) -> 0x{p:X}");
            foreach (var offset in cfg.ManPath)
            {
                var next = p == 0 ? 0 : Link.ReadPtr(p + offset);
                lines.Add($"  [0x{p:X} + 0x{offset:X}] -> 0x{next:X}");
                p = next;
            }
            lines.Add($"manager 0x{manager:X}, buckets at +0x{cfg.Buckets:X} x {cfg.BucketCount}");

            var heads = Link.ReadBytes(manager + cfg.Buckets, cfg.BucketCount * 8);
            lines.Add("heads: " + string.Join(" ", Enumerable.Range(0, cfg.BucketCount).Select(b => $"{BitConverter.ToInt64(heads, b * 8):X}")));

            var nodes = WalkHashed(cfg, manager);
            lines.Add($"nodes {nodes.Count}, invalid {InvalidNodes}");
            foreach (var n in nodes.OrderBy(n => n.Key).Take(400))
                lines.Add($"  key {n.Key} (flags {n.Key * 10_000L}-{n.Key * 10_000L + 9_999}) size {n.Size} data 0x{n.Data:X} node 0x{n.Node:X}");

            TempDiagnosticsLines(lines);
        }
        catch (Exception e)
        {
            Smithbox.Log(typeof(EventFlagMonitor), $"[Event Flags] Diagnostics log failed: {e.Message}");
        }
    }

    private static void TempDiagnosticsLine(string line) => TempDiagnosticsLines(new[] { line });

    private static void TempDiagnosticsLines(IEnumerable<string> lines)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(TempDiagnosticsPath));
            File.AppendAllLines(TempDiagnosticsPath, lines);
        }
        catch (Exception e)
        {
            Smithbox.Log(typeof(EventFlagMonitor), $"[Event Flags] Diagnostics log failed: {e.Message}");
        }
    }

    /// <summary>TEMP: the ON flags of a snapshot, decoded the same way as <see cref="OnFlags"/></summary>
    private static IEnumerable<long> OnFlagsIn(EventFlagLayout layout, byte[] data)
    {
        foreach (var baseFlag in layout.BaseFlags)
        {
            var chunk = layout.ChunkOf(baseFlag);
            for (var i = chunk.Offset; i < chunk.Offset + chunk.Length && i < data.Length; i++)
            {
                for (var bit = 0; bit < 8; bit++)
                {
                    if ((data[i] & (1 << bit)) == 0)
                        continue;
                    var flag = layout.FlagAt(chunk, i, bit);
                    if (flag >= 0)
                        yield return baseFlag + (flag - chunk.BaseFlag);
                }
            }
        }
    }

    private EventFlagLayout BuildHashed(Config cfg, byte[] identity)
    {
        InvalidNodes = 0;
        var manager = ReadManager(cfg);
        TempDiagnostics(cfg, manager);
        InvalidNodes = 0;
        var chunks = new List<(long, nint, int)>();
        var skipped = 0;

        foreach (var n in WalkHashed(cfg, manager))
        {
            if (n.Data == 0 || n.Size <= 0 || n.Size > 1250)
            {
                skipped++;
                continue;
            }
            chunks.Add((n.Key * 10_000L, n.Data, n.Size));
        }

        if (InvalidNodes > 0)
            Smithbox.Log(typeof(EventFlagMonitor), $"[Event Flags] {InvalidNodes} hash list(s) ended at an address that isn't a pointer.");

        return new EventFlagLayout(EventFlagLayout.BitOrder.ByteMsb, 10_000, 1250, chunks, identity, skipped);
    }

    private EventFlagLayout BuildVirtualMemory(Config cfg, byte[] identity)
    {
        var manager = ReadManager(cfg);
        var header = Link.ReadBytes(manager, Math.Max(cfg.Buffer + 8, Math.Max(cfg.SlotCount + 4, cfg.Tree + 16)));
        var buffer = (nint)BitConverter.ToInt64(header, cfg.Buffer);
        var slots = BitConverter.ToInt32(header, cfg.SlotCount);
        var head = (nint)BitConverter.ToInt64(header, cfg.Tree);
        var count = BitConverter.ToInt64(header, cfg.Tree + 8);

        var chunks = new List<(long, nint)>();
        var skipped = 0;
        if (head == 0 || count <= 0 || count > 1_000_000)
            return new EventFlagLayout(EventFlagLayout.BitOrder.ByteMsb, 1000, 125, chunks, identity, 0);

        // In-order walk of the std::map, from the root (head's parent); leaves point back at the head (isnil)
        var stack = new Stack<byte[]>();
        var node = Link.ReadPtr(head + NodeParent);
        var visited = 0L;

        while ((stack.Count > 0 || node != 0) && visited <= count + 16)
        {
            while (node != 0 && node != head)
            {
                var bytes = Link.ReadBytes(node, NodeSize);
                if (bytes[NodeIsNil] != 0)
                    break;
                stack.Push(bytes);
                node = (nint)BitConverter.ToInt64(bytes, NodeLeft);
            }

            if (stack.Count == 0)
                break;

            var n = stack.Pop();
            visited++;

            var block = BitConverter.ToUInt32(n, NodeKey);
            var type = BitConverter.ToInt32(n, NodeType);
            var value = BitConverter.ToInt64(n, NodeValue);

            if (type == 1 && value >= 0 && value < slots)
                chunks.Add((block * 1000L, buffer + (nint)(value * 125)));
            else if (type == 2 && value != 0)
                chunks.Add((block * 1000L, (nint)value));
            else
                skipped++;

            node = (nint)BitConverter.ToInt64(n, NodeRight);
        }

        return new EventFlagLayout(EventFlagLayout.BitOrder.ByteMsb, 1000, 125, chunks, identity, skipped);
    }

    private EventFlagLayout BuildSprj(Config cfg, byte[] identity)
    {
        var manager = ReadManager(cfg);
        var groups = Link.ReadPtr(manager + cfg.Groups);

        // World blocks: category c (1-based in the flag tables) → its area and block
        var blocks = new Dictionary<int, (int Area, int Block)>();
        var fieldArea = Link.ReadPtr(cfg.FieldAreaGlobal);
        var owner = fieldArea != 0 ? Link.ReadPtr(fieldArea + cfg.WorldInfo) : 0;
        if (owner != 0)
        {
            var areaCount = Link.ReadI32(owner + WorldCount);
            var areasPtr = Link.ReadPtr(owner + WorldAreas);
            if (areaCount > 0 && areaCount < 1000 && areasPtr != 0)
            {
                var areas = Link.ReadBytes(areasPtr, areaCount * cfg.WorldAreaStride);
                for (var a = 0; a < areaCount; a++)
                {
                    var blockCount = BitConverter.ToInt32(areas, a * cfg.WorldAreaStride + AreaBlockCount);
                    var blocksPtr = (nint)BitConverter.ToInt64(areas, a * cfg.WorldAreaStride + AreaBlocks);
                    if (blockCount <= 0 || blockCount > 1000 || blocksPtr == 0)
                        continue;

                    var info = Link.ReadBytes(blocksPtr, blockCount * cfg.WorldBlockStride);
                    for (var b = 0; b < blockCount; b++)
                    {
                        var mapId = BitConverter.ToUInt32(info, b * cfg.WorldBlockStride + BlockMapId);
                        var category = BitConverter.ToInt32(info, b * cfg.WorldBlockStride + BlockCategory);
                        if (category >= 0)
                            blocks.TryAdd(category + 1, ((int)(mapId >> 24), (int)((mapId >> 16) & 0xFF)));
                    }
                }
            }
        }

        var chunks = new List<(long, nint)>();
        var skipped = 0;
        var table = Link.ReadBytes(groups, SprjGroups * SprjGroupStride);

        for (var g = 0; g < SprjGroups; g++)
        {
            var categories = (nint)BitConverter.ToInt64(table, g * SprjGroupStride);
            var categoryCount = BitConverter.ToInt32(table, g * SprjGroupStride + SprjGroupCount);
            if (categories == 0 || categoryCount <= 0 || categoryCount > 10000)
                continue;

            var records = Link.ReadBytes(categories, categoryCount * SprjCategoryStride);
            for (var c = 0; c < categoryCount; c++)
            {
                long? baseOfCategory = null;
                if (c == 0)
                    baseOfCategory = g * 10_000_000L;   // global: area 0, block 0 (areas 90+ share it)
                else if (blocks.TryGetValue(c, out var wb) && wb.Area < 100 && wb.Block < 10)
                    baseOfCategory = g * 10_000_000L + wb.Area * 100_000L + wb.Block * 10_000L;

                for (var k = 0; k < 10; k++)
                {
                    var r = c * SprjCategoryStride + k * SprjRecordStride;
                    var data = (nint)BitConverter.ToInt64(records, r);
                    var size = BitConverter.ToInt64(records, r + SprjRecordSize);
                    if (data == 0 || size < 128)
                        continue;

                    if (baseOfCategory == null)
                    {
                        skipped++;
                        continue;
                    }

                    chunks.Add((baseOfCategory.Value + k * 1000L, data));
                }
            }
        }

        return new EventFlagLayout(EventFlagLayout.BitOrder.DwordMsb, 1000, 128, chunks, identity, skipped);
    }

    #endregion

    #region Snapshot

    private void TakeBaseline(ChangeKind kind)
    {
        lock (DataLock)
        {
            (Current, Scratch) = (Scratch, Current);
            HasBaseline = true;
            WatchingSince = DateTime.Now;
            PendingWrites.Clear();
            AddToLog(new FlagChange(kind, -1, false, DateTime.Now, ++Sequence, 0));
        }
        Interlocked.Increment(ref version);

        SetState(MonitorState.Watching, LOC.Get("EVENTFLAG_Monitor_State_Watching"));
    }

    private static List<int> ChangedBytes(byte[] a, byte[] b, int size)
    {
        var changed = new List<int>();
        var words = size / 8;
        var wa = MemoryMarshal.Cast<byte, ulong>(a.AsSpan(0, words * 8));
        var wb = MemoryMarshal.Cast<byte, ulong>(b.AsSpan(0, words * 8));

        for (var i = 0; i < words; i++)
        {
            if (wa[i] == wb[i])
                continue;

            for (var j = i * 8; j < i * 8 + 8; j++)
            {
                if (a[j] != b[j])
                    changed.Add(j);
            }

            if (changed.Count > BulkThreshold)
                return changed;
        }

        for (var j = words * 8; j < size; j++)
        {
            if (a[j] != b[j])
                changed.Add(j);
        }

        return changed;
    }

    private void Record(EventFlagLayout layout, List<int> changed)
    {
        var now = DateTime.Now;

        lock (DataLock)
        {
            foreach (var index in changed)
            {
                var chunk = layout.ChunkAt(index);
                if (chunk == null)
                {
                    UnknownSlotChanges++;
                    continue;
                }

                var diff = Current[index] ^ Scratch[index];
                for (var bit = 0; bit < 8; bit++)
                {
                    var mask = 1 << bit;
                    if ((diff & mask) == 0)
                        continue;

                    var flag = layout.FlagAt(chunk, index, bit);
                    if (flag < 0)
                    {
                        UnknownSlotChanges++;
                        continue;
                    }

                    var value = (Scratch[index] & mask) != 0;
                    var count = Latest.TryGetValue(flag, out var last) ? last.Count + 1 : 1;
                    var written = PendingWrites.Remove(flag, out var expected) && expected == value;
                    var change = new FlagChange(ChangeKind.Flag, flag, value, now, ++Sequence, count, written);

                    Latest[flag] = change;
                    AddToLog(change);
                }
            }

            (Current, Scratch) = (Scratch, Current);
        }

        Interlocked.Increment(ref version);
    }

    private void AddToLog(FlagChange change)
    {
        Log.AddFirst(change);
        while (Log.Count > LogCapacity)
            Log.RemoveLast();
    }

    #endregion

    #region Writing

    public volatile bool IsWriting;

    /// <summary>The result of the last write, for the UI (null before the first)</summary>
    public volatile string LastWrite;
    public volatile bool LastWriteFailed;

    /// <summary>
    /// Sets or clears a flag in the running game, on a background task: the byte holding it is read, the bit changed
    /// and the byte written back. Only while watching, only for flags the game stores, and only if the game's tables
    /// are still the ones the layout came from. The next poll records the change (marked as written by Smithbox). The
    /// game saves it with the character the next time it saves.
    /// </summary>
    public void RequestWrite(long flag, bool on)
    {
        if (IsWriting || !IsSupported)
            return;

        IsWriting = true;
        var operation = Link.BeginOperation();

        Task.Run(() =>
        {
            try
            {
                lock (Link.Lock)
                    Write(flag, on);
            }
            catch (Exception e)
            {
                Report(true, LOC.Get("EVENTFLAG_Write_Failed", flag, e.Message));
            }
            finally
            {
                operation.Dispose();
                IsWriting = false;
            }
        });
    }

    private void Report(bool failed, string message)
    {
        LastWriteFailed = failed;
        LastWrite = message;
    }

    // Under Link.Lock
    private void Write(long flag, bool on)
    {
        var layout = Layout;
        var cfg = Cfg;

        if (State != MonitorState.Watching || layout == null || cfg == null || !Link.IsOpen || ResolvedFor != Link.AttachId)
        {
            Report(true, LOC.Get("EVENTFLAG_Write_Not_Watching"));
            return;
        }

        if (!layout.Locate(flag, out var index, out var mask))
        {
            Report(true, LOC.Get("EVENTFLAG_Write_Not_Stored", flag));
            return;
        }

        // The write must go where the game keeps the flag now
        var identity = ReadIdentity(cfg);
        if (identity == null || !layout.Identity.AsSpan().SequenceEqual(identity))
        {
            Report(true, LOC.Get("EVENTFLAG_Write_Not_Watching"));
            return;
        }

        var address = layout.AddressOf(index);
        var current = Link.ReadBytes(address, 1)[0];
        var next = on ? (byte)(current | mask) : (byte)(current & ~mask);

        var state = LOC.Get(on ? "EVENTFLAG_Tool_On" : "EVENTFLAG_Tool_Off");
        if (next == current)
        {
            Report(false, LOC.Get("EVENTFLAG_Write_Already", flag, state));
            return;
        }

        lock (DataLock)
            PendingWrites[flag] = on;

        Link.WriteBytes(address, new[] { next });
        Smithbox.Log(typeof(EventFlagMonitor), $"[Event Flags] Set flag {flag} {(on ? "ON" : "OFF")}.");
        Report(false, LOC.Get("EVENTFLAG_Write_Done", flag, state));
    }

    #endregion

    #region Queries (for the UI)

    /// <summary>Whether the baseline is taken and the snapshot is current</summary>
    public bool IsWatching
    {
        get
        {
            lock (DataLock)
                return HasBaseline;
        }
    }

    /// <summary>Whether the game stores the flag; null before the game's tables are read</summary>
    public bool? IsStored(long flag)
    {
        var layout = Layout;
        return layout == null ? null : layout.IsStored(flag);
    }

    /// <summary>The flag's value in the latest read; null when not watching or the game doesn't store the flag</summary>
    public bool? IsOn(long flag)
    {
        lock (DataLock)
        {
            if (!HasBaseline || Layout == null || !Layout.Locate(flag, out var index, out var mask) || index >= Current.Length)
                return null;
            return (Current[index] & mask) != 0;
        }
    }

    /// <summary>The last change of each flag that changed since the history was cleared, newest first</summary>
    public List<FlagChange> RecentChanges()
    {
        lock (DataLock)
            return Latest.Values.OrderByDescending(c => c.Sequence).ToList();
    }

    /// <summary>Every change (and baseline / load marker), newest first, up to the last 20,000</summary>
    public List<FlagChange> ChangeLog()
    {
        lock (DataLock)
            return Log.ToList();
    }

    /// <summary>The last change of a flag since the history was cleared, if any</summary>
    public FlagChange? LastChange(long flag)
    {
        lock (DataLock)
            return Latest.TryGetValue(flag, out var c) ? c : null;
    }

    /// <summary>Every flag the game stores in a range, in ID order (empty before the game's tables are read)</summary>
    public List<long> StoredFlags(long first, long last, int limit)
    {
        var result = new List<long>();
        var layout = Layout;
        if (layout == null)
            return result;

        foreach (var baseFlag in layout.BaseFlags)
        {
            if (baseFlag + layout.FlagsPerChunk - 1 < first)
                continue;
            if (baseFlag > last || result.Count > limit)
                break;

            for (var n = 0; n < layout.FlagsPerChunk; n++)
            {
                var flag = baseFlag + n;
                if (flag >= first && flag <= last && flag > 0)
                    result.Add(flag);
            }
        }

        return result;
    }

    /// <summary>Every flag that is ON in the latest read, in ID order</summary>
    public List<long> OnFlags(long first = 0, long last = long.MaxValue)
    {
        var result = new List<long>();

        lock (DataLock)
        {
            var layout = Layout;
            if (!HasBaseline || layout == null)
                return result;

            foreach (var baseFlag in layout.BaseFlags)
            {
                if (baseFlag + layout.FlagsPerChunk - 1 < first)
                    continue;
                if (baseFlag > last)
                    break;

                var chunk = layout.ChunkOf(baseFlag);
                for (var i = chunk.Offset; i < chunk.Offset + chunk.Length && i < Current.Length; i++)
                {
                    var b = Current[i];
                    if (b == 0)
                        continue;

                    for (var bit = 0; bit < 8; bit++)
                    {
                        if ((b & (1 << bit)) == 0)
                            continue;

                        var flag = layout.FlagAt(chunk, i, bit);
                        if (flag < 0)
                            continue;

                        // A chunk shared by several ranges reports the range asked for
                        flag = baseFlag + (flag - chunk.BaseFlag);
                        if (flag >= first && flag <= last)
                            result.Add(flag);
                    }
                }
            }
        }

        result.Sort();
        return result;
    }

    #endregion
}
