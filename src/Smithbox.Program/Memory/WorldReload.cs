using StudioCore.Application;

namespace StudioCore.Memory;

/// <summary>
/// Reload World for a running ER, NR or AC6: optionally saves, then loads a save slot again without the title screen.
/// Owned by GameLink, so the Material Reloader and the Map Editor's World Reloader share it.
/// The AOBs come from the Game Link offsets (worldReloadMethod "LoadSlot"; GameLinkOffsetEntry.world*).
///
/// The world step's save/load handler (ER 0x140AFD060, NR 0x140C383C0, AC6 0x141272440) loads the save slot in
/// GameMan +LoadSlot when it isn't -1, through RequestLoad (ER 0x14067C500, NR 0x1406B15C0, AC6 0x140B91170) and
/// LoadGame, which restarts the world from that save. The games only ever reset that field, so this looks like a
/// leftover developer path. See Claude/Documentation/Materials and Textures.md (Material Reloader) §5d and §5g.
///
/// - Save first (ER, NR): a save request byte (GameMan +SaveRequest) writes the current state, so the reload loses
///   nothing. AC6 has no such byte (its saves go through a request code), so it loads the last save as it is.
/// - The slot: the current one (GameMan +CurrentSlot, which LoadGame sets) or a fixed one (AC6: RequestLoad only
///   accepts slot 0).
/// </summary>
public class WorldReload
{
    public const string Method = "LoadSlot";

    private const int TimeoutMs = 10000;

    private nint GameManGlobal;
    private int LoadSlotOffset;
    private int? SaveRequestOffset, CurrentSlotOffset, FixedSlot;

    /// <summary>
    /// Whether the world reload request was found in this game version
    /// </summary>
    public bool CanReload => GameManGlobal != 0;

    /// <summary>Whether a reload saves first (false: it loads the last save, losing progress since then)</summary>
    public bool SavesFirst => SaveRequestOffset != null;

    /// <summary>The outcome of the last reload, for the UI</summary>
    public volatile string LastMessage;

    public volatile bool IsRunning;

    /// <summary>
    /// Forgets the resolved addresses, when the connection closes
    /// </summary>
    public void Clear()
    {
        GameManGlobal = 0;
        SaveRequestOffset = CurrentSlotOffset = FixedSlot = null;
    }

    /// <summary>
    /// Finds GameMan and its load slot field, and the save request and current slot fields when the group has them,
    /// with one offset group. Optional for the caller: without them, <see cref="CanReload"/> is false.
    /// Called with the game's lock held. The problem is for the log.
    /// </summary>
    public bool Resolve(IGameProcess game, GameLinkOffsetEntry g, out string problem)
    {
        Clear();
        problem = "";

        // Load slot (required)
        if (!Field(game, g.worldLoadSlotAob, g.worldLoadSlotGameManRelativeOffset, g.worldLoadSlotFieldOffset,
            "load slot", out var gameMan, out var loadSlot, ref problem))
            return false;

        // Save request (optional)
        int? saveRequest = null;
        if (!string.IsNullOrWhiteSpace(g.worldSaveRequestAob))
        {
            if (!Field(game, g.worldSaveRequestAob, g.worldSaveRequestGameManRelativeOffset, g.worldSaveRequestFieldOffset,
                "save request", out var saveGameMan, out var saveField, ref problem))
                return false;

            if (saveGameMan != gameMan)
            {
                problem = "the save request reads a different global";
                return false;
            }

            saveRequest = saveField;
        }

        // Slot: the current one, or a fixed one
        int? currentSlot = null, fixedSlot = null;
        if (!string.IsNullOrWhiteSpace(g.worldCurrentSlotAob))
        {
            if (!Field(game, g.worldCurrentSlotAob, g.worldCurrentSlotGameManRelativeOffset, g.worldCurrentSlotFieldOffset,
                "current slot", out var currentGameMan, out var currentField, ref problem))
                return false;

            if (currentGameMan != gameMan)
            {
                problem = "the current slot reads a different global";
                return false;
            }

            currentSlot = currentField;
        }
        else
        {
            fixedSlot = GameLinkOffsets.ParseHex(g.worldFixedSlot);

            if (fixedSlot == null)
            {
                problem = "neither a current slot nor a fixed slot";
                return false;
            }
        }

        GameManGlobal = gameMan;
        LoadSlotOffset = loadSlot;
        SaveRequestOffset = saveRequest;
        CurrentSlotOffset = currentSlot;
        FixedSlot = fixedSlot;

        Smithbox.Log(typeof(WorldReload),
            $"[World Reload] GameMan global 0x{GameManGlobal:X}, load slot +0x{LoadSlotOffset:X}, " +
            $"save request {(SaveRequestOffset is { } s ? $"+0x{s:X}" : "none")}, " +
            $"slot {(CurrentSlotOffset is { } c ? $"current +0x{c:X}" : $"fixed {FixedSlot}")}");

        return true;
    }

    /// <summary>
    /// A GameMan field from a getter AOB: the global's rel32 and where the i32 field offset sits in the match
    /// </summary>
    private static bool Field(IGameProcess game, string aob, string globalRelative, string fieldAt, string what,
        out nint global, out int field, ref string problem)
    {
        global = 0;
        field = 0;

        var rel = GameLinkOffsets.ParseRelative(globalRelative);
        var at = GameLinkOffsets.ParseHex(fieldAt);

        if (string.IsNullOrWhiteSpace(aob) || rel == null || at == null)
        {
            problem = $"incomplete {what} offsets";
            return false;
        }

        var hits = game.Scan(aob);
        if (hits.Count != 1)
        {
            problem = $"{what} {hits.Count} match(es)";
            return false;
        }

        global = GameLink.Relative(game, hits[0], rel.Value);
        field = ReadI32(game, hits[0] + at.Value);
        return true;
    }

    /// <summary>
    /// Saves (where the game can), then asks the world step to load the save slot again, waiting for the game to take
    /// each request. Each request is withdrawn if the game doesn't take it, so nothing is left to fire later. Blocks for
    /// up to 20 s, so call it from a background task; it takes the game's lock itself.
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
                slot = gameMan == 0 ? -1 : CurrentSlotOffset is { } current ? ReadI32(game, gameMan + current) : FixedSlot ?? -1;

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
                if (SaveRequestOffset is { } save)
                    game.WriteBytes(gameMan + save, new byte[] { 1 });
            }

            if (SaveRequestOffset is { } saveOffset)
            {
                Smithbox.Log(typeof(WorldReload), $"[World Reload] Save requested (slot {slot}).");

                var saved = WaitFor(game, () => game.ReadBytes(gameMan + saveOffset, 1)[0] == 0);
                if (saved == null)
                    return;

                if (saved == false)
                {
                    lock (game.Lock)
                    {
                        if (game.IsOpen)
                            game.WriteBytes(gameMan + saveOffset, new byte[] { 0 });
                    }

                    LastMessage = LOC.Get("GAMELINK_World_Save_Not_Taken");
                    Smithbox.Log(typeof(WorldReload), "[World Reload] Save not taken within 10 s; nothing reloaded.");
                    return;
                }
            }

            lock (game.Lock)
            {
                if (!game.IsOpen)
                    return;

                game.WriteBytes(gameMan + LoadSlotOffset, BitConverter.GetBytes(slot));
            }

            Smithbox.Log(typeof(WorldReload), $"[World Reload] Load of slot {slot} requested.");

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
                Smithbox.Log(typeof(WorldReload), "[World Reload] Load not taken within 10 s; request withdrawn.");
                return;
            }

            LastMessage = LOC.Get(SavesFirst ? "GAMELINK_World_Done" : "GAMELINK_World_Done_No_Save", DateTime.Now.ToString("T"));
            Smithbox.Log(typeof(WorldReload), "[World Reload] Load accepted.");
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
