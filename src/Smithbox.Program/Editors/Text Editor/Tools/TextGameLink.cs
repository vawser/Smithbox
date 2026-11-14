using StudioCore.Application;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// The link to a running game used by the FMG Reloader (ER, NR, AC6, DS3): reads which FMGs the game holds, and replaces
/// one with the editor's copy. See Claude/Documentation/FMG Live Reload Analysis.md and FMG Reloader Other Games.md.
///
/// How the games hold text (the same FD4 message repository in all of them; found at runtime by game-independent code
/// shapes, checked on every exe by Claude/Tools/FmgReloadAob probe):
///   MsgRepositoryImp (singleton) +08 version array (one version), +10 version count, +14 slot count
///     (ER/DS3 0x200, NR 0x206, AC6 0xD3), allocator at +30 (DS3 +38; read from the insert's code), vfn +50 = alloc
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

    /// <summary>
    /// An FMG to send: binder entry ID, a label for the history, its bytes, where it came from, and its entry ID range
    /// for the identity check (Min > Max when it has no entries: not checked)
    /// </summary>
    public record SendItem(int Id, string Label, byte[] Bytes, string ContainerPath, int MinId, int MaxId);

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

    /// <summary>
    /// What differs per game: the process, and the binders the reloader may send. Each group lists the candidates for
    /// one binder, most complete first. The one the game loaded is told apart by which slots are filled (DS3 loads
    /// item/menu_dlc2 or _dlc1 depending on DLC ownership). ngword and sellregion are left out everywhere: ngword's only
    /// FMG has binder ID 1, the same as menu's first FMG, and it doesn't go through the message repository.
    /// </summary>
    public record GameProfile(ProjectType Type, string ProcessName, string GameName, string[][] Groups)
    {
        public bool IsCandidate(string containerName) =>
            Groups.Any(g => g.Contains(containerName, StringComparer.OrdinalIgnoreCase));
    }

    public static readonly GameProfile[] Profiles =
    {
        new(ProjectType.ER, "eldenring", "Elden Ring", new[] { new[] { "item_dlc02" }, new[] { "menu_dlc02" } }),
        new(ProjectType.NR, "nightreign", "Nightreign", new[] { new[] { "item_dlc01" }, new[] { "menu_dlc01" } }),
        new(ProjectType.AC6, "armoredcore6", "Armored Core VI", new[] { new[] { "item" }, new[] { "menu" } }),
        new(ProjectType.DS3, "DarkSoulsIII", "Dark Souls III", new[] { new[] { "item_dlc2", "item_dlc1" }, new[] { "menu_dlc2", "menu_dlc1" } }),
    };

    public static GameProfile ProfileFor(ProjectType type) => Profiles.FirstOrDefault(p => p.Type == type);

    public readonly GameProfile Profile;

    public TextGameLink(ProjectType type)
    {
        Profile = ProfileFor(type);
    }

    private readonly object GameLock = new();
    private Process GameProcess;
    private nint Handle;
    private nint RepoGlobal, LanguageIndexAddr, LanguageTableAddr;
    private int LanguageCount;      // 0 when the getter has no bounds check (AC6): read until an entry isn't a language code
    private int AllocatorOffset;    // +30, or +38 in DS3: read from the insert's code

    // Slot pointers this session wrote, so the list can mark them
    private readonly Dictionary<int, nint> Replaced = new();

    // The FD4 singleton check: mov reg,[global]; test reg,reg; jne; lea r9,["MsgRepository"]
    private const string SingletonCheckAob = "48 8B ?? ?? ?? ?? ?? 48 85 ?? 75 ?? 4C 8D 0D ?? ?? ?? ??";
    private static readonly byte[] RepositoryName = Encoding.ASCII.GetBytes("MsgRepository\0");

    // Insert wrappers (repo, id, data, size, name) → FD4 insert(repo, version 0, id, data, size, name): int id
    // (ER, NR, DS3) or short id (AC6). Each ends with the call.
    private static readonly string[] InsertWrapperAobs =
    {
        "48 83 EC 38 48 8B 44 24 60 48 89 44 24 28 4C 89 4C 24 20 4D 8B C8 44 8B C2 33 D2 E8",
        "48 83 EC 38 48 8B 44 24 60 4D 8B D0 48 89 44 24 28 4C 89 4C 24 20 4D 8B CA 44 0F BF C2 33 D2 E8",
    };

    // In the FD4 insert: the +18 fix-up (string offset table → pointer)
    private const string InsertFixupAob = "48 8B ?? 18 48 85 C0 74 ?? 48 03 ?? 48 89 ?? 18";

    private const int VersionArrayOffset = 0x8, VersionCountOffset = 0x10, SlotCountOffset = 0x14;

    // mov ecx,[langIndex]; call GetLanguageFolder; mov r8,rax; lea rdx,[msg:/%s/... path]. sellregion's uses a region
    // index instead, so it is skipped.
    private const string LanguageSiteAob = "8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C 8B C0 48 8D 15 ?? ?? ?? ??";

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

    /// <summary>Whether the reloader may send this container at all (one of the profile's candidates)</summary>
    public bool IsCandidate(TextContainerWrapper container) =>
        Profile != null && container?.FileEntry != null && Profile.IsCandidate(container.FileEntry.Filename);

    public bool IsInGameLanguage(TextContainerWrapper container) =>
        !string.IsNullOrEmpty(Language) && string.Equals(LanguageOf(container), Language, StringComparison.OrdinalIgnoreCase);

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

        if (Profile == null)
        {
            State = LinkState.Failed;
            StateMessage = LOC.Get("TEXT_FmgReloader_Game_Not_Supported");
            return false;
        }

        var processes = Process.GetProcessesByName(Profile.ProcessName);
        if (processes.Length == 0)
        {
            State = LinkState.NotRunning;
            StateMessage = LOC.Get("TEXT_FmgReloader_State_Not_Running", Profile.GameName);
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
        StateMessage = LOC.Get("TEXT_FmgReloader_State_Attached", GameProcess.Id, Profile.GameName);

        Smithbox.Log(typeof(TextGameLink),
            $"[Text Editor] FMG Reloader link ({Profile.ProcessName}): pid {GameProcess.Id}, MsgRepository global 0x{RepoGlobal:X}, allocator +0x{AllocatorOffset:X}, " +
            $"language index 0x{LanguageIndexAddr:X}, table 0x{LanguageTableAddr:X} ({(LanguageCount == 0 ? "unbounded" : LanguageCount)})");

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
        AllocatorOffset = 0;
        Replaced.Clear();
        Fmgs = Array.Empty<GameFmg>();
        Language = "";
    }

    /// <summary>
    /// Finds the repository global, the insert's allocator offset and the language index and table in the running exe,
    /// using code shapes shared by ER, NR, AC6 and DS3 (FmgReloadAob probe checks them on each exe)
    /// </summary>
    private bool Resolve(out string error)
    {
        error = "";

        // Repository: the singleton checks whose lea r9 names "MsgRepository" must all read one global
        var names = FindInData(RepositoryName).ToHashSet();
        var globals = new HashSet<nint>();

        foreach (var (at, bytes) in ScanWith(SingletonCheckAob))
        {
            if ((bytes[2] & 0xC7) != 0x05)                       // mov reg,[rip+x]
                continue;

            if (names.Contains(at + 19 + BitConverter.ToInt32(bytes, 15)))
                globals.Add(at + 7 + BitConverter.ToInt32(bytes, 3));
        }

        if (globals.Count != 1)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", $"MsgRepository {globals.Count}");
            return false;
        }

        // Insert: through the wrapper; the allocator offset and the fix-up must be in it
        nint insert = 0;
        foreach (var aob in InsertWrapperAobs)
        {
            var hits = Scan(aob);
            if (hits.Count != 1)
                continue;

            var call = hits[0] + aob.Split(' ').Length - 1;
            insert = call + 5 + ReadI32(call + 1);
            break;
        }

        if (insert == 0)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "insert wrapper");
            return false;
        }

        var code = ReadBytes(insert, 0x400);

        // mov rcx,[reg+30h|38h] (ModRM mod=01, reg=rcx), then call [rax+50h]
        var allocAt = -1;
        for (var i = 0; i + 4 <= 0x300 && allocAt < 0; i++)
        {
            if (code[i] == 0x48 && code[i + 1] == 0x8B && (code[i + 2] & 0xF8) == 0x48 && (code[i + 2] & 7) != 4 && code[i + 3] is 0x30 or 0x38)
                allocAt = i;
        }

        if (allocAt < 0 || IndexOf(code, "FF 50 50", allocAt, 0x40) < 0 || IndexOf(code, InsertFixupAob, 0, code.Length - 16) < 0)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "insert layout");
            return false;
        }

        // Language: the first msgbnd path build that isn't sellregion's
        nint languageSite = 0;
        foreach (var (at, bytes) in ScanWith(LanguageSiteAob))
        {
            var path = ReadUnicode(at + 21 + BitConverter.ToInt32(bytes, 17), 80);
            if (path.Contains(".msgbnd", StringComparison.OrdinalIgnoreCase) && !path.Contains("sellregion", StringComparison.OrdinalIgnoreCase))
            {
                languageSite = at;
                break;
            }
        }

        if (languageSite == 0)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "language site");
            return false;
        }

        var getter = languageSite + 11 + ReadI32(languageSite + 7);
        var getterBytes = ReadBytes(getter, 0x50);

        // lea rcx,[table]; mov rax,[rcx+rbx*8] (ER, NR, DS3) or [rcx+rax*8] (AC6, no bounds check)
        var lea = IndexOf(getterBytes, "48 8D 0D ?? ?? ?? ?? 48 8B 04 D9", 0, 0x40);
        if (lea < 0)
            lea = IndexOf(getterBytes, "48 8D 0D ?? ?? ?? ?? 48 8B 04 C1", 0, 0x40);

        if (lea < 0)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "language getter");
            return false;
        }

        var bound = IndexOf(getterBytes, "83 F9 ??", 0, 0x10);

        RepoGlobal = globals.First();
        AllocatorOffset = code[allocAt + 3];
        LanguageIndexAddr = languageSite + 6 + ReadI32(languageSite + 2);
        LanguageTableAddr = getter + lea + 7 + BitConverter.ToInt32(getterBytes, lea + 3);
        LanguageCount = bound >= 0 ? getterBytes[bound + 2] : 0;

        return true;
    }

    /// <summary>First match of a pattern in bytes[from, from+length), or -1</summary>
    private static int IndexOf(byte[] bytes, string aob, int from, int length)
    {
        var pattern = aob.Split(' ').Select(t => t.StartsWith('?') ? -1 : Convert.ToInt32(t, 16)).ToArray();

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

    /// <summary>A language code as the games name their msg folders: three lower-case letters, two upper-case (engUS)</summary>
    private static bool IsLanguageCode(string s) =>
        s.Length == 5 && s.Take(3).All(char.IsAsciiLetterLower) && s.Skip(3).All(char.IsAsciiLetterUpper);

    private record Section(nint Start, int Size, bool Executable);

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
    /// Every start of these bytes in the module's data sections, preceded by a 0 byte (a whole C string)
    /// </summary>
    private List<nint> FindInData(byte[] needle)
    {
        var hits = new List<nint>();

        foreach (var section in Sections().Where(s => !s.Executable))
        {
            const int chunk = 0x100000;
            for (var offset = 0; offset < section.Size; offset += chunk)
            {
                var length = Math.Min(chunk + needle.Length, section.Size - offset);
                var start = section.Start + offset;
                var bytes = new byte[length];

                if (!ReadProcessMemory(Handle, start, bytes, length, out _))
                    continue;

                for (var i = 1; i <= length - needle.Length && i < chunk; i++)
                {
                    if (bytes[i - 1] == 0 && bytes.AsSpan(i, needle.Length).SequenceEqual(needle))
                        hits.Add(start + i);
                }
            }
        }

        return hits;
    }

    /// <summary>
    /// Every match of an AOB in the executable sections of the main module
    /// </summary>
    private List<nint> Scan(string aob) => ScanWith(aob).Select(h => h.At).ToList();

    /// <summary>
    /// Every match of an AOB in the executable sections of the main module, with the matched bytes
    /// </summary>
    private List<(nint At, byte[] Bytes)> ScanWith(string aob)
    {
        var pattern = aob.Split(' ').Select(t => t.StartsWith('?') ? -1 : Convert.ToInt32(t, 16)).ToArray();
        var hits = new List<(nint, byte[])>();

        foreach (var section in Sections().Where(s => s.Executable))
        {
            const int chunk = 0x100000;
            for (var offset = 0; offset < section.Size; offset += chunk)
            {
                var length = Math.Min(chunk + pattern.Length, section.Size - offset);
                var start = section.Start + offset;
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
                        hits.Add((start + i, bytes.AsSpan(i, pattern.Length).ToArray()));
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
        // AC6's table has no bound (14 languages, then font names), so an entry counts only if it looks like a code
        var index = ReadI32(LanguageIndexAddr);
        var language = index >= 0 && index < (LanguageCount > 0 ? LanguageCount : 32) ? ReadUnicode(ReadPtr(LanguageTableAddr + index * 8), 32) : "";
        Language = IsLanguageCode(language) ? language : "";

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
        StateMessage = LOC.Get("TEXT_FmgReloader_State_Attached", GameProcess.Id, Profile.GameName);
    }

    /// <summary>
    /// The entry ID range of an FMG block in the game: the first group's first ID to the last group's last ID.
    /// Groups are 16 bytes from +28 (offset index, first ID, last ID); the group count is at +C.
    /// </summary>
    private (int Min, int Max)? IdRangeOf(nint fmg)
    {
        var groupCount = ReadI32(fmg + 0xC);
        if (groupCount <= 0 || groupCount > 0x100000)
            return null;

        var first = ReadI32(fmg + 0x28 + 4);
        var last = ReadI32(fmg + 0x28 + (groupCount - 1) * 0x10 + 8);
        return (first, last);
    }

    #endregion

    #region Reload

    private ReloadResult Reload(SendItem item)
    {
        var bytes = item.Bytes;

        // Version 2 little-endian FMG (all supported games): 64-bit string offsets, offset table offset at +18
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

        // Identity check: the game's block must cover some of the same entry IDs as the FMG sent, so a slot that holds a
        // different FMG (an ID shared by two binders, or a project that doesn't match the running game) is left alone
        if (item.MinId <= item.MaxId)
        {
            var range = IdRangeOf(old);
            if (range == null || range.Value.Max < item.MinId || range.Value.Min > item.MaxId)
            {
                var game = range == null ? "?" : $"{range.Value.Min}-{range.Value.Max}";
                return new(item.Label, ReloadOutcome.Failed,
                    LOC.Get("TEXT_FmgReloader_Reload_Different_Fmg", item.Label, item.Id, game, $"{item.MinId}-{item.MaxId}"), DateTime.Now);
            }
        }

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
