using StudioCore.Application;
using StudioCore.Memory;
using System.Text;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// The FMG Reloader's work in a running game (ER, NR, AC6, DS3), over the project's shared GameLink: reads which FMGs
/// the game holds, and replaces one with the editor's copy. See Claude/Documentation/Text Editor.md (FMG Reloader) and
/// Text Editor.md (FMG Reloader). The connection itself (which exe, the process handle) is GameLink's.
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
public class FmgReloader
{
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
    /// What differs per game: the binders the reloader may send. Each group lists the candidates for one binder, most
    /// complete first. The one the game loaded is told apart by which slots are filled (DS3 loads item/menu_dlc2 or
    /// _dlc1 depending on DLC ownership). ngword and sellregion are left out everywhere: ngword's only FMG has binder
    /// ID 1, the same as menu's first FMG, and it doesn't go through the message repository.
    /// The executable to attach to is GameLink's (Assets/Game Link).
    /// </summary>
    public record GameProfile(ProjectType Type, string[][] Groups)
    {
        public bool IsCandidate(string containerName) =>
            Groups.Any(g => g.Contains(containerName, StringComparer.OrdinalIgnoreCase));
    }

    public static readonly GameProfile[] Profiles =
    {
        new(ProjectType.ER, new[] { new[] { "item_dlc02" }, new[] { "menu_dlc02" } }),
        new(ProjectType.NR, new[] { new[] { "item_dlc01" }, new[] { "menu_dlc01" } }),
        new(ProjectType.AC6, new[] { new[] { "item" }, new[] { "menu" } }),
        new(ProjectType.DS3, new[] { new[] { "item_dlc2", "item_dlc1" }, new[] { "menu_dlc2", "menu_dlc1" } }),
    };

    public static GameProfile ProfileFor(ProjectType type) => Profiles.FirstOrDefault(p => p.Type == type);

    /// <summary>The game's binders, or null when the FMG Reloader doesn't support this game</summary>
    public readonly GameProfile Profile;

    private readonly ProjectEntry Project;

    public FmgReloader(ProjectEntry project)
    {
        Project = project;
        Profile = ProfileFor(project.Descriptor.ProjectType);
    }

    /// <summary>The project's connection to the game, shared with the other live tools</summary>
    public GameLink Link => Project.Handler.GameLink;

    /// <summary>Whether this game has binders here and a game link</summary>
    public bool IsSupported => Profile != null && Link.IsSupported;

    // Found for one attach of the link
    private int ResolvedFor = -1;
    private nint RepoGlobal, LanguageIndexAddr, LanguageTableAddr;
    private int LanguageCount;      // 0 when the getter has no bounds check (AC6): read until an entry isn't a language code
    private int AllocatorOffset;    // +30, or +38 in DS3: read from the insert's code

    // Why the text can't be read while the link is attached (not found in this version, not loaded yet), or null
    private volatile string Problem;
    private volatile bool ProblemIsFailure;

    /// <summary>
    /// The state to show: the link's, or the reloader's own when the link is attached but the text can't be read
    /// </summary>
    public GameLink.LinkState State =>
        Link.State is GameLink.LinkState.Attached && Problem != null && ProblemIsFailure ? GameLink.LinkState.Failed : Link.State;

    public string StateMessage =>
        Link.State is GameLink.LinkState.Attached && Problem != null ? Problem : Link.StateMessage;

    /// <summary>Whether the link is attached and the message repository was found</summary>
    public bool IsReady => Link.State is GameLink.LinkState.Attached && ResolvedFor == Link.AttachId && RepoGlobal != 0;

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
                    Link.Fail(LOC.Get("TEXT_FmgReloader_State_Read_Failed", e.Message));
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

    /// <summary>
    /// Replaces FMGs in the running game, on a background task. Each one goes into the history.
    /// </summary>
    public void RequestReloadMany(IReadOnlyList<SendItem> items, SendSource source)
    {
        if (items.Count == 0)
            return;

        if (IsReloading)
        {
            Smithbox.LogError(typeof(FmgReloader), LOC.Get("TEXT_FmgReloader_Reload_Busy", items.Count));
            return;
        }

        IsReloading = true;
        var operation = Link.BeginOperation();
        var label = items.Count == 1 ? items[0].Label : LOC.Get("TEXT_FmgReloader_Reload_Many_Name", items.Count);

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
                Smithbox.LogError(typeof(FmgReloader), $"[Text Editor] In-game reload of {label} failed.", e);
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

    /// <summary>
    /// Disconnects the shared link (other live tools too) and clears what was read
    /// </summary>
    public void Detach()
    {
        Link.Detach();

        lock (Link.Lock)
            Forget();
    }

    #region Attach

    /// <summary>
    /// Connects the link and finds the message repository for this attach. Call with the link's lock held.
    /// </summary>
    private bool EnsureReady()
    {
        if (!Link.EnsureAttached())
        {
            Forget();
            return false;
        }

        if (ResolvedFor == Link.AttachId)
            return RepoGlobal != 0;

        Forget();
        ResolvedFor = Link.AttachId;

        if (!Resolve(out var error))
        {
            SetProblem(error, true);
            return false;
        }

        Smithbox.Log(typeof(FmgReloader),
            $"[Text Editor] FMG Reloader ({Link.ExeName}): MsgRepository global 0x{RepoGlobal:X}, allocator +0x{AllocatorOffset:X}, " +
            $"language index 0x{LanguageIndexAddr:X}, table 0x{LanguageTableAddr:X} ({(LanguageCount == 0 ? "unbounded" : LanguageCount)})");

        return true;
    }

    private void SetProblem(string message, bool failure)
    {
        ProblemIsFailure = failure;
        Problem = message;
    }

    /// <summary>
    /// Forgets what was found for the last attach, and what was read
    /// </summary>
    private void Forget()
    {
        ResolvedFor = -1;
        RepoGlobal = LanguageIndexAddr = LanguageTableAddr = 0;
        LanguageCount = 0;
        AllocatorOffset = 0;
        Problem = null;
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
        var names = Link.FindInData(RepositoryName).ToHashSet();
        var globals = new HashSet<nint>();

        foreach (var (at, bytes) in Link.ScanWith(SingletonCheckAob))
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
            var hits = Link.Scan(aob);
            if (hits.Count != 1)
                continue;

            var call = hits[0] + aob.Split(' ').Length - 1;
            insert = call + 5 + Link.ReadI32(call + 1);
            break;
        }

        if (insert == 0)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "insert wrapper");
            return false;
        }

        var code = Link.ReadBytes(insert, 0x400);

        // mov rcx,[reg+30h|38h] (ModRM mod=01, reg=rcx), then call [rax+50h]
        var allocAt = -1;
        for (var i = 0; i + 4 <= 0x300 && allocAt < 0; i++)
        {
            if (code[i] == 0x48 && code[i + 1] == 0x8B && (code[i + 2] & 0xF8) == 0x48 && (code[i + 2] & 7) != 4 && code[i + 3] is 0x30 or 0x38)
                allocAt = i;
        }

        if (allocAt < 0 || GameLink.IndexOf(code, "FF 50 50", allocAt, 0x40) < 0 || GameLink.IndexOf(code, InsertFixupAob, 0, code.Length - 16) < 0)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "insert layout");
            return false;
        }

        // Language: the first msgbnd path build that isn't sellregion's
        nint languageSite = 0;
        foreach (var (at, bytes) in Link.ScanWith(LanguageSiteAob))
        {
            var path = Link.ReadUnicode(at + 21 + BitConverter.ToInt32(bytes, 17), 80);
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

        var getter = languageSite + 11 + Link.ReadI32(languageSite + 7);
        var getterBytes = Link.ReadBytes(getter, 0x50);

        // lea rcx,[table]; mov rax,[rcx+rbx*8] (ER, NR, DS3) or [rcx+rax*8] (AC6, no bounds check)
        var lea = GameLink.IndexOf(getterBytes, "48 8D 0D ?? ?? ?? ?? 48 8B 04 D9", 0, 0x40);
        if (lea < 0)
            lea = GameLink.IndexOf(getterBytes, "48 8D 0D ?? ?? ?? ?? 48 8B 04 C1", 0, 0x40);

        if (lea < 0)
        {
            error = LOC.Get("TEXT_FmgReloader_State_Version", "language getter");
            return false;
        }

        var bound = GameLink.IndexOf(getterBytes, "83 F9 ??", 0, 0x10);

        RepoGlobal = globals.First();
        AllocatorOffset = code[allocAt + 3];
        LanguageIndexAddr = languageSite + 6 + Link.ReadI32(languageSite + 2);
        LanguageTableAddr = getter + lea + 7 + BitConverter.ToInt32(getterBytes, lea + 3);
        LanguageCount = bound >= 0 ? getterBytes[bound + 2] : 0;

        return true;
    }

    /// <summary>A language code as the games name their msg folders: three lower-case letters, two upper-case (engUS)</summary>
    private static bool IsLanguageCode(string s) =>
        s.Length == 5 && s.Take(3).All(char.IsAsciiLetterLower) && s.Skip(3).All(char.IsAsciiLetterUpper);

    #endregion

    #region Poll

    /// <summary>
    /// The slot array of version 0 and the slot count, or 0 when the game hasn't loaded its text yet
    /// </summary>
    private nint SlotArray(out nint repo, out int slotCount)
    {
        slotCount = 0;
        repo = Link.ReadPtr(RepoGlobal);
        if (repo == 0)
            return 0;

        var versions = Link.ReadPtr(repo + VersionArrayOffset);
        var versionCount = Link.ReadI32(repo + VersionCountOffset);
        slotCount = Link.ReadI32(repo + SlotCountOffset);

        if (versions == 0 || versionCount < 1 || slotCount <= 0 || slotCount > 0x10000)
            return 0;

        return Link.ReadPtr(versions);
    }

    /// <summary>
    /// Reads the game's language and the FMGs it holds
    /// </summary>
    private void Poll()
    {
        // AC6's table has no bound (14 languages, then font names), so an entry counts only if it looks like a code
        var index = Link.ReadI32(LanguageIndexAddr);
        var language = index >= 0 && index < (LanguageCount > 0 ? LanguageCount : 32) ? Link.ReadUnicode(Link.ReadPtr(LanguageTableAddr + index * 8), 32) : "";
        Language = IsLanguageCode(language) ? language : "";

        var slots = SlotArray(out _, out var slotCount);
        if (slots == 0)
        {
            Fmgs = Array.Empty<GameFmg>();
            SetProblem(LOC.Get("TEXT_FmgReloader_State_Not_Loaded"), false);
            return;
        }

        var pointers = Link.ReadBytes(slots, slotCount * 8);
        var fmgs = new List<GameFmg>();

        for (var id = 0; id < slotCount; id++)
        {
            var fmg = (nint)BitConverter.ToInt64(pointers, id * 8);
            if (fmg == 0)
                continue;

            var header = Link.ReadBytes(fmg, 0x14);
            var replaced = Replaced.TryGetValue(id, out var ours) && ours == fmg;
            fmgs.Add(new GameFmg(id, BitConverter.ToInt32(header, 0x10), replaced));
        }

        Fmgs = fmgs;
        Problem = null;
    }

    /// <summary>
    /// The entry ID range of an FMG block in the game: the first group's first ID to the last group's last ID.
    /// Groups are 16 bytes from +28 (offset index, first ID, last ID); the group count is at +C.
    /// </summary>
    private (int Min, int Max)? IdRangeOf(nint fmg)
    {
        var groupCount = Link.ReadI32(fmg + 0xC);
        if (groupCount <= 0 || groupCount > 0x100000)
            return null;

        var first = Link.ReadI32(fmg + 0x28 + 4);
        var last = Link.ReadI32(fmg + 0x28 + (groupCount - 1) * 0x10 + 8);
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
        var old = Link.ReadPtr(slot);

        // Only FMGs the game loaded are replaced: an empty slot means nothing reads this ID
        if (old == 0)
            return new(item.Label, ReloadOutcome.NotFound, LOC.Get("TEXT_FmgReloader_Reload_Not_Found", item.Label, item.Id), DateTime.Now);

        var oldHeader = Link.ReadBytes(old, 0x8);
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

        var allocator = Link.ReadPtr(repo + AllocatorOffset);
        if (allocator == 0)
            return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_No_Allocator"), DateTime.Now);

        var staging = Link.Alloc(bytes.Length);
        var codeBuffer = Link.Alloc(0x100, executable: true);

        // Freed only once the game's thread is done with them
        var finished = true;

        try
        {
            if (staging == 0 || codeBuffer == 0)
                return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Alloc_Failed"), DateTime.Now);

            Link.WriteBytes(staging, bytes);
            Link.WriteBytes(codeBuffer, BuildSwapCode(allocator, staging, bytes.Length, slot));

            var outcome = Link.RunThread(codeBuffer, 10000, out var exitCode, out var error);

            if (outcome is GameLink.ThreadOutcome.NotStarted)
                return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Thread_Failed", error), DateTime.Now);

            if (outcome is GameLink.ThreadOutcome.TimedOut)
            {
                finished = false;
                return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Timeout"), DateTime.Now);
            }

            if (exitCode == 0)
                return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Game_Alloc_Failed", bytes.Length), DateTime.Now);

            // Read back: the slot holds a new block that starts with the bytes sent (+18 is now a pointer)
            var current = Link.ReadPtr(slot);
            var check = current != 0 && current != old ? Link.ReadBytes(current, 0x18) : null;

            if (check == null || !check.AsSpan().SequenceEqual(bytes.AsSpan(0, 0x18)))
                return new(item.Label, ReloadOutcome.Failed, LOC.Get("TEXT_FmgReloader_Reload_Check_Failed", item.Label), DateTime.Now);

            Replaced[item.Id] = current;
            Interlocked.Add(ref keptBytes, Math.Max(0, oldSize));

            Smithbox.Log(typeof(FmgReloader), $"[Text Editor] Reloaded FMG {item.Id} ({item.Label}) in game: {bytes.Length} bytes at 0x{current:X}; kept old block 0x{old:X} ({oldSize} bytes).");
            return new(item.Label, ReloadOutcome.Reloaded, LOC.Get("TEXT_FmgReloader_Reload_Done", item.Label), DateTime.Now);
        }
        finally
        {
            if (finished)
            {
                Link.Free(staging);
                Link.Free(codeBuffer);
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
}
