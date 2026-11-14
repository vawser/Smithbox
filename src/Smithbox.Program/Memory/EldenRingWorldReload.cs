using StudioCore.Application;

namespace StudioCore.Memory;

/// <summary>
/// Reload World for a running eldenring.exe: saves, then loads the same save slot again without the title screen.
/// Used by the Material Editor's Material Reloader and the Map Editor's World Reloader, through their game link.
///
/// MoveMapStep's save/load handler (0x140AFD060 in the 1.16 Steam build) loads the save slot in GameMan +LoadSlot when
/// it isn't -1, through RequestLoad (0x14067C500) and LoadGame (0x14067C590), which restarts the world from that save.
/// The game only ever resets that field, so this looks like a leftover developer path.
/// Before it, a save request (GameMan +SaveRequest) writes the current state, so the reload loses nothing.
/// </summary>
public class EldenRingWorldReload
{
    // mov rax,[GameMan]; mov eax,[rax+CurrentSlot]; ret (AC0 in 1.16; the 0x0AC0 pins it among the many such getters)
    private const string CurrentSlotAob = "48 8B 05 ?? ?? ?? ?? 8B 80 C0 0A 00 00 C3";
    // the save request getter: mov rax,[GameMan]; movzx ebx,byte ptr [rax+SaveRequest]; call; xor edi,edi; test al,al; cmovne ebx,edi
    private const string SaveRequestAob = "48 8B 05 ?? ?? ?? ?? 0F B6 98 ?? ?? ?? ?? E8 ?? ?? ?? ?? 33 FF 84 C0 0F 45 DF";
    // the load slot getter (mov rax,[GameMan]; mov eax,[rax+LoadSlot]; ret) sits just before the save request getter:
    // 0x14067A6C0 vs the save AOB match at 0x14067A6DA
    private const int LoadSlotBeforeSave = 0x1A;

    private const int TimeoutMs = 10000;

    private nint GameManGlobal;
    private int SaveRequestOffset, LoadSlotOffset, CurrentSlotOffset;

    /// <summary>
    /// Whether the world reload request was found in this game version
    /// </summary>
    public bool CanReload => GameManGlobal != 0;

    /// <summary>The outcome of the last reload, for the UI</summary>
    public volatile string LastMessage;

    public volatile bool IsRunning;

    /// <summary>
    /// Forgets the resolved addresses, when the connection closes
    /// </summary>
    public void Clear()
    {
        GameManGlobal = 0;
    }

    /// <summary>
    /// Finds GameMan and its save request, load slot and current slot fields. Optional for the caller: without them,
    /// <see cref="CanReload"/> is false. Called with the game's lock held.
    /// </summary>
    public void Resolve(IGameProcess game)
    {
        GameManGlobal = 0;

        var saves = game.Scan(SaveRequestAob);
        var currents = game.Scan(CurrentSlotAob);

        if (saves.Count != 1 || currents.Count != 1)
        {
            Smithbox.Log(typeof(EldenRingWorldReload), $"[World Reload] Unavailable: save request {saves.Count}, current slot {currents.Count} match(es).");
            return;
        }

        var save = saves[0];
        var load = save - LoadSlotBeforeSave;
        var loadBytes = game.ReadBytes(load, 14);

        if (loadBytes[0] != 0x48 || loadBytes[1] != 0x8B || loadBytes[2] != 0x05 || loadBytes[7] != 0x8B || loadBytes[8] != 0x80 || loadBytes[13] != 0xC3)
        {
            Smithbox.Log(typeof(EldenRingWorldReload), $"[World Reload] Unavailable: unexpected load slot getter {Convert.ToHexString(loadBytes)}.");
            return;
        }

        var gameMan = save + 7 + ReadI32(game, save + 3);

        // All three must read the same GameMan
        if (load + 7 + ReadI32(game, load + 3) != gameMan || currents[0] + 7 + ReadI32(game, currents[0] + 3) != gameMan)
        {
            Smithbox.Log(typeof(EldenRingWorldReload), "[World Reload] Unavailable: the getters read different globals.");
            return;
        }

        GameManGlobal = gameMan;
        SaveRequestOffset = ReadI32(game, save + 10);
        LoadSlotOffset = BitConverter.ToInt32(loadBytes, 9);
        CurrentSlotOffset = ReadI32(game, currents[0] + 9);

        Smithbox.Log(typeof(EldenRingWorldReload),
            $"[World Reload] GameMan global 0x{GameManGlobal:X}, save request +0x{SaveRequestOffset:X}, load slot +0x{LoadSlotOffset:X}, current slot +0x{CurrentSlotOffset:X}");
    }

    /// <summary>
    /// Saves, then asks the world step to load the current save slot again, waiting for the game to take each request.
    /// Each request is withdrawn if the game doesn't take it, so nothing is left to fire later. Blocks for up to 20 s,
    /// so call it from a background task; it takes the game's lock itself.
    /// </summary>
    public void Run(IGameProcess game)
    {
        IsRunning = true;

        try
        {
            nint gameMan;
            int slot;

            lock (game.Lock)
            {
                if (!CanReload)
                {
                    LastMessage = LOC.Get("MAT_InGameTool_World_Unavailable");
                    return;
                }

                gameMan = ReadPtr(game, GameManGlobal);
                slot = gameMan != 0 ? ReadI32(game, gameMan + CurrentSlotOffset) : -1;

                if (slot < 0 || slot > 9)
                {
                    LastMessage = LOC.Get("MAT_InGameTool_World_No_Save");
                    return;
                }

                if (ReadI32(game, gameMan + LoadSlotOffset) != -1)
                {
                    LastMessage = LOC.Get("MAT_InGameTool_World_Busy");
                    return;
                }

                // Save first, as quitting does, so the reload doesn't go back to an older save
                game.WriteBytes(gameMan + SaveRequestOffset, new byte[] { 1 });
            }

            Smithbox.Log(typeof(EldenRingWorldReload), $"[World Reload] Save requested (slot {slot}).");

            var saved = WaitFor(game, () => game.ReadBytes(gameMan + SaveRequestOffset, 1)[0] == 0);
            if (saved == null)
                return;

            if (saved == false)
            {
                lock (game.Lock)
                {
                    if (game.IsOpen)
                        game.WriteBytes(gameMan + SaveRequestOffset, new byte[] { 0 });
                }

                LastMessage = LOC.Get("MAT_InGameTool_World_Save_Not_Taken");
                Smithbox.Log(typeof(EldenRingWorldReload), "[World Reload] Save not taken within 10 s; nothing reloaded.");
                return;
            }

            lock (game.Lock)
            {
                if (!game.IsOpen)
                    return;

                game.WriteBytes(gameMan + LoadSlotOffset, BitConverter.GetBytes(slot));
            }

            Smithbox.Log(typeof(EldenRingWorldReload), $"[World Reload] Load of slot {slot} requested.");

            // The world step clears the field once it has accepted the load
            var loaded = WaitFor(game, () => ReadI32(game, gameMan + LoadSlotOffset) == -1);
            if (loaded == null)
                return;

            if (loaded == false)
            {
                lock (game.Lock)
                {
                    if (game.IsOpen)
                        game.WriteBytes(gameMan + LoadSlotOffset, BitConverter.GetBytes(-1));
                }

                LastMessage = LOC.Get("MAT_InGameTool_World_Load_Not_Taken");
                Smithbox.Log(typeof(EldenRingWorldReload), "[World Reload] Load not taken within 10 s; request withdrawn.");
                return;
            }

            LastMessage = LOC.Get("MAT_InGameTool_World_Done", DateTime.Now.ToString("T"));
            Smithbox.Log(typeof(EldenRingWorldReload), "[World Reload] Load accepted.");
        }
        finally
        {
            IsRunning = false;
        }
    }

    /// <summary>
    /// Polls a condition on the game every 100 ms. True when it held, false on timeout, null when the game went away.
    /// </summary>
    private static bool? WaitFor(IGameProcess game, Func<bool> condition)
    {
        for (var waited = 0; waited < TimeoutMs; waited += 100)
        {
            Thread.Sleep(100);

            lock (game.Lock)
            {
                if (!game.IsOpen)
                    return null;

                if (condition())
                    return true;
            }
        }

        return false;
    }

    private static int ReadI32(IGameProcess game, nint address) => BitConverter.ToInt32(game.ReadBytes(address, 4), 0);

    private static nint ReadPtr(IGameProcess game, nint address) => (nint)BitConverter.ToInt64(game.ReadBytes(address, 8), 0);
}
