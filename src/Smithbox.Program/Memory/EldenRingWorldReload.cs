using StudioCore.Application;

namespace StudioCore.Memory;

/// <summary>
/// Reload World for a running eldenring.exe: saves, then loads the same save slot again without the title screen.
/// Owned by GameLink, so the Material Reloader and the Map Editor's World Reloader share it.
/// The AOBs come from the Game Link offsets (worldReloadMethod "EldenRing"; GameLinkOffsetEntry.world*).
///
/// MoveMapStep's save/load handler (0x140AFD060 in the 1.16 Steam build) loads the save slot in GameMan +LoadSlot when
/// it isn't -1, through RequestLoad (0x14067C500) and LoadGame (0x14067C590), which restarts the world from that save.
/// The game only ever resets that field, so this looks like a leftover developer path.
/// Before it, a save request (GameMan +SaveRequest) writes the current state, so the reload loses nothing.
///
/// ER's AOBs: the save request getter (mov rax,[GameMan]; movzx ebx,byte ptr [rax+SaveRequest]; call; xor edi,edi;
/// test al,al; cmovne ebx,edi); the load slot getter (mov rax,[GameMan]; mov eax,[rax+LoadSlot]; ret) 0x1A before it;
/// the current slot getter (mov rax,[GameMan]; mov eax,[rax+0AC0h]; ret; the 0x0AC0 pins it among such getters).
/// </summary>
public class EldenRingWorldReload
{
    public const string Method = "EldenRing";

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
    /// Finds GameMan and its save request, load slot and current slot fields with one offset group. Optional for the
    /// caller: without them, <see cref="CanReload"/> is false. Called with the game's lock held.
    /// The problem is for the log.
    /// </summary>
    public bool Resolve(IGameProcess game, GameLinkOffsetEntry g, out string problem)
    {
        GameManGlobal = 0;
        problem = "";

        var saveRel = GameLinkOffsets.ParseRelative(g.worldSaveRequestGameManRelativeOffset);
        var saveField = GameLinkOffsets.ParseHex(g.worldSaveRequestFieldOffset);
        var loadBefore = GameLinkOffsets.ParseHex(g.worldLoadSlotBeforeSave);
        var loadRel = GameLinkOffsets.ParseRelative(g.worldLoadSlotGameManRelativeOffset);
        var loadField = GameLinkOffsets.ParseHex(g.worldLoadSlotFieldOffset);
        var currentRel = GameLinkOffsets.ParseRelative(g.worldCurrentSlotGameManRelativeOffset);
        var currentField = GameLinkOffsets.ParseHex(g.worldCurrentSlotFieldOffset);

        if (saveRel == null || saveField == null || loadBefore == null || loadRel == null || loadField == null ||
            currentRel == null || currentField == null || string.IsNullOrWhiteSpace(g.worldSaveRequestAob) ||
            string.IsNullOrWhiteSpace(g.worldCurrentSlotAob) || string.IsNullOrWhiteSpace(g.worldLoadSlotPrologue))
        {
            problem = "incomplete offsets";
            return false;
        }

        var saves = game.Scan(g.worldSaveRequestAob);
        var currents = game.Scan(g.worldCurrentSlotAob);

        if (saves.Count != 1 || currents.Count != 1)
        {
            problem = $"save request {saves.Count}, current slot {currents.Count} match(es)";
            return false;
        }

        var save = saves[0];
        var load = save - loadBefore.Value;

        if (!GameLink.Matches(game, load, g.worldLoadSlotPrologue))
        {
            problem = "unexpected load slot getter";
            return false;
        }

        var gameMan = GameLink.Relative(game, save, saveRel.Value);

        // All three must read the same GameMan
        if (GameLink.Relative(game, load, loadRel.Value) != gameMan || GameLink.Relative(game, currents[0], currentRel.Value) != gameMan)
        {
            problem = "the getters read different globals";
            return false;
        }

        GameManGlobal = gameMan;
        SaveRequestOffset = ReadI32(game, save + saveField.Value);
        LoadSlotOffset = ReadI32(game, load + loadField.Value);
        CurrentSlotOffset = ReadI32(game, currents[0] + currentField.Value);

        Smithbox.Log(typeof(EldenRingWorldReload),
            $"[World Reload] GameMan global 0x{GameManGlobal:X}, save request +0x{SaveRequestOffset:X}, load slot +0x{LoadSlotOffset:X}, current slot +0x{CurrentSlotOffset:X}");

        return true;
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
                    LastMessage = LOC.Get("GAMELINK_World_Unavailable");
                    return;
                }

                gameMan = ReadPtr(game, GameManGlobal);
                slot = gameMan != 0 ? ReadI32(game, gameMan + CurrentSlotOffset) : -1;

                if (slot < 0 || slot > 9)
                {
                    LastMessage = LOC.Get("GAMELINK_World_No_Save");
                    return;
                }

                if (ReadI32(game, gameMan + LoadSlotOffset) != -1)
                {
                    LastMessage = LOC.Get("GAMELINK_World_Busy");
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

                LastMessage = LOC.Get("GAMELINK_World_Save_Not_Taken");
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

                LastMessage = LOC.Get("GAMELINK_World_Load_Not_Taken");
                Smithbox.Log(typeof(EldenRingWorldReload), "[World Reload] Load not taken within 10 s; request withdrawn.");
                return;
            }

            LastMessage = LOC.Get("GAMELINK_World_Done", DateTime.Now.ToString("T"));
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
