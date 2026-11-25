using StudioCore.Memory;
using System.Runtime.InteropServices;

namespace StudioCore.Editors.Common;

/// <summary>
/// Watches the running game's event flags (ER): reads the flag manager's slot buffer (slots × 125 bytes, one bit per
/// flag, MSB first) on each poll, compares it with the last read and records which flags changed and when. The
/// slot → block mapping is the game's own, rebuilt from the project's data by <see cref="ErEventFlagAllocation"/>.
/// The first read after attaching (or after a save is loaded) is the baseline: nothing counts as changed.
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

    private const long AlwaysOn = 6001, AlwaysOff = 6000;

    private readonly ProjectEntry Project;

    public EventFlagMonitor(ProjectEntry project)
    {
        Project = project;
    }

    private GameLink Link => Project.Handler.GameLink;
    private ErEventFlagAllocation Model => Project.Handler.ProjectData?.EventFlags;

    /// <summary>Whether this project has a flag model and the game's offsets name the flag manager</summary>
    public bool IsSupported => Model != null && Link.IsSupported &&
        Link.OffsetGroups.Any(g => !string.IsNullOrWhiteSpace(g.eventFlagManAob));

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

    // Found once per attach (the buffer is allocated once at startup and never moves)
    private int ResolvedFor = -1;
    private nint Global;
    private int BufferOffset, SlotCountOffset, BlockBytesOffset, BlockFlagsOffset, LayoutModeOffset, HeaderSize;

    // Current is shared with the UI (under DataLock); Scratch belongs to the poll
    private readonly object DataLock = new();
    private byte[] Current = Array.Empty<byte>();
    private byte[] Scratch = Array.Empty<byte>();
    private bool HasBaseline;
    private long Sequence;
    private readonly Dictionary<long, FlagChange> Latest = new();
    private readonly LinkedList<FlagChange> Log = new();

    // Flags Smithbox wrote and hasn't seen change yet: the value written (under DataLock)
    private readonly Dictionary<long, bool> PendingWrites = new();

    /// <summary>Changed bytes whose slot no block uses (the project's alloc lists differ from the game's)</summary>
    public int UnknownSlotChanges { get; private set; }

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
                DropBaseline();
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
        DropBaseline();
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

    // Under Link.Lock
    private void Poll()
    {
        var model = Model;
        if (model == null)
            return;

        if (!Link.EnsureAttached())
        {
            SetState(Link.State == GameLink.LinkState.Failed ? MonitorState.Failed : MonitorState.NotRunning, Link.StateMessage);
            ResolvedFor = -1;
            DropBaseline();
            return;
        }

        if (ResolvedFor != Link.AttachId)
        {
            DropBaseline();
            if (!Resolve())
                return;
        }

        // Not found for this attach (state already set): wait for the next attach
        if (Global == 0)
            return;

        // The manager is created at startup: 0 only while the game is still starting
        var manager = Link.ReadPtr(Global);
        if (manager == 0)
        {
            SetState(MonitorState.NotFound, LOC.Get("EVENTFLAG_Monitor_State_No_Manager"));
            return;
        }

        var header = Link.ReadBytes(manager, HeaderSize);
        var buffer = (nint)BitConverter.ToInt64(header, BufferOffset);
        var slots = BitConverter.ToInt32(header, SlotCountOffset);
        var blockBytes = BitConverter.ToInt32(header, BlockBytesOffset);
        var blockFlags = BitConverter.ToInt32(header, BlockFlagsOffset);
        var mode = BitConverter.ToInt32(header, LayoutModeOffset);

        if (blockFlags != ErEventFlagAllocation.FlagsPerBlock || blockBytes != ErEventFlagAllocation.BytesPerBlock ||
            slots != ErEventFlagAllocation.Slots)
        {
            SetState(MonitorState.Failed, LOC.Get("EVENTFLAG_Monitor_State_Layout_Mismatch", slots, blockBytes, blockFlags));
            DropBaseline();
            return;
        }

        if (buffer == 0)
        {
            SetState(MonitorState.NotFound, LOC.Get("EVENTFLAG_Monitor_State_No_Manager"));
            return;
        }

        if (mode != 0)
        {
            SetState(MonitorState.SpecialLayout, LOC.Get("EVENTFLAG_Monitor_State_Special_Layout", mode));
            DropBaseline();
            return;
        }

        var size = slots * blockBytes;
        if (Scratch.Length != size)
            Scratch = new byte[size];

        if (!Link.ReadInto(buffer, Scratch, size))
        {
            SetState(MonitorState.Failed, LOC.Get("EVENTFLAG_Monitor_State_Buffer_Read_Failed", $"0x{buffer:X}"));
            DropBaseline();
            return;
        }

        // On the title screen (or while a save loads) the buffer doesn't hold a character
        var on = Bit(model, Scratch, AlwaysOn);
        var off = Bit(model, Scratch, AlwaysOff);
        if (on != true || off != false)
        {
            if (HasBaseline)
                DropBaseline();
            SetState(MonitorState.NoCharacter, LOC.Get("EVENTFLAG_Monitor_State_No_Character"));
            return;
        }

        if (!HasBaseline || Current.Length != size)
        {
            TakeBaseline(ChangeKind.Baseline);
            return;
        }

        var changed = ChangedBytes(Current, Scratch, size);
        if (changed.Count > BulkThreshold)
        {
            TakeBaseline(ChangeKind.Load);
            return;
        }

        if (changed.Count > 0)
            Record(model, changed);

        SetState(MonitorState.Watching, LOC.Get("EVENTFLAG_Monitor_State_Watching"));
    }

    private bool Resolve()
    {
        foreach (var g in Link.OffsetGroups.Where(g => !string.IsNullOrWhiteSpace(g.eventFlagManAob)))
        {
            var rel = GameLinkOffsets.ParseRelative(g.eventFlagManRelativeOffset);
            var bufferOffset = GameLinkOffsets.ParseHex(g.eventFlagBufferOffset);
            var slotCount = GameLinkOffsets.ParseHex(g.eventFlagSlotCountOffset);
            var blockBytes = GameLinkOffsets.ParseHex(g.eventFlagBlockBytesOffset);
            var blockFlags = GameLinkOffsets.ParseHex(g.eventFlagBlockFlagsOffset);
            var layoutMode = GameLinkOffsets.ParseHex(g.eventFlagLayoutModeOffset);
            if (rel == null || bufferOffset == null || slotCount == null || blockBytes == null || blockFlags == null || layoutMode == null)
                continue;

            var hits = Link.Scan(g.eventFlagManAob);
            if (hits.Count != 1)
            {
                Smithbox.Log(typeof(EventFlagMonitor), $"[Event Flags] Group {g.exeVersion}: {hits.Count} matches for the flag manager AOB.");
                continue;
            }

            Global = Link.Relative(hits[0], rel.Value);
            BufferOffset = bufferOffset.Value;
            SlotCountOffset = slotCount.Value;
            BlockBytesOffset = blockBytes.Value;
            BlockFlagsOffset = blockFlags.Value;
            LayoutModeOffset = layoutMode.Value;
            HeaderSize = new[] { BufferOffset + 8, SlotCountOffset + 4, BlockBytesOffset + 4, BlockFlagsOffset + 4, LayoutModeOffset + 4 }.Max();
            ResolvedFor = Link.AttachId;

            Smithbox.Log(typeof(EventFlagMonitor),
                $"[Event Flags] Flag manager global at module + 0x{Global - Link.ModuleBase:X} (offsets group {g.exeVersion}, exe {Link.ExeVersion}).");
            return true;
        }

        SetState(MonitorState.NotFound, LOC.Get("EVENTFLAG_Monitor_State_Not_Found", Link.ExeVersion));
        ResolvedFor = Link.AttachId;   // don't scan again until the next attach
        Global = 0;
        return false;
    }

    private void TakeBaseline(ChangeKind kind)
    {
        lock (DataLock)
        {
            (Current, Scratch) = (Scratch, Current);
            HasBaseline = true;
            WatchingSince = DateTime.Now;
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

    private void Record(ErEventFlagAllocation model, List<int> changed)
    {
        var now = DateTime.Now;

        lock (DataLock)
        {
            foreach (var index in changed)
            {
                var block = model.BlockOfSlot(index / ErEventFlagAllocation.BytesPerBlock);
                if (block < 0)
                {
                    UnknownSlotChanges++;
                    continue;
                }

                var offset = index % ErEventFlagAllocation.BytesPerBlock;
                var diff = Current[index] ^ Scratch[index];

                for (var bit = 0; bit < 8; bit++)
                {
                    var mask = 0x80 >> bit;
                    if ((diff & mask) == 0)
                        continue;

                    var flag = block * (long)ErEventFlagAllocation.FlagsPerBlock + offset * 8 + bit;
                    var count = Latest.TryGetValue(flag, out var last) ? last.Count + 1 : 1;
                    var value = (Scratch[index] & mask) != 0;
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

    private static bool? Bit(ErEventFlagAllocation model, byte[] data, long flag)
    {
        var slot = model.SlotOfBlock((int)(flag / ErEventFlagAllocation.FlagsPerBlock));
        if (slot < 0)
            return null;

        var index = slot * ErEventFlagAllocation.BytesPerBlock + (int)(flag % ErEventFlagAllocation.FlagsPerBlock) / 8;
        return index < data.Length ? (data[index] & (0x80 >> (int)(flag % 8))) != 0 : null;
    }

    #endregion

    #region Writing

    public volatile bool IsWriting;

    /// <summary>The result of the last write, for the UI (null before the first)</summary>
    public volatile string LastWrite;
    public volatile bool LastWriteFailed;

    /// <summary>
    /// Sets or clears a flag in the running game, on a background task: one byte of the flag buffer is read, the bit
    /// changed and the byte written back. Only while watching normal play, and only for flags the game stores. The
    /// next poll records the change like any other (marked as written by Smithbox). The game saves it with the
    /// character the next time it saves.
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
        var model = Model;
        var slot = model?.SlotOfBlock((int)(flag / ErEventFlagAllocation.FlagsPerBlock)) ?? -1;
        if (flag <= 0 || flag > uint.MaxValue || slot < 0)
        {
            Report(true, LOC.Get("EVENTFLAG_Write_Not_Stored", flag));
            return;
        }

        if (State != MonitorState.Watching || !Link.IsOpen || ResolvedFor != Link.AttachId || Global == 0)
        {
            Report(true, LOC.Get("EVENTFLAG_Write_Not_Watching"));
            return;
        }

        // Read the header again: the write must go to the buffer of normal play, as it is now
        var manager = Link.ReadPtr(Global);
        if (manager == 0)
        {
            Report(true, LOC.Get("EVENTFLAG_Write_Not_Watching"));
            return;
        }

        var header = Link.ReadBytes(manager, HeaderSize);
        var buffer = (nint)BitConverter.ToInt64(header, BufferOffset);
        if (buffer == 0 || BitConverter.ToInt32(header, LayoutModeOffset) != 0 ||
            BitConverter.ToInt32(header, SlotCountOffset) != ErEventFlagAllocation.Slots ||
            BitConverter.ToInt32(header, BlockBytesOffset) != ErEventFlagAllocation.BytesPerBlock)
        {
            Report(true, LOC.Get("EVENTFLAG_Write_Not_Watching"));
            return;
        }

        var address = buffer + slot * ErEventFlagAllocation.BytesPerBlock + (int)(flag % ErEventFlagAllocation.FlagsPerBlock) / 8;
        var mask = (byte)(0x80 >> (int)(flag % 8));
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

    /// <summary>The flag's value in the latest read; null when not watching or the game doesn't store the flag</summary>
    public bool? IsOn(long flag)
    {
        var model = Model;
        if (model == null || flag <= 0)
            return null;

        lock (DataLock)
            return HasBaseline ? Bit(model, Current, flag) : null;
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

    /// <summary>Every flag that is ON in the latest read, in ID order</summary>
    public List<long> OnFlags(long first = 0, long last = long.MaxValue)
    {
        var result = new List<long>();
        var model = Model;
        if (model == null)
            return result;

        lock (DataLock)
        {
            if (!HasBaseline)
                return result;

            foreach (var block in model.BufferBlocks)
            {
                long blockFirst = block * (long)ErEventFlagAllocation.FlagsPerBlock;
                if (blockFirst + ErEventFlagAllocation.FlagsPerBlock - 1 < first || blockFirst > last)
                    continue;

                var start = model.SlotOfBlock(block) * ErEventFlagAllocation.BytesPerBlock;
                for (var i = 0; i < ErEventFlagAllocation.BytesPerBlock; i++)
                {
                    var b = Current[start + i];
                    if (b == 0)
                        continue;

                    for (var bit = 0; bit < 8; bit++)
                    {
                        var flag = blockFirst + i * 8 + bit;
                        if ((b & (0x80 >> bit)) != 0 && flag >= first && flag <= last)
                            result.Add(flag);
                    }
                }
            }
        }

        return result;
    }

    #endregion
}
