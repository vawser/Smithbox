using StudioCore.Application;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// The link to a running eldenring.exe used by the FMG Reloader: reads which FMGs the game holds, and replaces one with
/// the editor's copy. See Claude/Documentation/FMG Live Reload Analysis.md.
///
/// How the game holds text (addresses are the 1.16 Steam build, found by AOB at runtime; checked by
/// Claude/Tools/FmgReloadAob):
///   MsgRepositoryImp (singleton) +08 version array (one version), +10 version count, +14 slot count (0x200),
///     +30 allocator (vfn +50 = alloc(size, align))
///   [[repo+8]][id] = the FMG of binder entry id: the file's bytes, copied into the game heap, with the string offset
///     table offset at +18 turned into a pointer. Lookups binary-search it in place and return pointers into it.
///
/// A reload allocates a new block from the game's allocator, copies the FMG in, fixes +18 and swaps the slot pointer in
/// one store, on a thread in the game. The old block is kept (not freed): text the game already fetched may still point
/// into it.
/// </summary>
public class TextGameLink
{
    public enum LinkState { NotRunning, Attached, Failed }

    public enum ReloadOutcome { Reloaded, NotFound, Failed }

    public record ReloadResult(string Label, ReloadOutcome Outcome, string Message, DateTime At);

    /// <summary>An FMG the game holds: its binder entry ID, string count and whether this session replaced it</summary>
    public record GameFmg(int Id, int StringCount, bool Replaced);

    /// <summary>An FMG to send: binder entry ID, a label for the history, its bytes, and where it came from</summary>
    public record SendItem(int Id, string Label, byte[] Bytes, string ContainerPath);

    /// <summary>Where a send came from, shown in the history</summary>
    public enum SendSource { Selected, Container, Modified, Save, History }

    // Updated by the background tasks, read by the UI
    public volatile LinkState State = LinkState.NotRunning;
    public volatile string StateMessage = "";
    public volatile string Language = "";
    public volatile IReadOnlyList<GameFmg> Fmgs = Array.Empty<GameFmg>();
    public volatile bool IsPolling;
    public volatile bool IsReloading;
    public volatile ReloadResult LastReload;
    public DateTime? LastPoll { get; private set; }

    /// <summary>Bytes of replaced FMG blocks left in the game this session</summary>
    public long KeptBytes => Interlocked.Read(ref keptBytes);
    private long keptBytes;

    /// <summary>The binders the game loads; the others (item, menu, _dlc01) are never read</summary>
    public static readonly string[] ResidentContainers = { "item_dlc02", "menu_dlc02", "ngword", "sellregion" };

    private const string ProcessName = "eldenring";

    private readonly object GameLock = new();
    private Process GameProcess;
    private nint Handle;
    private nint RepoGlobal, LanguageIndexAddr, LanguageTableAddr;
    private int LanguageCount;

    // Slot pointers this session wrote, so the list can mark them
    private readonly Dictionary<int, nint> Replaced = new();

    // MsgbndFileCap load: mov rcx,[MsgRepository]; mov [rsp+20h],r12; mov r9,[rsp+30h]; mov r8,r15; mov edx,r14d;
    // call Insert; test al,al
    private const string RepoInsertSiteAob = "48 8B 0D ?? ?? ?? ?? 4C 89 64 24 20 4C 8B 4C 24 30 4D 8B C7 41 8B D6 E8 ?? ?? ?? ?? 84 C0";
    // lea r9,["MsgRepository"] of the singleton check just before the site
    private const int RepoNameLeaBefore = 0x1F;
    // Insert wrapper (repo, id, data, size, name) calls the FD4 insert at +1B
    private const int WrapperCallOffset = 0x1B;
    // FD4 insert: the string offset fix-up at +137, and the layout the swap relies on
    private const int InsertFixupOffset = 0x137;
    private static readonly byte[] InsertFixup = { 0x48, 0x8B, 0x47, 0x18, 0x48, 0x85, 0xC0, 0x74, 0x07, 0x48, 0x03, 0xC7, 0x48, 0x89, 0x47, 0x18 };
    private static readonly byte[] InsertPrologue = { 0x48, 0x89, 0x5C, 0x24, 0x18, 0x55, 0x56, 0x41, 0x54 };
    private static readonly (int Offset, byte[] Bytes)[] InsertChecks =
    {
        (0x18, new byte[] { 0x3B, 0x51, 0x10 }),                // cmp edx,[rcx+10h]: version count
        (0x21, new byte[] { 0x3B, 0x69, 0x14 }),                // cmp ebp,[rcx+14h]: slot count
        (0x3F, new byte[] { 0x48, 0x8B, 0x41, 0x08 }),          // mov rax,[rcx+8]: version array
        (0xF2, new byte[] { 0x48, 0x8B, 0x4B, 0x30, 0x41, 0xB8, 0x10, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x54, 0x24, 0x60, 0x48, 0x8B, 0x01, 0xFF, 0x50, 0x50 }),
                                                                 // allocator [rcx+30h], alloc = vfn +50h (size, 10h)
    };
    private const int VersionArrayOffset = 0x8, VersionCountOffset = 0x10, SlotCountOffset = 0x14, AllocatorOffset = 0x30;

    // mov ecx,[langIndex]; call GetLanguageFolder; mov r8,rax; lea rdx,[path format] (the item_dlc02 one is used)
    private const string LanguageSiteAob = "8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C 8B C0 48 8D 15 ?? ?? ?? ??";
    private const string LanguagePath = "msg:/%s/item_dlc02.msgbnd";

    /// <summary>
    /// Starts a background read of the game's language and FMG table, unless one is running
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
                lock (GameLock)
                {
                    if (EnsureAttached())
                        Poll();
                }
            }
            catch (Exception e)
            {
                Fail(LOC.Get("TEXT_FmgReloader_State_Read_Failed", e.Message));
            }
            finally
            {
                LastPoll = DateTime.Now;
                IsPolling = false;
            }
        });
    }

    /// <summary>
    /// Replaces FMGs in the running game, on a background task. Each one goes into the history.
    /// </summary>
    public void RequestReloadMany(IReadOnlyList<SendItem> items, SendSource source)
    {
        if (items.Count == 0)
            return;

        if (IsReloading)
        {
            Smithbox.LogError(typeof(TextGameLink), LOC.Get("TEXT_FmgReloader_Reload_Busy", items.Count));
            return;
        }

        IsReloading = true;
        var label = items.Count == 1 ? items[0].Label : LOC.Get("TEXT_FmgReloader_Reload_Many_Name", items.Count);

        Task.Run(() =>
        {
            try
            {
                var results = new List<ReloadResult>();

                lock (GameLock)
                {
                    if (!EnsureAttached())
                    {
                        LastReload = new(label, ReloadOutcome.Failed, StateMessage, DateTime.Now);
                        return;
                    }

                    foreach (var item in items)
                    {
                        var result = Reload(item);
                        results.Add(result);
                        AddHistory(item, result.Outcome, source);
                    }

                    Poll();
                }

                LastReload = results.Count == 1 ? results[0] : Summarise(label, results);
            }
            catch (Exception e)
            {
                LastReload = new(label, ReloadOutcome.Failed, e.Message, DateTime.Now);
                Smithbox.LogError(typeof(TextGameLink), $"[Text Editor] In-game reload of {label} failed.", e);
            }
            finally
            {
                IsReloading = false;
            }
        });
    }

    private static ReloadResult Summarise(string label, List<ReloadResult> results)
    {
        var done = results.Count(r => r.Outcome is ReloadOutcome.Reloaded);
        var missing = results.Where(r => r.Outcome is ReloadOutcome.NotFound).Select(r => r.Label).ToList();
        var failed = results.Count(r => r.Outcome is ReloadOutcome.Failed);

        var message = LOC.Get("TEXT_FmgReloader_Reload_Many_Done", done, results.Count);
        if (missing.Count > 0)
            message += " " + LOC.Get("TEXT_FmgReloader_Reload_Many_Not_Found", string.Join(", ", missing.Take(5)) + (missing.Count > 5 ? ", ..." : ""));
        if (failed > 0)
            message += " " + LOC.Get("TEXT_FmgReloader_Reload_Many_Failed", failed);

        var outcome = done == results.Count ? ReloadOutcome.Reloaded : ReloadOutcome.Failed;
        return new ReloadResult(label, outcome, message, DateTime.Now);
    }

    /// <summary>
    /// Whether the game reads this container: one of the binders it loads, in the language it runs in
    /// </summary>
    public static bool IsResident(TextContainerWrapper container, string gameLanguage)
    {
        if (container?.FileEntry == null || string.IsNullOrEmpty(gameLanguage))
            return false;

        return IsResidentName(container) && string.Equals(LanguageOf(container), gameLanguage, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsResidentName(TextContainerWrapper container) =>
        container?.FileEntry != null && ResidentContainers.Contains(container.FileEntry.Filename, StringComparer.OrdinalIgnoreCase);

    /// <summary>The language folder a container sits in: /msg/engus/item_dlc02.msgbnd.dcx → engus</summary>
    public static string LanguageOf(TextContainerWrapper container)
    {
        var folder = (container?.FileEntry?.Folder ?? "").Replace('\\', '/').TrimEnd('/');
        var cut = folder.LastIndexOf('/');
        return cut >= 0 ? folder[(cut + 1)..] : folder;
    }

    #region History

    /// <summary>One FMG sent this session: the latest send of each is kept, newest first</summary>
    public record HistoryEntry(string ContainerPath, int Id, string Label, DateTime At, ReloadOutcome Outcome, SendSource Source);

    private const int HistoryLimit = 50;
    private readonly object HistoryLock = new();
    private List<HistoryEntry> HistoryList = new();

    public volatile IReadOnlyList<HistoryEntry> History = Array.Empty<HistoryEntry>();

    private void AddHistory(SendItem item, ReloadOutcome outcome, SendSource source)
    {
        lock (HistoryLock)
        {
            var list = HistoryList.Where(h => !(h.Id == item.Id && string.Equals(h.ContainerPath, item.ContainerPath, StringComparison.OrdinalIgnoreCase))).ToList();
            list.Insert(0, new HistoryEntry(item.ContainerPath, item.Id, item.Label, DateTime.Now, outcome, source));

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

    public void Detach()
    {
        lock (GameLock)
        {
            Close();
            State = LinkState.NotRunning;
            StateMessage = "";
        }
    }

    #region Attach

    private bool EnsureAttached()
    {
        if (GameProcess != null && !GameProcess.HasExited && Handle != 0 && RepoGlobal != 0)
            return true;

        Close();

        var processes = Process.GetProcessesByName(ProcessName);
        if (processes.Length == 0)
        {
            State = LinkState.NotRunning;
            StateMessage = LOC.Get("TEXT_FmgReloader_State_Not_Running");
            return false;
        }

        GameProcess = processes[0];
        Handle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION,
            false, GameProcess.Id);

        if (Handle == 0)
        {
            Fail(LOC.Get("TEXT_FmgReloader_State_Open_Failed", Marshal.GetLastWin32Error()));
            return false;
        }

        if (!Resolve(out var error))
        {
            Fail(error);
            return false;
        }

        State = LinkState.Attached;
        StateMessage = LOC.Get("TEXT_FmgReloader_State_Attached", GameProcess.Id);

        Smithbox.Log(typeof(TextGameLink),
            $"[Text Editor] FMG Reloader link: pid {GameProcess.Id}, MsgRepository global 0x{RepoGlobal:X}, language index 0x{LanguageIndexAddr:X}, table 0x{LanguageTableAddr:X} ({LanguageCount})");

        return true;
    }

    private void Fail(string message)
    {
        State = LinkState.Failed;
        StateMessage = message;
        Close();
    }

    private void Close()
    {
        if (Handle != 0)
            CloseHandle(Handle);

        Handle = 0;
        GameProcess?.Dispose();
        GameProcess = null;
        RepoGlobal = LanguageIndexAddr = LanguageTableAddr = 0;
        LanguageCount = 0;
        Replaced.Clear();
        Fmgs = Array.Empty<GameFmg>();
        Language = "";
    }

    /// <summary>
    /// Finds the repository global and the language index and table in the running exe's code, and checks that the
    /// game's insert still uses the layout the swap relies on
    /// </summary>
    private bool Resolve(out string error)
    {
        error = "";

        // Repository
        var sites = Scan(RepoInsertSiteAob);
        if (sites.Count != 1)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", $"repository site {sites.Count}");
            return false;
        }

        var site = sites[0];
        var nameLea = site - RepoNameLeaBefore;
        var nameLeaBytes = ReadBytes(nameLea, 3);

        if (nameLeaBytes[0] != 0x4C || nameLeaBytes[1] != 0x8D || nameLeaBytes[2] != 0x0D ||
            ReadAscii(nameLea + 7 + ReadI32(nameLea + 3), 32) != "MsgRepository")
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "MsgRepository");
            return false;
        }

        var wrapper = site + 28 + ReadI32(site + 24);
        if (ReadBytes(wrapper + WrapperCallOffset, 1)[0] != 0xE8)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "insert wrapper");
            return false;
        }

        var insert = wrapper + WrapperCallOffset + 5 + ReadI32(wrapper + WrapperCallOffset + 1);

        if (!ReadBytes(insert, InsertPrologue.Length).AsSpan().SequenceEqual(InsertPrologue) ||
            !ReadBytes(insert + InsertFixupOffset, InsertFixup.Length).AsSpan().SequenceEqual(InsertFixup) ||
            InsertChecks.Any(c => !ReadBytes(insert + c.Offset, c.Bytes.Length).AsSpan().SequenceEqual(c.Bytes)))
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "insert layout");
            return false;
        }

        // Language
        nint languageSite = 0;
        foreach (var hit in Scan(LanguageSiteAob))
        {
            if (ReadUnicode(hit + 21 + ReadI32(hit + 17), 80).StartsWith(LanguagePath, StringComparison.Ordinal))
            {
                languageSite = hit;
                break;
            }
        }

        if (languageSite == 0)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "language site");
            return false;
        }

        var getter = languageSite + 11 + ReadI32(languageSite + 7);
        var getterBytes = ReadBytes(getter, 0x30);

        if (getterBytes[0x8] != 0x83 || getterBytes[0x9] != 0xF9 ||
            getterBytes[0x25] != 0x48 || getterBytes[0x26] != 0x8D || getterBytes[0x27] != 0x0D ||
            getterBytes[0x2C] != 0x48 || getterBytes[0x2D] != 0x8B || getterBytes[0x2E] != 0x04 || getterBytes[0x2F] != 0xD9)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "language getter");
            return false;
        }

        LanguageIndexAddr = languageSite + 6 + ReadI32(languageSite + 2);
        LanguageTableAddr = getter + 0x2C + BitConverter.ToInt32(getterBytes, 0x28);
        LanguageCount = getterBytes[0xA];
        RepoGlobal = site + 7 + ReadI32(site + 3);

        return true;
    }

    /// <summary>
    /// Every match of an AOB in the executable sections of the main module
    /// </summary>
    private List<nint> Scan(string aob)
    {
        var pattern = aob.Split(' ').Select(t => t.StartsWith('?') ? -1 : Convert.ToInt32(t, 16)).ToArray();
        var hits = new List<nint>();
        var module = GameProcess.MainModule.BaseAddress;

        var header = ReadBytes(module, 0x1000);
        var peOff = BitConverter.ToInt32(header, 0x3C);
        var sectionCount = BitConverter.ToUInt16(header, peOff + 6);
        var optSize = BitConverter.ToUInt16(header, peOff + 20);
        var sec = peOff + 24 + optSize;

        for (var s = 0; s < sectionCount; s++, sec += 40)
        {
            var virtualSize = BitConverter.ToInt32(header, sec + 8);
            var rva = BitConverter.ToInt32(header, sec + 12);
            var characteristics = BitConverter.ToUInt32(header, sec + 36);

            if ((characteristics & 0x20000000) == 0) // IMAGE_SCN_MEM_EXECUTE
                continue;

            const int chunk = 0x100000;
            for (var offset = 0; offset < virtualSize; offset += chunk)
            {
                var length = Math.Min(chunk + pattern.Length, virtualSize - offset);
                var start = module + rva + offset;
                var bytes = new byte[length];

                if (!ReadProcessMemory(Handle, start, bytes, length, out _))
                    continue;

                for (var i = 0; i <= length - pattern.Length && i < chunk; i++)
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
                        hits.Add(start + i);
                }
            }
        }

        return hits;
    }

    #endregion

    #region Poll

    /// <summary>
    /// The slot array of version 0 and the slot count, or 0 when the game hasn't loaded its text yet
    /// </summary>
    private nint SlotArray(out nint repo, out int slotCount)
    {
        slotCount = 0;
        repo = ReadPtr(RepoGlobal);
        if (repo == 0)
            return 0;

        var versions = ReadPtr(repo + VersionArrayOffset);
        var versionCount = ReadI32(repo + VersionCountOffset);
        slotCount = ReadI32(repo + SlotCountOffset);

        if (versions == 0 || versionCount < 1 || slotCount <= 0 || slotCount > 0x10000)
            return 0;

        return ReadPtr(versions);
    }

    /// <summary>
    /// Reads the game's language and the FMGs it holds
    /// </summary>
    private void Poll()
    {
        var index = ReadI32(LanguageIndexAddr);
        Language = index >= 0 && index < LanguageCount ? ReadUnicode(ReadPtr(LanguageTableAddr + index * 8), 32) : "";

        var slots = SlotArray(out _, out var slotCount);
        if (slots == 0)
        {
            Fmgs = Array.Empty<GameFmg>();
            StateMessage = LOC.Get("TEXT_FmgReloader_State_Not_Loaded");
            return;
        }

        var pointers = ReadBytes(slots, slotCount * 8);
        var fmgs = new List<GameFmg>();

        for (var id = 0; id < slotCount; id++)
        {
            var fmg = (nint)BitConverter.ToInt64(pointers, id * 8);
            if (fmg == 0)
                continue;

            var header = ReadBytes(fmg, 0x14);
            var replaced = Replaced.TryGetValue(id, out var ours) && ours == fmg;
            fmgs.Add(new GameFmg(id, BitConverter.ToInt32(header, 0x10), replaced));
        }

        Fmgs = fmgs;
        StateMessage = LOC.Get("TEXT_FmgReloader_State_Attached", GameProcess.Id);
    }

    #endregion

    #region Reload

    private ReloadResult Reload(SendItem item)
    {
        var bytes = item.Bytes;

        // Version 2 (Elden Ring) little-endian FMG: 64-bit string offsets, offset table offset at +18
        if (bytes.Length < 0x28 || bytes[1] != 0 || bytes[2] != 2)
            return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Bad_Format", item.Label), DateTime.Now);

        var slots = SlotArray(out var repo, out var slotCount);
        if (slots == 0)
            return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_State_Not_Loaded"), DateTime.Now);

        if (item.Id < 0 || item.Id >= slotCount)
            return new(item.Label, ReloadOutcome.NotFound, LOC.Get("TEXT_FmgReloader_Reload_Out_Of_Range", item.Label, item.Id, slotCount), DateTime.Now);

        var slot = slots + item.Id * 8;
        var old = ReadPtr(slot);

        // Only FMGs the game loaded are replaced: an empty slot means nothing reads this ID
        if (old == 0)
            return new(item.Label, ReloadOutcome.NotFound, LOC.Get("TEXT_FmgReloader_Reload_Not_Found", item.Label, item.Id), DateTime.Now);

        var oldHeader = ReadBytes(old, 0x8);
        if (oldHeader[2] != 2)
            return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Bad_Slot", item.Label, item.Id), DateTime.Now);

        var oldSize = BitConverter.ToInt32(oldHeader, 4);

        var allocator = ReadPtr(repo + AllocatorOffset);
        if (allocator == 0)
            return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_No_Allocator"), DateTime.Now);

        var staging = VirtualAllocEx(Handle, 0, bytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        var codeBuffer = VirtualAllocEx(Handle, 0, 0x100, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

        // Freed only once the game's thread is done with them
        var finished = true;

        try
        {
            if (staging == 0 || codeBuffer == 0)
                return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Alloc_Failed"), DateTime.Now);

            WriteBytes(staging, bytes);
            WriteBytes(codeBuffer, BuildSwapCode(allocator, staging, bytes.Length, slot));

            var thread = CreateRemoteThread(Handle, 0, 0, codeBuffer, 0, 0, out _);
            if (thread == 0)
                return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Thread_Failed", Marshal.GetLastWin32Error()), DateTime.Now);

            try
            {
                finished = WaitForSingleObject(thread, 10000) == 0;

                if (!finished)
                    return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Timeout"), DateTime.Now);

                GetExitCodeThread(thread, out var exitCode);

                if (exitCode == 0)
                    return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Game_Alloc_Failed", bytes.Length), DateTime.Now);
            }
            finally
            {
                CloseHandle(thread);
            }

            // Read back: the slot holds a new block that starts with the bytes sent (+18 is now a pointer)
            var current = ReadPtr(slot);
            var check = current != 0 && current != old ? ReadBytes(current, 0x18) : null;

            if (check == null || !check.AsSpan().SequenceEqual(bytes.AsSpan(0, 0x18)))
                return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Check_Failed", item.Label), DateTime.Now);

            Replaced[item.Id] = current;
            Interlocked.Add(ref keptBytes, Math.Max(0, oldSize));

            Smithbox.Log(typeof(TextGameLink), $"[Text Editor] Reloaded FMG {item.Id} ({item.Label}) in game: {bytes.Length} bytes at 0x{current:X}; kept old block 0x{old:X} ({oldSize} bytes).");
            return new(item.Label, ReloadOutcome.Reloaded, LOC.Get("TEXT_FmgReloader_Reload_Done", item.Label), DateTime.Now);
        }
        finally
        {
            if (finished)
            {
                if (staging != 0) VirtualFreeEx(Handle, staging, 0, MEM_RELEASE);
                if (codeBuffer != 0) VirtualFreeEx(Handle, codeBuffer, 0, MEM_RELEASE);
            }
        }
    }

    /// <summary>
    /// The thread body: block = allocator->Alloc(size, 10h); copy the FMG in; turn +18 into a pointer as the game's
    /// insert does; swap the slot in one store. Returns 1, or 0 when the allocation failed (nothing changed).
    /// The old block is not freed. Copied in Claude/Tools/FmgReloadAob (`code` disassembles it).
    /// </summary>
    private static byte[] BuildSwapCode(nint allocator, nint staging, int size, nint slot)
    {
        var code = new List<byte>();
        void B(params byte[] b) => code.AddRange(b);
        void Imm(long value) => code.AddRange(BitConverter.GetBytes(value));

        B(0x56);                                   // push rsi
        B(0x57);                                   // push rdi
        B(0x48, 0x83, 0xEC, 0x28);                 // sub rsp,28h
        B(0x48, 0xB9); Imm(allocator);             // mov rcx,allocator
        B(0x48, 0x8B, 0x01);                       // mov rax,[rcx]
        B(0x48, 0xBA); Imm(size);                  // mov rdx,size
        B(0x41, 0xB8, 0x10, 0x00, 0x00, 0x00);     // mov r8d,10h
        B(0xFF, 0x50, 0x50);                       // call [rax+50h]
        B(0x48, 0x85, 0xC0);                       // test rax,rax
        B(0x74, 0x3D);                             // je fail
        B(0x48, 0x8B, 0xF8);                       // mov rdi,rax
        B(0x48, 0xBE); Imm(staging);               // mov rsi,staging
        B(0x48, 0xB9); Imm(size);                  // mov rcx,size
        B(0xF3, 0xA4);                             // rep movsb
        B(0x48, 0x8B, 0x48, 0x18);                 // mov rcx,[rax+18h]
        B(0x48, 0x85, 0xC9);                       // test rcx,rcx
        B(0x74, 0x07);                             // je skip
        B(0x48, 0x03, 0xC8);                       // add rcx,rax
        B(0x48, 0x89, 0x48, 0x18);                 // mov [rax+18h],rcx
        B(0x48, 0xB9); Imm(slot);                  // skip: mov rcx,slot
        B(0x48, 0x87, 0x01);                       // xchg [rcx],rax
        B(0xB8, 0x01, 0x00, 0x00, 0x00);           // mov eax,1
        B(0xEB, 0x02);                             // jmp done
        B(0x31, 0xC0);                             // fail: xor eax,eax
        B(0x48, 0x83, 0xC4, 0x28);                 // done: add rsp,28h
        B(0x5F);                                   // pop rdi
        B(0x5E);                                   // pop rsi
        B(0xC3);                                   // ret
        return code.ToArray();
    }

    #endregion

    #region Memory

    private byte[] ReadBytes(nint address, int length)
    {
        var buffer = new byte[length];
        if (!ReadProcessMemory(Handle, address, buffer, length, out _))
            throw new InvalidOperationException($"Could not read {length} bytes at 0x{address:X}");
        return buffer;
    }

    private int ReadI32(nint address) => BitConverter.ToInt32(ReadBytes(address, 4), 0);

    private nint ReadPtr(nint address) => (nint)BitConverter.ToInt64(ReadBytes(address, 8), 0);

    private string ReadUnicode(nint address, int maxBytes)
    {
        var s = Encoding.Unicode.GetString(ReadBytes(address, maxBytes));
        var end = s.IndexOf('\0');
        return end >= 0 ? s[..end] : s;
    }

    private string ReadAscii(nint address, int maxBytes)
    {
        var s = Encoding.ASCII.GetString(ReadBytes(address, maxBytes));
        var end = s.IndexOf('\0');
        return end >= 0 ? s[..end] : s;
    }

    private void WriteBytes(nint address, byte[] bytes)
    {
        if (!WriteProcessMemory(Handle, address, bytes, bytes.Length, out _))
            throw new InvalidOperationException($"Could not write {bytes.Length} bytes at 0x{address:X}");
    }

    private const int PROCESS_CREATE_THREAD = 0x2, PROCESS_VM_OPERATION = 0x8, PROCESS_VM_READ = 0x10,
        PROCESS_VM_WRITE = 0x20, PROCESS_QUERY_INFORMATION = 0x400;
    private const uint MEM_COMMIT = 0x1000, MEM_RESERVE = 0x2000, MEM_RELEASE = 0x8000;
    private const uint PAGE_READWRITE = 0x04, PAGE_EXECUTE_READWRITE = 0x40;

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
