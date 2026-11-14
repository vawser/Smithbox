using StudioCore.Application;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The link to a running eldenring.exe used by the In-Game tool: lists the materials loaded models use, and
/// reloads one material from bytes. See Claude/Documentation/MATBIN Live Reload Analysis.md.
///
/// How the game holds materials (addresses are the 1.16 Steam build, found by AOB at runtime):
///   MtdRepository (singleton) +78 = hash map of MtdResCap, keyed by file name up to the first '.'
///     map +1C bucket count (u32), +20 bucket array; node +18 name (MSVC wstring, +28 length, +30 capacity),
///     +50 next in bucket, +78 GXMaterialData* (refcount u32 at +8; the ResCap holds one, every model material another)
///   Find(repo, wchar* name)      returns the ResCap
///   Init(rc, bytes, size)        parses a MATBIN into a new GXMaterialData and swaps it in, releasing the old one
/// </summary>
public class MatGameLink
{
    public enum LinkState { NotRunning, Attached, Failed }

    /// <param name="FirstSeen">when it was first seen in use, while it has stayed in use</param>
    /// <param name="Initial">in use at the first poll after connecting, so not "recently loaded"</param>
    public record InGameMaterial(string Name, int Users, DateTime FirstSeen, bool Initial);

    /// <param name="Name">the model's file name up to the first '.' (c2010, AEG099_001, m60_44_36_00_600000)</param>
    public record InGameModel(string Name, DateTime FirstSeen, bool Initial);

    public enum ReloadOutcome { Reloaded, NotFound, Failed }

    public record ReloadResult(string Name, ReloadOutcome Outcome, string Message, DateTime At);

    // Updated by the background task, read by the UI
    public volatile LinkState State = LinkState.NotRunning;
    public volatile string StateMessage = "";
    public volatile IReadOnlyList<InGameMaterial> InUse = Array.Empty<InGameMaterial>();
    public volatile IReadOnlyList<InGameModel> Models = Array.Empty<InGameModel>();
    public volatile int TotalMaterials;

    /// <summary>
    /// Whether the loaded model list was found; without it, only materials are listed
    /// </summary>
    public bool CanListModels => FlverRepoPtrAddr != 0;
    public volatile bool IsPolling;
    public volatile bool IsReloading;
    public volatile ReloadResult LastReload;
    public volatile string LastQuitOut;
    public volatile bool IsQuittingOut;
    public DateTime? LastPoll { get; private set; }

    /// <summary>
    /// Whether the quit-to-title request was found; without it, materials still reload
    /// </summary>
    public bool CanQuitOut => MenuManGlobal != 0;

    private readonly object GameLock = new();
    private Process GameProcess;
    private nint Handle;
    private nint RepoPtrAddr, FindAddr, InitAddr;
    private readonly Dictionary<(nint Node, long Pointer, long Length), string> NameCache = new();

    // Loaded models: FlverRepositoryImp is the same FD4 resource map as MtdRepository (+78), keyed by the model's file
    // name up to the first '.'. Found from a lookup of the player model: mov rcx,[repo]; lea rdx,["c0000"]; call Find;
    // mov rcx,[rax+78h]; lea rbx,[rcx+500h]
    private nint FlverRepoPtrAddr;
    private const string FlverLookupAob = "48 8B 0D ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 48 78 48 8D 99 00 05 00 00";

    // First-seen times for "recently loaded", kept by the poll
    private readonly Dictionary<string, DateTime> MaterialFirstSeen = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTime> ModelFirstSeen = new(StringComparer.OrdinalIgnoreCase);
    private bool HasBaseline;
    private DateTime BaselineTime;

    // Quit to title: the game's own request (0x1407A4BD0) sets byte [[CSMenuManImp]+8]+5D = 1; MoveMapStep reads it,
    // leaves the world and the byte is cleared. Loading back in rebuilds every model, so all of them pick up reloaded
    // materials. The MATBIN binders stay resident, so reloaded materials survive the trip to the title screen.
    private nint MenuManGlobal;

    // mov rax,[rax+8]; mov byte ptr [rax+5Dh],1; add rsp,28h; jmp — the tail of the request; the function starts 0x3E before
    private const string QuitOutAob = "48 8B 40 08 C6 40 5D 01 48 83 C4 28 E9";
    private const int QuitOutAobOffset = 0x3E;
    private static readonly byte[] QuitOutPrologue = { 0x48, 0x83, 0xEC, 0x28, 0x48, 0x8B, 0x05 }; // sub rsp,28h; mov rax,[rip+x]
    private const int QuitOutMenuOffset = 0x8, QuitOutFlagOffset = 0x5D;

    // Reload world: MoveMapStep's save/load handler (0x140AFD060) loads the save slot in GameMan +LoadSlot when it isn't
    // -1, through RequestLoad (0x14067C500) and LoadGame (0x14067C590), which restarts the world from that save without
    // the title screen. The game only ever resets that field, so this looks like a leftover developer path.
    // Before it, a save request (GameMan +SaveRequest) writes the current state, so the reload loses nothing.
    private nint GameManGlobal;
    private int SaveRequestOffset, LoadSlotOffset, CurrentSlotOffset;

    // mov rax,[GameMan]; mov eax,[rax+CurrentSlot]; ret (AC0 in 1.16; the 0x0AC0 pins it among the many such getters)
    private const string CurrentSlotAob = "48 8B 05 ?? ?? ?? ?? 8B 80 C0 0A 00 00 C3";
    // the save request getter: mov rax,[GameMan]; movzx ebx,byte ptr [rax+SaveRequest]; call; xor edi,edi; test al,al; cmovne ebx,edi
    private const string SaveRequestAob = "48 8B 05 ?? ?? ?? ?? 0F B6 98 ?? ?? ?? ?? E8 ?? ?? ?? ?? 33 FF 84 C0 0F 45 DF";
    // the load slot getter (mov rax,[GameMan]; mov eax,[rax+LoadSlot]; ret) sits just before the save request getter:
    // 0x14067A6C0 vs the save AOB match at 0x14067A6DA
    private const int LoadSlotBeforeSave = 0x1A;

    /// <summary>
    /// Whether the world reload request was found; without it, materials still reload
    /// </summary>
    public bool CanReloadWorld => GameManGlobal != 0;

    public volatile string LastWorldReload;
    public volatile bool IsReloadingWorld;

    public enum AfterReload { Nothing, ReloadWorld, QuitOut }

    /// <summary>A material to send: the game's name for it and its MATBIN bytes</summary>
    public record SendItem(string Name, byte[] Bytes);

    /// <summary>Where a send came from, shown in the history</summary>
    public enum SendSource { Selected, Modified, Save, History, Vanilla }

    private const string ProcessName = "eldenring";

    // CSFlverResourceFactory::GetMaterialData's fallback: mov rcx,[repo]; lea rdx,["M[A]"]; call Find; test rax,rax; jne
    private const string FallbackLookupAob = "48 8B 0D ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 75";
    // MtdResCap::Init's magic check: mov eax,[r14]; cmp eax,'MAB\0'; je; cmp eax,'\0BAM'
    private const string InitMagicAob = "41 8B 06 3D 4D 41 42 00 74 ?? 3D 00 42 41 4D";
    private const int InitMagicOffset = 0xE4;
    private static readonly byte[] InitPrologue = { 0x40, 0x53, 0x55, 0x56, 0x57, 0x41, 0x56, 0x48, 0x81, 0xEC, 0xE0, 0x00, 0x00, 0x00 };

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
                lock (GameLock)
                {
                    if (EnsureAttached())
                        Poll();
                }
            }
            catch (Exception e)
            {
                Fail(LOC.Get("MAT_InGameTool_State_Read_Failed", e.Message));
            }
            finally
            {
                LastPoll = DateTime.Now;
                IsPolling = false;
            }
        });
    }

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
            Smithbox.LogError(typeof(MatGameLink), LOC.Get("MAT_InGameTool_Reload_Busy", items.Count));
            return;
        }

        IsReloading = true;
        var label = items.Count == 1 ? items[0].Name : LOC.Get("MAT_InGameTool_Reload_Many_Name", items.Count);

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
                        var result = Reload(item.Name, item.Bytes);
                        results.Add(result);
                        AddHistory(item.Name, result.Outcome, source);
                    }
                }

                LastReload = results.Count == 1 ? results[0] : Summarise(label, results);

                var reloaded = results.Any(r => r.Outcome is ReloadOutcome.Reloaded);

                if (reloaded && after is AfterReload.ReloadWorld)
                    ReloadWorldAndConfirm();
                else if (reloaded && after is AfterReload.QuitOut)
                    QuitOutAndConfirm();
            }
            catch (Exception e)
            {
                LastReload = new(label, ReloadOutcome.Failed, e.Message, DateTime.Now);
                Smithbox.LogError(typeof(MatGameLink), $"[Material Editor] In-game reload of {label} failed.", e);
            }
            finally
            {
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

    /// <summary>
    /// Asks the game to save and load the current save again, on a background task
    /// </summary>
    public void RequestReloadWorld()
    {
        if (IsReloadingWorld || IsReloading || IsQuittingOut)
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

                ReloadWorldAndConfirm();
            }
            catch (Exception e)
            {
                LastWorldReload = LOC.Get("MAT_InGameTool_World_Failed", e.Message);
                Smithbox.LogError(typeof(MatGameLink), "[Material Editor] Reload world failed.", e);
            }
            finally
            {
                IsReloadingWorld = false;
            }
        });
    }

    /// <summary>
    /// Saves, then asks the world step to load the current save slot again. Each request is withdrawn if the game
    /// doesn't take it, so nothing is left to fire later.
    /// </summary>
    private void ReloadWorldAndConfirm()
    {
        IsReloadingWorld = true;

        try
        {
            nint gameMan;
            int slot;

            lock (GameLock)
            {
                if (!CanReloadWorld)
                {
                    LastWorldReload = LOC.Get("MAT_InGameTool_World_Unavailable");
                    return;
                }

                gameMan = ReadPtr(GameManGlobal);
                slot = gameMan != 0 ? ReadI32(gameMan + CurrentSlotOffset) : -1;

                if (slot < 0 || slot > 9)
                {
                    LastWorldReload = LOC.Get("MAT_InGameTool_World_No_Save");
                    return;
                }

                if (ReadI32(gameMan + LoadSlotOffset) != -1)
                {
                    LastWorldReload = LOC.Get("MAT_InGameTool_World_Busy");
                    return;
                }

                // Save first, as quitting does, so the reload doesn't go back to an older save
                WriteBytes(gameMan + SaveRequestOffset, new byte[] { 1 });
            }

            Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Reload world: save requested (slot {slot}).");

            var saved = WaitFor(() => ReadBytes(gameMan + SaveRequestOffset, 1)[0] == 0, 10000);
            if (saved == null)
                return;

            if (saved == false)
            {
                lock (GameLock)
                {
                    if (Handle != 0)
                        WriteBytes(gameMan + SaveRequestOffset, new byte[] { 0 });
                }

                LastWorldReload = LOC.Get("MAT_InGameTool_World_Save_Not_Taken");
                Smithbox.Log(typeof(MatGameLink), "[Material Editor] Reload world: save not taken within 10 s; nothing reloaded.");
                return;
            }

            lock (GameLock)
            {
                if (Handle == 0)
                    return;

                WriteBytes(gameMan + LoadSlotOffset, BitConverter.GetBytes(slot));
            }

            Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Reload world: load of slot {slot} requested.");

            // The world step clears the field once it has accepted the load
            var loaded = WaitFor(() => ReadI32(gameMan + LoadSlotOffset) == -1, 10000);
            if (loaded == null)
                return;

            if (loaded == false)
            {
                lock (GameLock)
                {
                    if (Handle != 0)
                        WriteBytes(gameMan + LoadSlotOffset, BitConverter.GetBytes(-1));
                }

                LastWorldReload = LOC.Get("MAT_InGameTool_World_Load_Not_Taken");
                Smithbox.Log(typeof(MatGameLink), "[Material Editor] Reload world: load not taken within 10 s; request withdrawn.");
                return;
            }

            LastWorldReload = LOC.Get("MAT_InGameTool_World_Done", DateTime.Now.ToString("T"));
            Smithbox.Log(typeof(MatGameLink), "[Material Editor] Reload world: load accepted.");
        }
        finally
        {
            IsReloadingWorld = false;
        }
    }

    /// <summary>
    /// Polls a condition on the game every 100 ms. True when it held, false on timeout, null when the game went away.
    /// </summary>
    private bool? WaitFor(Func<bool> condition, int timeoutMs)
    {
        for (var waited = 0; waited < timeoutMs; waited += 100)
        {
            Thread.Sleep(100);

            lock (GameLock)
            {
                if (Handle == 0)
                    return null;

                if (condition())
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Asks the game to quit to the title screen, on a background task
    /// </summary>
    public void RequestQuitOut()
    {
        if (IsQuittingOut || IsReloading)
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

                QuitOutAndConfirm();
            }
            catch (Exception e)
            {
                LastQuitOut = LOC.Get("MAT_InGameTool_QuitOut_Failed", e.Message);
                Smithbox.LogError(typeof(MatGameLink), "[Material Editor] Quit out failed.", e);
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
    /// </summary>
    private void QuitOutAndConfirm()
    {
        IsQuittingOut = true;

        try
        {
            nint flag;

            lock (GameLock)
            {
                if (!CanQuitOut)
                {
                    LastQuitOut = LOC.Get("MAT_InGameTool_QuitOut_Unavailable");
                    return;
                }

                var menuMan = ReadPtr(MenuManGlobal);
                var menu = menuMan != 0 ? ReadPtr(menuMan + QuitOutMenuOffset) : 0;

                if (menu == 0)
                {
                    LastQuitOut = LOC.Get("MAT_InGameTool_QuitOut_Not_In_World");
                    return;
                }

                flag = menu + QuitOutFlagOffset;
                WriteBytes(flag, new byte[] { 1 });
            }

            Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Quit out requested (flag 0x{flag:X}).");

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
                        LastQuitOut = LOC.Get("MAT_InGameTool_QuitOut_Done", DateTime.Now.ToString("T"));
                        return;
                    }
                }
            }

            lock (GameLock)
            {
                if (Handle != 0)
                    WriteBytes(flag, new byte[] { 0 });
            }

            LastQuitOut = LOC.Get("MAT_InGameTool_QuitOut_Not_Taken");
            Smithbox.Log(typeof(MatGameLink), "[Material Editor] Quit out not taken within 5 s; request withdrawn.");
        }
        finally
        {
            IsQuittingOut = false;
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

    public void Detach()
    {
        lock (GameLock)
        {
            Close();
            State = LinkState.NotRunning;
            StateMessage = "";
            InUse = Array.Empty<InGameMaterial>();
        }
    }

    #region Attach

    private bool EnsureAttached()
    {
        if (GameProcess != null && !GameProcess.HasExited && Handle != 0 && FindAddr != 0)
            return true;

        Close();

        var processes = Process.GetProcessesByName(ProcessName);
        if (processes.Length == 0)
        {
            State = LinkState.NotRunning;
            StateMessage = LOC.Get("MAT_InGameTool_State_Not_Running");
            InUse = Array.Empty<InGameMaterial>();
            return false;
        }

        GameProcess = processes[0];
        Handle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION,
            false, GameProcess.Id);

        if (Handle == 0)
        {
            Fail(LOC.Get("MAT_InGameTool_State_Open_Failed", Marshal.GetLastWin32Error()));
            return false;
        }

        if (!Resolve(out var error))
        {
            Fail(error);
            return false;
        }

        State = LinkState.Attached;
        StateMessage = LOC.Get("MAT_InGameTool_State_Attached", GameProcess.Id);

        Smithbox.Log(typeof(MatGameLink),
            $"[Material Editor] In-game link: pid {GameProcess.Id}, repository ptr 0x{RepoPtrAddr:X}, Find 0x{FindAddr:X}, Init 0x{InitAddr:X}");

        return true;
    }

    private void Fail(string message)
    {
        State = LinkState.Failed;
        StateMessage = message;
        InUse = Array.Empty<InGameMaterial>();
        Close();
    }

    private void Close()
    {
        if (Handle != 0)
            CloseHandle(Handle);

        Handle = 0;
        GameProcess?.Dispose();
        GameProcess = null;
        RepoPtrAddr = FindAddr = InitAddr = 0;
        NameCache.Clear();
        MenuManGlobal = 0;
        GameManGlobal = 0;
        FlverRepoPtrAddr = 0;
        MaterialFirstSeen.Clear();
        ModelFirstSeen.Clear();
        HasBaseline = false;
        Models = Array.Empty<InGameModel>();
    }

    /// <summary>
    /// Finds the repository pointer, Find and Init in the running exe's code
    /// </summary>
    private bool Resolve(out string error)
    {
        error = "";

        var lookups = Scan(FallbackLookupAob);
        var inits = Scan(InitMagicAob);

        if (lookups.Count != 1 || inits.Count != 1)
        {
            error = LOC.Get("MAT_InGameTool_State_Version", lookups.Count, inits.Count);
            return false;
        }

        var site = lookups[0];
        RepoPtrAddr = site + 7 + ReadI32(site + 3);
        var fallbackName = site + 14 + ReadI32(site + 10);
        FindAddr = site + 19 + ReadI32(site + 15);

        // The lea must point at the fallback material's name, or this is a different lookup
        if (ReadUnicode(fallbackName, 8) != "M[A]")
        {
            error = LOC.Get("MAT_InGameTool_State_Version", "M[A]", 1);
            return false;
        }

        InitAddr = inits[0] - InitMagicOffset;

        if (!ReadBytes(InitAddr, InitPrologue.Length).AsSpan().SequenceEqual(InitPrologue))
        {
            error = LOC.Get("MAT_InGameTool_State_Version", 1, "Init");
            return false;
        }

        ResolveQuitOut();
        ResolveReloadWorld();
        ResolveModels();

        return true;
    }

    /// <summary>
    /// Finds the loaded model repository. Optional: without it, only materials are listed.
    /// </summary>
    private void ResolveModels()
    {
        FlverRepoPtrAddr = 0;

        var sites = Scan(FlverLookupAob);
        if (sites.Count != 1)
        {
            Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Loaded models unavailable: {sites.Count} match(es).");
            return;
        }

        var site = sites[0];
        var name = site + 14 + ReadI32(site + 10);

        if (ReadUnicode(name, 12) != "c0000")
        {
            Smithbox.Log(typeof(MatGameLink), "[Material Editor] Loaded models unavailable: the lookup isn't of c0000.");
            return;
        }

        FlverRepoPtrAddr = site + 7 + ReadI32(site + 3);
        Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Loaded models: FlverRepository global 0x{FlverRepoPtrAddr:X}");
    }

    /// <summary>
    /// Finds GameMan and its save request, load slot and current slot fields. Optional.
    /// </summary>
    private void ResolveReloadWorld()
    {
        GameManGlobal = 0;

        var saves = Scan(SaveRequestAob);
        var currents = Scan(CurrentSlotAob);

        if (saves.Count != 1 || currents.Count != 1)
        {
            Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Reload world unavailable: save request {saves.Count}, current slot {currents.Count} match(es).");
            return;
        }

        var save = saves[0];
        var load = save - LoadSlotBeforeSave;
        var loadBytes = ReadBytes(load, 14);

        if (loadBytes[0] != 0x48 || loadBytes[1] != 0x8B || loadBytes[2] != 0x05 || loadBytes[7] != 0x8B || loadBytes[8] != 0x80 || loadBytes[13] != 0xC3)
        {
            Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Reload world unavailable: unexpected load slot getter {Convert.ToHexString(loadBytes)}.");
            return;
        }

        var gameMan = save + 7 + ReadI32(save + 3);

        // All three must read the same GameMan
        if (load + 7 + ReadI32(load + 3) != gameMan || currents[0] + 7 + ReadI32(currents[0] + 3) != gameMan)
        {
            Smithbox.Log(typeof(MatGameLink), "[Material Editor] Reload world unavailable: the getters read different globals.");
            return;
        }

        GameManGlobal = gameMan;
        SaveRequestOffset = ReadI32(save + 10);
        LoadSlotOffset = BitConverter.ToInt32(loadBytes, 9);
        CurrentSlotOffset = ReadI32(currents[0] + 9);

        Smithbox.Log(typeof(MatGameLink),
            $"[Material Editor] Reload world: GameMan global 0x{GameManGlobal:X}, save request +0x{SaveRequestOffset:X}, load slot +0x{LoadSlotOffset:X}, current slot +0x{CurrentSlotOffset:X}");
    }

    /// <summary>
    /// Finds the menu manager's quit-to-title request. Optional: without it, materials still reload.
    /// </summary>
    private void ResolveQuitOut()
    {
        MenuManGlobal = 0;

        var tails = Scan(QuitOutAob);
        if (tails.Count != 1)
        {
            Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Quit out unavailable: {tails.Count} match(es).");
            return;
        }

        var start = tails[0] - QuitOutAobOffset;
        if (!ReadBytes(start, QuitOutPrologue.Length).AsSpan().SequenceEqual(QuitOutPrologue))
        {
            Smithbox.Log(typeof(MatGameLink), "[Material Editor] Quit out unavailable: unexpected function start.");
            return;
        }

        // mov rax,[CSMenuManImp] at start+4
        MenuManGlobal = start + 11 + ReadI32(start + 7);

        Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Quit out: CSMenuManImp global 0x{MenuManGlobal:X}");
    }

    /// <summary>
    /// Every match of an AOB in the executable sections of the main module
    /// </summary>
    private List<nint> Scan(string aob)
    {
        var pattern = aob.Split(' ').Select(t => t.StartsWith('?') ? -1 : Convert.ToInt32(t, 16)).ToArray();
        var hits = new List<nint>();
        var module = GameProcess.MainModule.BaseAddress;

        // PE headers: section table
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

    private record MapNode(string Name, nint Data);

    /// <summary>
    /// Walks the materials (with their users) and the loaded models, and stamps when each first appeared
    /// </summary>
    private void Poll()
    {
        var repo = ReadPtr(RepoPtrAddr);
        if (repo == 0)
        {
            StateMessage = LOC.Get("MAT_InGameTool_State_Not_Loaded");
            InUse = Array.Empty<InGameMaterial>();
            Models = Array.Empty<InGameModel>();
            return;
        }

        var materialNodes = WalkMap(repo + 0x78, out var bucketCount);
        if (materialNodes == null)
        {
            StateMessage = LOC.Get("MAT_InGameTool_State_Bad_Map", bucketCount);
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

            var refs = ReadI32(node.Data + 8);
            if (refs <= 1)
                continue;

            seenMaterials.Add(node.Name);
            var firstSeen = Stamp(MaterialFirstSeen, node.Name, now, baseline);
            materials.Add(new(node.Name, refs - 1, firstSeen, baseline || firstSeen == BaselineTime));
        }

        Forget(MaterialFirstSeen, seenMaterials);

        // Models
        var models = new List<InGameModel>();

        if (FlverRepoPtrAddr != 0)
        {
            var flverRepo = ReadPtr(FlverRepoPtrAddr);
            var modelNodes = flverRepo != 0 ? WalkMap(flverRepo + 0x78, out _) : null;
            var seenModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var node in modelNodes ?? new List<MapNode>())
            {
                if (!seenModels.Add(node.Name))
                    continue;

                var firstSeen = Stamp(ModelFirstSeen, node.Name, now, baseline);
                models.Add(new(node.Name, firstSeen, baseline || firstSeen == BaselineTime));
            }

            Forget(ModelFirstSeen, seenModels);
        }

        if (baseline)
        {
            HasBaseline = true;
            BaselineTime = now;

            // TEMP diagnostics for the first in-app check of the model list: count and name format (2026-11-14)
            Smithbox.Log(typeof(MatGameLink),
                $"[Material Editor] Loaded models: {models.Count}; sample: {string.Join(", ", models.Where((_, i) => i % Math.Max(1, models.Count / 12) == 0).Take(12).Select(m => m.Name))}");
        }

        materials.Sort((a, c) => string.Compare(a.Name, c.Name, StringComparison.OrdinalIgnoreCase));
        models.Sort((a, c) => string.Compare(a.Name, c.Name, StringComparison.OrdinalIgnoreCase));

        TotalMaterials = materialNodes.Count;
        InUse = materials;
        Models = models;
        StateMessage = LOC.Get("MAT_InGameTool_State_Attached", GameProcess.Id);
    }

    /// <summary>
    /// When a name first appeared: kept while it stays loaded, so a model that unloads and loads again counts as new.
    /// Names present at the first poll share the baseline time and aren't shown as new.
    /// </summary>
    private DateTime Stamp(Dictionary<string, DateTime> firstSeen, string name, DateTime now, bool baseline)
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
    /// Every node of an FD4 resource map: +1C bucket count, +20 bucket array; node +18 name, +50 next, +78 data.
    /// Null when the map doesn't look valid.
    /// </summary>
    private List<MapNode> WalkMap(nint map, out int bucketCount)
    {
        bucketCount = ReadI32(map + 0x1C);
        var buckets = ReadPtr(map + 0x20);

        if (bucketCount <= 0 || bucketCount > 1 << 20 || buckets == 0)
            return null;

        var bucketBytes = ReadBytes(buckets, bucketCount * 8);
        var nodes = new List<MapNode>();

        for (var b = 0; b < bucketCount; b++)
        {
            var node = (nint)BitConverter.ToInt64(bucketBytes, b * 8);

            for (var chain = 0; node != 0 && chain < 4096; chain++)
            {
                var nodeBytes = ReadBytes(node, 0x80);
                var name = NameOf(node, nodeBytes);

                if (name != null)
                    nodes.Add(new(name, (nint)BitConverter.ToInt64(nodeBytes, 0x78)));

                node = (nint)BitConverter.ToInt64(nodeBytes, 0x50);
            }
        }

        return nodes;
    }

    private string NameOf(nint node, byte[] nodeBytes)
    {
        // MSVC wstring: inline buffer when capacity < 8, else a pointer
        var length = BitConverter.ToInt64(nodeBytes, 0x28);
        var capacity = BitConverter.ToInt64(nodeBytes, 0x30);
        var pointer = BitConverter.ToInt64(nodeBytes, 0x18);

        if (length <= 0 || length > 260)
            return null;

        // Model nodes are freed and reused as models load and unload, so the key includes the name's own fields
        var key = (node, pointer, length);
        if (NameCache.TryGetValue(key, out var cached))
            return cached;

        var name = capacity < 8
            ? Encoding.Unicode.GetString(nodeBytes, 0x18, (int)length * 2)
            : Encoding.Unicode.GetString(ReadBytes((nint)pointer, (int)length * 2));

        if (NameCache.Count > 200000)
            NameCache.Clear();

        NameCache[key] = name;
        return name;
    }

    #endregion

    #region Reload

    private ReloadResult Reload(string gameName, byte[] matbin)
    {
        var nameBytes = Encoding.Unicode.GetBytes(gameName + "\0");

        var dataBuffer = VirtualAllocEx(Handle, 0, matbin.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        var nameBuffer = VirtualAllocEx(Handle, 0, nameBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        var codeBuffer = VirtualAllocEx(Handle, 0, 0x100, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

        // Freed only once the game's thread is done with them; a thread still running would crash on freed memory
        var finished = true;

        try
        {
            if (dataBuffer == 0 || nameBuffer == 0 || codeBuffer == 0)
                return new(gameName, ReloadOutcome.Failed, LOC.Get("MAT_InGameTool_Reload_Alloc_Failed"), DateTime.Now);

            WriteBytes(dataBuffer, matbin);
            WriteBytes(nameBuffer, nameBytes);
            WriteBytes(codeBuffer, BuildReloadCode(nameBuffer, dataBuffer, matbin.Length));

            var thread = CreateRemoteThread(Handle, 0, 0, codeBuffer, 0, 0, out _);
            if (thread == 0)
                return new(gameName, ReloadOutcome.Failed, LOC.Get("MAT_InGameTool_Reload_Thread_Failed", Marshal.GetLastWin32Error()), DateTime.Now);

            try
            {
                finished = WaitForSingleObject(thread, 10000) == 0;

                if (!finished)
                    return new(gameName, ReloadOutcome.Failed, LOC.Get("MAT_InGameTool_Reload_Timeout"), DateTime.Now);

                GetExitCodeThread(thread, out var exitCode);

                if (exitCode == 0)
                    return new(gameName, ReloadOutcome.NotFound, LOC.Get("MAT_InGameTool_Reload_Not_Found", gameName), DateTime.Now);

                Smithbox.Log(typeof(MatGameLink), $"[Material Editor] Reloaded {gameName} in game ({matbin.Length} bytes).");
                return new(gameName, ReloadOutcome.Reloaded, LOC.Get("MAT_InGameTool_Reload_Done", gameName), DateTime.Now);
            }
            finally
            {
                CloseHandle(thread);
            }
        }
        finally
        {
            if (finished)
            {
                if (dataBuffer != 0) VirtualFreeEx(Handle, dataBuffer, 0, MEM_RELEASE);
                if (nameBuffer != 0) VirtualFreeEx(Handle, nameBuffer, 0, MEM_RELEASE);
                if (codeBuffer != 0) VirtualFreeEx(Handle, codeBuffer, 0, MEM_RELEASE);
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
