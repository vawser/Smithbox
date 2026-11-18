using StudioCore.Application;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Memory;

/// <summary>
/// A connection to the project's running game, shared by the tools that change it live (the Material Editor's
/// Material Reloader, the Text Editor's FMG Reloader, the Map Editor's World Reloader): one process handle, one lock,
/// one "busy" state.
///
/// Owns the game actions that aren't any one tool's: quit to title and reload the world. Which exe to attach to and
/// where those actions are come from Assets/Game Link/{game}/*.json (GameLinkOffsets).
///
/// Tools find their own addresses with <see cref="Scan"/> and the read helpers, and find them again whenever
/// <see cref="AttachId"/> changes (a new process, or the same one after a reconnect). Everything that touches the
/// game is done while holding <see cref="Lock"/>.
/// </summary>
public class GameLink : IGameProcess
{
    public enum LinkState { NotRunning, Attached, Failed }

    private readonly ProjectEntry Project;
    private GameLinkOffsets OffsetsCache;

    public GameLink(ProjectEntry project)
    {
        Project = project;
    }

    private GameLinkOffsets Offsets => OffsetsCache ??= GameLinkOffsets.Load(Project);

    /// <summary>Whether this game has Game Link offsets</summary>
    public bool IsSupported => !string.IsNullOrEmpty(Offsets.ExeName);

    /// <summary>The executable the link attaches to (eldenring.exe)</summary>
    public string ExeName => Offsets.ExeName;

    /// <summary>The attached executable's file version ("2.7.2.0"), for tools picking their own offsets</summary>
    public string ExeVersion { get; private set; } = "";

    /// <summary>Goes up on every attach; a tool whose addresses were found under another value finds them again</summary>
    public int AttachId { get; private set; }

    /// <summary>Whether this game's offsets offer any world action (reload world, quit out), attached or not</summary>
    public bool OffersWorldActions => OffersWorldReload || OffersQuitOut;

    /// <summary>Whether this game's offsets offer reloading the world, attached or not</summary>
    public bool OffersWorldReload => Offsets.Groups.Any(g => !string.IsNullOrWhiteSpace(g.worldReloadMethod));

    /// <summary>Whether this game's offsets offer quitting to the title screen, attached or not</summary>
    public bool OffersQuitOut => Offsets.Groups.Any(g => !string.IsNullOrWhiteSpace(g.quitOutAob));

    /// <summary>
    /// Whether Reload World saves first (ER, NR), or loads the last save as it is (AC6): what was found when attached,
    /// else what the offsets say
    /// </summary>
    public bool WorldReloadSavesFirst => CanReloadWorld
        ? WorldReload.SavesFirst
        : Offsets.Groups.Any(g => !string.IsNullOrWhiteSpace(g.worldReloadMethod) && !string.IsNullOrWhiteSpace(g.worldSaveRequestAob));

    // Updated by background tasks, read by the UI
    public volatile LinkState State = LinkState.NotRunning;
    public volatile string StateMessage = "";

    private readonly object GameLock = new();
    private Process GameProcess;
    private nint Handle;

    #region Busy

    private int BusyCount;

    /// <summary>
    /// Whether any user of the link is changing the game (a tool's send, a world reload, a quit out)
    /// </summary>
    public bool IsBusy => Volatile.Read(ref BusyCount) > 0 || IsReloadingWorld || IsQuittingOut;

    /// <summary>
    /// Marks the link busy until the result is disposed, so other tools wait (e.g. a material send)
    /// </summary>
    public IDisposable BeginOperation()
    {
        Interlocked.Increment(ref BusyCount);
        return new Operation(this);
    }

    private sealed class Operation(GameLink link) : IDisposable
    {
        private int Done;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref Done, 1) == 0)
                Interlocked.Decrement(ref link.BusyCount);
        }
    }

    #endregion

    #region Attach

    /// <summary>
    /// Connects to the game if it isn't already, and finds the world actions. Call with <see cref="Lock"/> held.
    /// False with <see cref="StateMessage"/> set when the game isn't running or can't be opened.
    /// </summary>
    public bool EnsureAttached()
    {
        if (GameProcess != null && !GameProcess.HasExited && Handle != 0)
            return true;

        Close();

        if (!IsSupported)
        {
            State = LinkState.Failed;
            StateMessage = LOC.Get("GAMELINK_State_Not_Supported");
            return false;
        }

        var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ExeName));
        if (processes.Length == 0)
        {
            State = LinkState.NotRunning;
            StateMessage = LOC.Get("GAMELINK_State_Not_Running", ExeName);
            return false;
        }

        GameProcess = processes[0];
        Handle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION,
            false, GameProcess.Id);

        if (Handle == 0)
        {
            Fail(LOC.Get("GAMELINK_State_Open_Failed", Marshal.GetLastWin32Error()));
            return false;
        }

        try
        {
            ExeVersion = GameProcess.MainModule?.FileVersionInfo.FileVersion ?? "";
        }
        catch (Exception)
        {
            ExeVersion = "";
        }

        AttachId++;
        State = LinkState.Attached;
        StateMessage = LOC.Get("GAMELINK_State_Attached", ExeName, GameProcess.Id);

        Smithbox.Log(typeof(GameLink), $"[Game Link] Attached to {ExeName} {ExeVersion}, pid {GameProcess.Id}.");

        ResolveQuitOut();
        ResolveWorldReload();

        return true;
    }

    /// <summary>
    /// Closes the connection; the next use connects again
    /// </summary>
    public void Detach()
    {
        lock (GameLock)
        {
            Close();
            State = LinkState.NotRunning;
            StateMessage = "";
        }
    }

    /// <summary>
    /// Marks the link failed (e.g. reading the game threw) and closes it. Call with <see cref="Lock"/> held.
    /// </summary>
    public void Fail(string message)
    {
        Close();
        State = LinkState.Failed;
        StateMessage = message;
    }

    private void Close()
    {
        if (Handle != 0)
            CloseHandle(Handle);

        Handle = 0;
        GameProcess?.Dispose();
        GameProcess = null;
        ExeVersion = "";
        MenuManGlobal = 0;
        WorldReload.Clear();
    }

    #endregion

    #region Quit Out

    // Quit to title (ER): the game's own request (0x1407A4BD0) sets byte [[CSMenuManImp]+8]+5D = 1; MoveMapStep reads
    // it, leaves the world and the byte is cleared. Loading back in rebuilds every model.
    private nint MenuManGlobal;
    private int QuitOutMenuOffset, QuitOutFlagOffset;

    public volatile string LastQuitOut;
    public volatile bool IsQuittingOut;

    /// <summary>Whether the quit-to-title request was found in the attached game</summary>
    public bool CanQuitOut => MenuManGlobal != 0;

    private void ResolveQuitOut()
    {
        MenuManGlobal = 0;

        foreach (var g in Offsets.GroupsFor(ExeVersion).Where(g => !string.IsNullOrWhiteSpace(g.quitOutAob)))
        {
            if (ResolveQuitOut(g, out var problem))
            {
                Smithbox.Log(typeof(GameLink), $"[Game Link] Quit out ({g.exeVersion} offsets): CSMenuManImp global 0x{MenuManGlobal:X}");
                return;
            }

            Smithbox.Log(typeof(GameLink), $"[Game Link] Quit out unavailable with the {g.exeVersion} offsets: {problem}.");
        }
    }

    private bool ResolveQuitOut(GameLinkOffsetEntry g, out string problem)
    {
        problem = "";

        var aobOffset = GameLinkOffsets.ParseHex(g.quitOutAobOffset);
        var globalRel = GameLinkOffsets.ParseRelative(g.quitOutMenuManRelativeOffset);
        var menuOffset = GameLinkOffsets.ParseHex(g.quitOutMenuOffset);
        var flagOffset = GameLinkOffsets.ParseHex(g.quitOutFlagOffset);

        if (aobOffset == null || globalRel == null || menuOffset == null || flagOffset == null)
        {
            problem = "incomplete offsets";
            return false;
        }

        var tails = Scan(g.quitOutAob);
        if (tails.Count != 1)
        {
            problem = $"{tails.Count} match(es)";
            return false;
        }

        var start = tails[0] - aobOffset.Value;
        if (!string.IsNullOrWhiteSpace(g.quitOutPrologue) && !Matches(start, g.quitOutPrologue))
        {
            problem = "unexpected function start";
            return false;
        }

        MenuManGlobal = Relative(start, globalRel.Value);
        QuitOutMenuOffset = menuOffset.Value;
        QuitOutFlagOffset = flagOffset.Value;
        return true;
    }

    /// <summary>
    /// Asks the game to quit to the title screen, on a background task
    /// </summary>
    public void RequestQuitOut()
    {
        if (IsBusy)
            return;

        IsQuittingOut = true;

        Task.Run(() =>
        {
            try
            {
                lock (GameLock)
                {
                    if (!EnsureAttached())
                    {
                        LastQuitOut = StateMessage;
                        return;
                    }
                }

                QuitOut();
            }
            catch (Exception e)
            {
                LastQuitOut = LOC.Get("GAMELINK_QuitOut_Failed", e.Message);
                Smithbox.LogError(typeof(GameLink), "[Game Link] Quit out failed.", e);
            }
            finally
            {
                IsQuittingOut = false;
            }
        });
    }

    /// <summary>
    /// Sets the quit-to-title request and checks the game took it. Outside the world nothing reads the byte, and a
    /// leftover 1 would end the next session as soon as it loads, so an unread request is withdrawn.
    /// Blocks for up to 5 s, so call it from a background task, without <see cref="Lock"/> held.
    /// </summary>
    public void QuitOut()
    {
        IsQuittingOut = true;

        try
        {
            nint flag;

            lock (GameLock)
            {
                if (!CanQuitOut)
                {
                    LastQuitOut = LOC.Get("GAMELINK_QuitOut_Unavailable");
                    return;
                }

                var menuMan = ReadPtr(MenuManGlobal);
                var menu = menuMan != 0 ? ReadPtr(menuMan + QuitOutMenuOffset) : 0;

                if (menu == 0)
                {
                    LastQuitOut = LOC.Get("GAMELINK_QuitOut_Not_In_World");
                    return;
                }

                flag = menu + QuitOutFlagOffset;
                WriteBytes(flag, new byte[] { 1 });
            }

            Smithbox.Log(typeof(GameLink), $"[Game Link] Quit out requested (flag 0x{flag:X}).");

            // The world step reads the request within a frame or two
            for (var i = 0; i < 20; i++)
            {
                Thread.Sleep(250);

                lock (GameLock)
                {
                    if (Handle == 0)
                        return;

                    if (ReadBytes(flag, 1)[0] == 0)
                    {
                        LastQuitOut = LOC.Get("GAMELINK_QuitOut_Done", DateTime.Now.ToString("T"));
                        return;
                    }
                }
            }

            lock (GameLock)
            {
                if (Handle != 0)
                    WriteBytes(flag, new byte[] { 0 });
            }

            LastQuitOut = LOC.Get("GAMELINK_QuitOut_Not_Taken");
            Smithbox.Log(typeof(GameLink), "[Game Link] Quit out not taken within 5 s; request withdrawn.");
        }
        finally
        {
            IsQuittingOut = false;
        }
    }

    #endregion

    #region Reload World

    private readonly WorldReload WorldReload = new();

    /// <summary>Whether the world reload request was found in the attached game</summary>
    public bool CanReloadWorld => WorldReload.CanReload;

    public string LastWorldReload
    {
        get => WorldReload.LastMessage;
        private set => WorldReload.LastMessage = value;
    }

    public bool IsReloadingWorld
    {
        get => WorldReload.IsRunning;
        private set => WorldReload.IsRunning = value;
    }

    private void ResolveWorldReload()
    {
        WorldReload.Clear();

        foreach (var g in Offsets.GroupsFor(ExeVersion).Where(g => !string.IsNullOrWhiteSpace(g.worldReloadMethod)))
        {
            if (!string.Equals(g.worldReloadMethod, WorldReload.Method, StringComparison.OrdinalIgnoreCase))
            {
                Smithbox.Log(typeof(GameLink), $"[Game Link] Reload world: unknown method {g.worldReloadMethod} in the {g.exeVersion} offsets.");
                continue;
            }

            if (WorldReload.Resolve(this, g, out var problem))
                return;

            Smithbox.Log(typeof(GameLink), $"[Game Link] Reload world unavailable with the {g.exeVersion} offsets: {problem}.");
        }
    }

    /// <summary>
    /// Asks the game to save and load the current save again, on a background task
    /// </summary>
    public void RequestReloadWorld()
    {
        if (IsBusy)
            return;

        IsReloadingWorld = true;

        Task.Run(() =>
        {
            try
            {
                lock (GameLock)
                {
                    if (!EnsureAttached())
                    {
                        LastWorldReload = StateMessage;
                        return;
                    }
                }

                ReloadWorld();
            }
            catch (Exception e)
            {
                LastWorldReload = LOC.Get("GAMELINK_World_Failed", e.Message);
                Smithbox.LogError(typeof(GameLink), "[Game Link] Reload world failed.", e);
            }
            finally
            {
                IsReloadingWorld = false;
            }
        });
    }

    /// <summary>
    /// Saves (where the game can), then asks the world step to load the save slot again (Memory/WorldReload).
    /// Blocks for up to 20 s, so call it from a background task, without <see cref="Lock"/> held.
    /// </summary>
    public void ReloadWorld() => WorldReload.Run(this);

    #endregion

    #region Memory

    // IGameProcess: what shared memory code (Reload World) reaches the game through
    public object Lock => GameLock;
    public bool IsOpen => Handle != 0;

    /// <summary>
    /// Every match of an AOB ("48 8B ?? ...") in the executable sections of the main module
    /// </summary>
    public List<nint> Scan(string aob) => ScanWith(aob).Select(h => h.At).ToList();

    /// <summary>
    /// Every match of an AOB in the executable sections of the main module, with the matched bytes
    /// (for checks on the wildcard bytes, or reading rel32s without another read)
    /// </summary>
    public List<(nint At, byte[] Bytes)> ScanWith(string aob)
    {
        var pattern = ParsePattern(aob);
        var hits = new List<(nint, byte[])>();

        foreach (var section in Sections().Where(s => s.Executable))
        {
            ForEachChunk(section, pattern.Length, (start, bytes, length) =>
            {
                for (var i = 0; i <= length - pattern.Length && i < Chunk; i++)
                {
                    var match = true;
                    for (var j = 0; j < pattern.Length; j++)
                    {
                        if (pattern[j] >= 0 && bytes[i + j] != pattern[j])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        hits.Add((start + i, bytes.AsSpan(i, pattern.Length).ToArray()));
                }
            });
        }

        return hits;
    }

    /// <summary>
    /// Every start of these bytes in the module's data (non-executable) sections that follows a 0 byte, so a whole
    /// C string ("MsgRepository\0")
    /// </summary>
    public List<nint> FindInData(byte[] needle)
    {
        var hits = new List<nint>();

        foreach (var section in Sections().Where(s => !s.Executable))
        {
            ForEachChunk(section, needle.Length, (start, bytes, length) =>
            {
                for (var i = 1; i <= length - needle.Length && i < Chunk; i++)
                {
                    if (bytes[i - 1] == 0 && bytes.AsSpan(i, needle.Length).SequenceEqual(needle))
                        hits.Add(start + i);
                }
            });
        }

        return hits;
    }

    /// <summary>
    /// The first match of an AOB in bytes[from, from + length), or -1. For searching code already read.
    /// </summary>
    public static int IndexOf(byte[] bytes, string aob, int from, int length)
    {
        var pattern = ParsePattern(aob);

        for (var i = from; i < from + length && i + pattern.Length <= bytes.Length; i++)
        {
            var match = true;
            for (var j = 0; j < pattern.Length && match; j++)
                match = pattern[j] < 0 || bytes[i + j] == pattern[j];

            if (match)
                return i;
        }

        return -1;
    }

    private record Section(nint Start, int Size, bool Executable);

    private const int Chunk = 0x100000;

    /// <summary>The main module's sections, from its PE header</summary>
    private List<Section> Sections()
    {
        var module = GameProcess.MainModule.BaseAddress;
        var header = ReadBytes(module, 0x1000);
        var peOff = BitConverter.ToInt32(header, 0x3C);
        var sectionCount = BitConverter.ToUInt16(header, peOff + 6);
        var optSize = BitConverter.ToUInt16(header, peOff + 20);
        var sec = peOff + 24 + optSize;
        var list = new List<Section>();

        for (var s = 0; s < sectionCount; s++, sec += 40)
        {
            var virtualSize = BitConverter.ToInt32(header, sec + 8);
            var rva = BitConverter.ToInt32(header, sec + 12);
            var characteristics = BitConverter.ToUInt32(header, sec + 36);
            list.Add(new Section(module + rva, virtualSize, (characteristics & 0x20000000) != 0)); // IMAGE_SCN_MEM_EXECUTE
        }

        return list;
    }

    /// <summary>
    /// Reads a section in 1 MB chunks, each with enough overlap for a match of the given size across the boundary.
    /// The callback gets the chunk's address, its bytes and how many were read; matches should start below Chunk.
    /// </summary>
    private void ForEachChunk(Section section, int overlap, Action<nint, byte[], int> visit)
    {
        for (var offset = 0; offset < section.Size; offset += Chunk)
        {
            var length = Math.Min(Chunk + overlap, section.Size - offset);
            var start = section.Start + offset;
            var bytes = new byte[length];

            if (ReadProcessMemory(Handle, start, bytes, length, out _))
                visit(start, bytes, length);
        }
    }

    /// <summary>The game exe's load address and size in memory (0 when not attached)</summary>
    public nint ModuleBase => GameProcess?.MainModule?.BaseAddress ?? 0;
    public int ModuleSize => GameProcess?.MainModule?.ModuleMemorySize ?? 0;

    /// <summary>Size of the pieces <see cref="ForEachHeapChunk"/> hands out (each may carry extra overlap bytes)</summary>
    public const int HeapChunk = 0x400000;

    /// <summary>
    /// Reads the game's committed, writable private memory (the heaps, not the exe image) in 4 MB pieces. The callback
    /// gets each piece's address, a buffer and how many bytes were read; each piece carries `overlap` extra bytes so a
    /// match can cross into the next one (matches should start below <see cref="HeapChunk"/>). Reads the whole heap, so
    /// it takes a moment on a large game: call it on demand, not every frame. Returns the bytes read.
    /// </summary>
    public long ForEachHeapChunk(int overlap, Action<nint, byte[], int> visit)
    {
        long scanned = 0;
        var buffer = new byte[HeapChunk + overlap];
        var size = Marshal.SizeOf<MemoryBasicInformation>();
        nint address = 0;

        while (VirtualQueryEx(Handle, address, out var info, size) == size)
        {
            var regionEnd = info.BaseAddress + (nint)info.RegionSize;

            if (info.State == MEM_COMMIT && info.Type == MEM_PRIVATE && (info.Protect & PAGE_GUARD) == 0 &&
                (info.Protect & (PAGE_READWRITE | PAGE_EXECUTE_READWRITE)) != 0)
            {
                for (var start = info.BaseAddress; start < regionEnd; start += HeapChunk)
                {
                    var length = (int)Math.Min(HeapChunk + overlap, (long)(regionEnd - start));

                    if (!ReadProcessMemory(Handle, start, buffer, length, out var read) || read == 0)
                        continue;

                    scanned += Math.Min(read, HeapChunk);
                    visit(start, buffer, (int)read);
                }
            }

            if (regionEnd <= address)
                break;

            address = regionEnd;
        }

        return scanned;
    }

    /// <summary>Every address in the game's heaps where these bytes start (see <see cref="ForEachHeapChunk"/>)</summary>
    public List<nint> FindInHeap(byte[] prefix, out long scannedBytes)
    {
        var hits = new List<nint>();

        scannedBytes = ForEachHeapChunk(prefix.Length - 1, (start, buffer, read) =>
        {
            var span = buffer.AsSpan(0, read);

            // Matches must start inside this chunk; the overlap only completes them
            for (var from = 0; ;)
            {
                var at = span[from..].IndexOf(prefix);
                if (at < 0 || from + at >= HeapChunk)
                    break;

                hits.Add(start + from + at);
                from += at + 1;
            }
        });

        return hits;
    }

    public byte[] ReadBytes(nint address, int length)
    {
        var buffer = new byte[length];
        if (!ReadProcessMemory(Handle, address, buffer, length, out _))
            throw new InvalidOperationException($"Could not read {length} bytes at 0x{address:X}");
        return buffer;
    }

    public void WriteBytes(nint address, byte[] bytes)
    {
        if (!WriteProcessMemory(Handle, address, bytes, bytes.Length, out _))
            throw new InvalidOperationException($"Could not write {bytes.Length} bytes at 0x{address:X}");
    }

    public int ReadI32(nint address) => BitConverter.ToInt32(ReadBytes(address, 4), 0);

    public nint ReadPtr(nint address) => (nint)BitConverter.ToInt64(ReadBytes(address, 8), 0);

    /// <summary>A UTF-16 string of at most maxBytes, up to its terminator</summary>
    public string ReadUnicode(nint address, int maxBytes)
    {
        var s = Encoding.Unicode.GetString(ReadBytes(address, maxBytes));
        var end = s.IndexOf('\0');
        return end >= 0 ? s[..end] : s;
    }

    /// <summary>The address a rel32 points at: at site + start, relative to site + end</summary>
    public nint Relative(nint site, (int Start, int End) rel) => Relative(this, site, rel);

    /// <summary>Whether the bytes at an address match an AOB ("??" = any byte)</summary>
    public bool Matches(nint address, string aob) => Matches(this, address, aob);

    public static nint Relative(IGameProcess game, nint site, (int Start, int End) rel) =>
        site + rel.End + BitConverter.ToInt32(game.ReadBytes(site + rel.Start, 4), 0);

    public static bool Matches(IGameProcess game, nint address, string aob)
    {
        var pattern = ParsePattern(aob);
        var bytes = game.ReadBytes(address, pattern.Length);

        for (var i = 0; i < pattern.Length; i++)
        {
            if (pattern[i] >= 0 && bytes[i] != pattern[i])
                return false;
        }

        return true;
    }

    public static int[] ParsePattern(string aob) =>
        aob.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(t => t.StartsWith('?') ? -1 : Convert.ToInt32(t, 16)).ToArray();

    /// <summary>Memory in the game: read-write, or executable for thread code. 0 on failure.</summary>
    public nint Alloc(int size, bool executable = false) =>
        VirtualAllocEx(Handle, 0, size, MEM_COMMIT | MEM_RESERVE, executable ? PAGE_EXECUTE_READWRITE : PAGE_READWRITE);

    /// <summary>Frees memory from <see cref="Alloc"/>; only once no game thread can still use it</summary>
    public void Free(nint address)
    {
        if (address != 0 && Handle != 0)
            VirtualFreeEx(Handle, address, 0, MEM_RELEASE);
    }

    public enum ThreadOutcome { Finished, TimedOut, NotStarted }

    /// <summary>
    /// Runs code in the game on a new thread and waits for it. Exit code is the thread's (its eax);
    /// error is the Win32 error when it couldn't start. After a time-out the thread may still be running, so its
    /// memory must not be freed.
    /// </summary>
    public ThreadOutcome RunThread(nint start, uint timeoutMs, out uint exitCode, out int error)
    {
        exitCode = 0;
        error = 0;

        var thread = CreateRemoteThread(Handle, 0, 0, start, 0, 0, out _);
        if (thread == 0)
        {
            error = Marshal.GetLastWin32Error();
            return ThreadOutcome.NotStarted;
        }

        try
        {
            if (WaitForSingleObject(thread, timeoutMs) != 0)
                return ThreadOutcome.TimedOut;

            GetExitCodeThread(thread, out exitCode);
            return ThreadOutcome.Finished;
        }
        finally
        {
            CloseHandle(thread);
        }
    }

    private const int PROCESS_CREATE_THREAD = 0x2, PROCESS_VM_OPERATION = 0x8, PROCESS_VM_READ = 0x10,
        PROCESS_VM_WRITE = 0x20, PROCESS_QUERY_INFORMATION = 0x400;
    private const uint MEM_COMMIT = 0x1000, MEM_RESERVE = 0x2000, MEM_RELEASE = 0x8000, MEM_PRIVATE = 0x20000;
    private const uint PAGE_READWRITE = 0x04, PAGE_EXECUTE_READWRITE = 0x40, PAGE_GUARD = 0x100;

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryBasicInformation
    {
        public nint BaseAddress;
        public nint AllocationBase;
        public uint AllocationProtect;
        public ushort PartitionId;
        public nint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int VirtualQueryEx(nint process, nint address, out MemoryBasicInformation info, int length);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint OpenProcess(int access, bool inherit, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(nint handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(nint process, nint address, byte[] buffer, nint size, out nint read);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(nint process, nint address, byte[] buffer, nint size, out nint written);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint VirtualAllocEx(nint process, nint address, nint size, uint type, uint protect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualFreeEx(nint process, nint address, nint size, uint type);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint CreateRemoteThread(nint process, nint attributes, nint stackSize, nint start, nint parameter, uint flags, out uint threadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(nint handle, uint milliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetExitCodeThread(nint thread, out uint exitCode);

    #endregion
}
